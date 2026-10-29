using System;
using System.Collections.Generic;
using System.Diagnostics;
using Beam.Rendering;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using MEC;
using SharpNeatLib.Maths;
using Sirenix.Utilities;
using UnityEngine;

namespace Beam.Terrain;

public class ZoneLoader : ILoader
{
	public delegate void GenerationCallback(IGenerationObject obj, Vector3 position, Quaternion rotation, Zone zone);

	public delegate void ProcessCollectionCallback<in T>(T obj, Zone zone);

	public enum EObjectProcessingMode
	{
		Stepped,
		Faster
	}

	private const int MIN_PROCESSING_STEPS_PER_FRAME = 4;

	private const int MAX_PROCESSING_STEPS_PER_FRAME = 200;

	private const int INDEX_SIZE = 256;

	[SerializeField]
	private bool _simulateSlowLoad;

	private List<Action> _internalDeferredLoading = new List<Action>();

	private List<Action> _deferredLoading = new List<Action>();

	private List<Action> _postLoading = new List<Action>();

	private List<Action> _physicalLoading = new List<Action>();

	private bool _isGenerating;

	private FastRandom _random = new FastRandom();

	private Vector3 _position;

	private int _processes;

	private Stopwatch _timer = new Stopwatch();

	private float _timeout;

	public IList<Action> DeferredLoading => _deferredLoading;

	public IList<Action> PostLoading => _postLoading;

	public IList<Action> PhysicalLoading => _physicalLoading;

	public bool IsGenerating => _isGenerating;

	public bool IsLoading { get; set; }

	public FastRandom Random => _random;

	public EObjectProcessingMode ObjectProcessingMode { get; set; }

	public event Action<Zone> CreatedZone;

	public event Action<Zone> LoadedZone;

	private void OnDestroy()
	{
		this.CreatedZone = null;
		this.LoadedZone = null;
	}

	public float LoadZone_Async(Zone zone, JObject data)
	{
		return Timing.WaitUntilDone(LoadZone(zone, data));
	}

	public CoroutineHandle LoadZone(Zone zone, JObject data)
	{
		return Timing.RunCoroutine(LoadZone_Sequence(zone, data), Segment.Update, 100);
	}

	private IEnumerator<float> LoadZone_Sequence(Zone zone, JObject data)
	{
		_timer.Restart();
		UpdateSessionToken_IsBusy(isBusy: true);
		LevelLoader.Instance.LoadingZones.Add(zone);
		LevelLoader.Instance.HashCodes.Clear();
		LevelLoader.CurrentLoader = this;
		LevelLoader.CurrentLoader.IsLoading = true;
		ResetTemporaryLoadingPosition();
		zone.Loading = true;
		zone.Enter();
		_timeout = Peer.GetDefaultTimeout();
		if (Game.Mode.IsMultiplayer())
		{
			_timeout = PlayerRegistry.LocalPeer.Timeout;
			yield return PlayerRegistry.LocalPeer.SetTimeout_Async(Peer.GetSessionTimeout());
		}
		JObject field = data.GetField("Attributes");
		if (field != null)
		{
			SavingUtilities.Load(zone.Attributes, field);
		}
		_processes = 4;
		JObject field2 = data.GetField("Objects");
		if (field2 != null && !field2.IsNull())
		{
			yield return ProcessCollection_Async(field2.Children, CreateLoadedPrefab, zone);
			if (Game.Mode.IsClient())
			{
				yield return Timing.WaitUntilDone(Timing.RunCoroutine(ClientLinking()));
			}
		}
		else
		{
			UnityEngine.Debug.LogError("ZoneLoader::LoadZoneAsync:: Zone [" + zone.ZoneName + "] [Objects] field is null or empty.");
		}
		yield return 0f;
		yield return 0f;
		yield return Timing.WaitUntilDone(MultiplayerFence.Sync(Fence.ProcessLoading));
		yield return ProcessCollection_Async(_internalDeferredLoading, InvokeCallback, zone);
		yield return ProcessCollection_Async(_deferredLoading, InvokeCallback, zone);
		yield return ProcessCollection_Async(_postLoading, InvokeCallback, zone);
		if (AssetBundleManager.IsLoading)
		{
			UnityEngine.Debug.Log("ZoneLoader::LoadZoneAsync:: [" + zone.ZoneName + "] waiting for bundles to load...");
		}
		while (AssetBundleManager.IsLoading)
		{
			yield return 0f;
		}
		yield return 0f;
		for (int i = 0; i < _physicalLoading.Count; i++)
		{
			LoadingActionUtilities.ExecuteDeferredAction(_physicalLoading[i]);
		}
		zone.Loading = false;
		zone.Loaded = true;
		this.LoadedZone?.Invoke(zone);
		zone.HasVisited = true;
		CleanUp(zone);
		if (Game.Mode.IsMaster())
		{
			Singleton<StrandedWorld>.Instance.HandleOutOfGameBounds(zone, handleNmlContainer: true);
		}
		if (Game.Mode.IsMultiplayer())
		{
			PlayerRegistry.LocalPeer.SetTimeout(_timeout);
		}
		UpdateSessionToken_IsBusy(isBusy: false);
	}

	public void AbortLoadingZone(Zone zone)
	{
		Timing.KillCoroutines(100);
		BeamTeam.DestroyAllChildrenImmediate(zone.SaveContainer);
		Transform parent = Singleton<StrandedWorld>.Instance.GetParent();
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			Transform child = parent.GetChild(num);
			if (Singleton<StrandedWorld>.Instance.InBounds(child.position))
			{
				BeamTeam.DestroyImmediate(child.gameObject);
			}
		}
		zone.Exit();
		zone.Loading = false;
		zone.Loaded = false;
		_ = _isGenerating;
		CleanUp(zone);
		if (Game.Mode.IsMultiplayer())
		{
			PlayerRegistry.LocalPeer.SetTimeout(_timeout);
		}
		UpdateSessionToken_IsBusy(isBusy: false);
	}

	private void CleanUp(Zone zone)
	{
		_timer.Stop();
		_isGenerating = false;
		if (!LevelLoader.CurrentLoader.IsNullOrDestroyed())
		{
			_internalDeferredLoading.Clear();
			_deferredLoading.Clear();
			_postLoading.Clear();
			_physicalLoading.Clear();
			LevelLoader.CurrentLoader.IsLoading = false;
			LevelLoader.CurrentLoader = null;
			LevelLoader.Instance.HashCodes.Clear();
			LevelLoader.Instance.LoadingZones.Remove(zone);
		}
	}

	public bool CreateZone(Zone zone)
	{
		if (_isGenerating)
		{
			return false;
		}
		CreateZoneObjects(zone);
		return true;
	}

	public float CreateZoneObjects_Async(Zone zone, int seed)
	{
		return Timing.WaitUntilDone(CreateZoneObjects(zone));
	}

	private CoroutineHandle CreateZoneObjects(Zone zone)
	{
		return Timing.RunCoroutine(CreateZoneObjects_Sequence(zone), Segment.Update, 100);
	}

	private IEnumerator<float> CreateZoneObjects_Sequence(Zone zone)
	{
		_timer.Restart();
		UpdateSessionToken_IsBusy(isBusy: true);
		_isGenerating = true;
		zone.Loading = true;
		zone.Enter();
		_processes = 1;
		_timeout = Peer.GetDefaultTimeout();
		if (Game.Mode.IsMultiplayer())
		{
			_timeout = PlayerRegistry.LocalPeer.Timeout;
			yield return PlayerRegistry.LocalPeer.SetTimeout_Async(Peer.GetSessionTimeout());
		}
		yield return ProcessCollection_Async(zone.GeneratedObjects, CreateGeneratedPrefab, zone);
		if (Game.Mode.IsClient())
		{
			yield return Timing.WaitUntilDone(Timing.RunCoroutine(ClientLinking()));
		}
		zone.Loading = false;
		zone.Loaded = true;
		this.CreatedZone?.Invoke(zone);
		zone.HasVisited = true;
		CleanUp(zone);
		_isGenerating = false;
		if (Game.Mode.IsMultiplayer())
		{
			PlayerRegistry.LocalPeer.SetTimeout(_timeout);
		}
		UpdateSessionToken_IsBusy(isBusy: false);
	}

	public bool GenerateZone(Zone zone)
	{
		if (_isGenerating)
		{
			return false;
		}
		GenerateObjects(zone, GenerateObject);
		return true;
	}

	private void GenerateObjects(Zone zone, GenerationCallback generationCallback)
	{
		_isGenerating = true;
		_random.Reinitialise(StrandedWorld.WORLD_SEED - zone.Seed);
		zone.GenerateGridData();
		zone.GeneratedObjects.Clear();
		ZoneObjects sheltersGeneration = Singleton<StrandedWorld>.Instance.ProceduralObjectsManager.SheltersGeneration;
		if (zone.HasShelter)
		{
			GenerateShelter(sheltersGeneration, zone, generationCallback);
		}
		for (int i = 0; i < Singleton<StrandedWorld>.Instance.ProceduralObjectsManager.BiomeGeneration.Count; i++)
		{
			BiomeCategory biomeCategory = Singleton<StrandedWorld>.Instance.ProceduralObjectsManager.BiomeGeneration[i];
			if (biomeCategory.Biome != zone.Biome && biomeCategory.Biome != 0)
			{
				continue;
			}
			for (int j = 0; j < biomeCategory.ProceduralObjects.Count; j++)
			{
				ZoneObjects zoneObjects = biomeCategory.ProceduralObjects[j];
				switch (zoneObjects.generationType)
				{
				case ZoneGenerationType.Position:
					GenerateProceduralObjectsPosition(zoneObjects, zone, generationCallback);
					break;
				case ZoneGenerationType.Procedural:
					GenerateProceduralObjectsProcedural(zoneObjects, zone, generationCallback);
					break;
				}
			}
		}
		if (zone.Biome == Zone.BiomeType.ISLAND || zone.Biome == Zone.BiomeType.ISLAND_SMALL)
		{
			for (int k = 0; k < Singleton<StrandedWorld>.Instance.ProceduralObjectsManager.ObjectGeneration.Count; k++)
			{
				ObjectCategory objectCategory = Singleton<StrandedWorld>.Instance.ProceduralObjectsManager.ObjectGeneration[k];
				for (int l = 0; l < objectCategory.ProceduralObjects.Count; l++)
				{
					ZoneObjects zoneObjects2 = objectCategory.ProceduralObjects[l];
					switch (zoneObjects2.generationType)
					{
					case ZoneGenerationType.Position:
						GenerateProceduralObjectsPosition(zoneObjects2, zone, generationCallback);
						break;
					case ZoneGenerationType.Procedural:
						GenerateProceduralObjectsProcedural(zoneObjects2, zone, generationCallback);
						break;
					}
				}
			}
		}
		zone.GeneratedObjects.Sort((GeneratedObject a, GeneratedObject b) => Comparisons.FurthestFromPoint(zone.transform.position, a.Position, b.Position));
		_isGenerating = false;
	}

	private void GenerateProceduralObjectsPosition(ZoneObjects zoneObjects, Zone zone, GenerationCallback generationCallback)
	{
		if (zoneObjects.Objects == null || zoneObjects.Objects.Length == 0)
		{
			UnityEngine.Debug.LogError("ZoneObjectGenerator::GenerateProceduralObjectsPosition:: Objects to generate was null or empty.");
			return;
		}
		_random.Reinitialise(StrandedWorld.WORLD_SEED - zone.Seed);
		float num = 0f;
		if (zoneObjects.Objects.Length != 0)
		{
			for (int i = 0; i < zoneObjects.Objects.Length; i++)
			{
				ZoneObject zoneObject = zoneObjects.Objects[i];
				num += zoneObject.Rarity;
			}
		}
		if (zone.IsStartingIsland && !zoneObjects.spawnOnStartingIsland)
		{
			return;
		}
		for (int j = 0; j < zoneObjects.maxObjectCount; j++)
		{
			if (!((float)_random.NextDouble() < (float)zoneObjects.spawnChance / 100f))
			{
				continue;
			}
			int num2 = 0;
			if (zoneObjects.Objects.Length != 0)
			{
				float num3 = (float)_random.NextDouble() * num;
				for (int k = 0; k < zoneObjects.Objects.Length; k++)
				{
					if (num3 < zoneObjects.Objects[k].Rarity)
					{
						num2 = k;
						break;
					}
					num3 -= zoneObjects.Objects[k].Rarity;
				}
			}
			Vector3 position = zone.transform.position + zoneObjects.manualPosition;
			Quaternion identity = Quaternion.identity;
			IGenerationObject obj = zoneObjects.Objects[num2].Obj;
			generationCallback(obj, position, identity, zone);
		}
	}

	private void GenerateProceduralObjectsProcedural(ZoneObjects zoneObjects, Zone zone, GenerationCallback generationCallback)
	{
		if (zoneObjects.Objects == null || zoneObjects.Objects.Length == 0)
		{
			UnityEngine.Debug.LogError("ZoneLoader::GenerateProceduralObjectsProcedural:: Objects to generate was null or empty.");
			return;
		}
		_random.Reinitialise(StrandedWorld.WORLD_SEED - zone.Seed);
		float num = 0f;
		if (zoneObjects.Objects.Length != 0)
		{
			for (int i = 0; i < zoneObjects.Objects.Length; i++)
			{
				ZoneObject zoneObject = zoneObjects.Objects[i];
				num += zoneObject.Rarity;
			}
		}
		if (zone.IsStartingIsland && !zoneObjects.spawnOnStartingIsland)
		{
			return;
		}
		int detailAmountFactor = zoneObjects.detailAmountFactor;
		int num2 = 0;
		for (int j = 0; j < 256 - detailAmountFactor; j += detailAmountFactor)
		{
			for (int k = 0; k < 256 - detailAmountFactor; k += detailAmountFactor)
			{
				if (num2 >= zoneObjects.maxObjectCount)
				{
					return;
				}
				GenerationPoints generationPoints = zone.GenerationPoints[j, k];
				if (!((float)_random.NextDouble() < (float)zoneObjects.spawnChance / 100f) || ((!zoneObjects.occupyPoint || generationPoints.Occupied) && zoneObjects.occupyPoint) || !(generationPoints.Position.y >= zoneObjects.minHeight) || !(generationPoints.Position.y < zoneObjects.maxHeight) || !(generationPoints.Soilmap >= zoneObjects.minStrength) || !(generationPoints.Soilmap < zoneObjects.maxStrength) || !(generationPoints.steepNess < zoneObjects.maxSteepness) || !(generationPoints.steepNess > zoneObjects.minSteepness))
				{
					continue;
				}
				int num3 = 0;
				if (zoneObjects.Objects.Length != 0)
				{
					float num4 = (float)_random.NextDouble() * num;
					for (int l = 0; l < zoneObjects.Objects.Length; l++)
					{
						if (num4 < zoneObjects.Objects[l].Rarity)
						{
							num3 = l;
							break;
						}
						num4 -= zoneObjects.Objects[l].Rarity;
					}
				}
				Vector3 vector = new Vector3((float)_random.Next((int)(zoneObjects.minPositionOffset.x * 100f), (int)(zoneObjects.maxPositionOffset.x * 100f)) / 100f, (float)_random.Next((int)(zoneObjects.minPositionOffset.y * 100f), (int)(zoneObjects.maxPositionOffset.y * 100f)) / 100f, (float)_random.Next((int)(zoneObjects.minPositionOffset.z * 100f), (int)(zoneObjects.maxPositionOffset.z * 100f)) / 100f);
				Vector3 position = generationPoints.Position + zoneObjects.offsetPosition + vector;
				Quaternion quaternion = Quaternion.identity;
				if (zoneObjects.terrainSnap)
				{
					quaternion = Quaternion.Lerp(Quaternion.identity, generationPoints.Rotation, zoneObjects.TerrainSnapBias);
				}
				int num5 = _random.Next(0, 360);
				if (zoneObjects.randomRotation)
				{
					quaternion = (zoneObjects.terrainSnap ? (quaternion * Quaternion.Euler(Vector3.up * num5)) : Quaternion.Euler(Vector3.up * num5));
				}
				else if (zoneObjects.randomFullRotation)
				{
					int num6 = _random.Next(0, 360);
					int num7 = _random.Next(0, 360);
					quaternion = (zoneObjects.terrainSnap ? (quaternion * Quaternion.Euler(new Vector3(num6, num5, num7))) : Quaternion.Euler(new Vector3(num6, num5, num7)));
				}
				if (zoneObjects.occupyPoint)
				{
					generationPoints.Occupied = true;
				}
				IGenerationObject obj = zoneObjects.Objects[num3].Obj;
				generationCallback(obj, position, quaternion, zone);
				if (zoneObjects.maxObjectCount != 0)
				{
					num2++;
				}
			}
		}
	}

	private void GenerateShelter(ZoneObjects zoneObjects, Zone zone, GenerationCallback generationCallback)
	{
		if (!zone.HasShelter)
		{
			return;
		}
		int shelterType = zone.ShelterType;
		UnityEngine.Debug.Log(">>Generating shelter type : " + zoneObjects.Objects[shelterType].Obj.gameObject.name + ", on Zone :" + zone.ZoneName);
		int detailAmountFactor = zoneObjects.detailAmountFactor;
		int num = 0;
		int num2 = ((shelterType == 7) ? 8 : ((int)Mathf.Sqrt(zoneObjects.detailAmountFactor)));
		int num3 = 257;
		int num4 = 100;
		int num5 = num3 * num3 % num4;
		int num6 = 0;
		int num7 = 0;
		_ = num3 * num3 / num4;
		if (num3 * num3 % num4 <= 0)
		{
		}
		while (num6 < num3 * num3 && num < 1)
		{
			num7 = 0;
			int num8 = num6 % num4;
			int num9 = num6 / num4;
			num6++;
			for (int i = 0; i < num8; i++)
			{
				num7 = ((i >= num5) ? (num7 + num3 * num3 / detailAmountFactor) : (num7 + (num3 * num3 / num4 + 1)));
			}
			num7 += num9;
			if (num7 >= num3 * num3)
			{
				continue;
			}
			int num10 = num7 / num3;
			int num11 = num7 % num3;
			if (num10 >= num3 - 1 || num11 >= num3 - 1 || !((float)_random.NextDouble() < (float)zoneObjects.spawnChance / 100f))
			{
				continue;
			}
			GenerationPoints generationPoints = zone.GenerationPoints[num10, num11];
			bool flag = false;
			for (int j = num10 - num2; j <= num10 + num2; j++)
			{
				if (j < 0 || j >= num3 - 1)
				{
					continue;
				}
				for (int k = num11 - num2; k <= num11 + num2; k++)
				{
					if (k >= 0 && k < num3 - 1 && zone.GenerationPoints[j, k].Occupied)
					{
						flag = true;
					}
				}
			}
			if (flag || ((!zoneObjects.occupyPoint || generationPoints.Occupied) && zoneObjects.occupyPoint) || !(generationPoints.Position.y >= zoneObjects.minHeight) || !(generationPoints.Position.y < zoneObjects.maxHeight) || !(generationPoints.Soilmap >= zoneObjects.minStrength) || !(generationPoints.Soilmap < zoneObjects.maxStrength) || !(generationPoints.steepNess < zoneObjects.maxSteepness) || !(generationPoints.steepNess > zoneObjects.minSteepness))
			{
				continue;
			}
			Vector3 vector = new Vector3((float)_random.Next((int)(zoneObjects.minPositionOffset.x * 100f), (int)(zoneObjects.maxPositionOffset.x * 100f)) / 100f, (float)_random.Next((int)(zoneObjects.minPositionOffset.y * 100f), (int)(zoneObjects.maxPositionOffset.y * 100f)) / 100f, (float)_random.Next((int)(zoneObjects.minPositionOffset.z * 100f), (int)(zoneObjects.maxPositionOffset.z * 100f)) / 100f);
			Vector3 position = generationPoints.Position + zoneObjects.offsetPosition + vector;
			Quaternion quaternion = Quaternion.identity;
			if (zoneObjects.terrainSnap)
			{
				quaternion = Quaternion.Lerp(Quaternion.identity, generationPoints.Rotation, zoneObjects.TerrainSnapBias);
			}
			int num12 = _random.Next(0, 360);
			if (zoneObjects.randomRotation)
			{
				quaternion = (zoneObjects.terrainSnap ? (quaternion * Quaternion.Euler(Vector3.up * num12)) : Quaternion.Euler(Vector3.up * num12));
			}
			else if (zoneObjects.randomFullRotation)
			{
				int num13 = _random.Next(0, 360);
				int num14 = _random.Next(0, 360);
				quaternion = (zoneObjects.terrainSnap ? (quaternion * Quaternion.Euler(new Vector3(num13, num12, num14))) : Quaternion.Euler(new Vector3(num13, num12, num14)));
			}
			if (zoneObjects.occupyPoint)
			{
				generationPoints.Occupied = true;
				for (int l = num10 - num2; l <= num10 + num2; l++)
				{
					if (l < 0 || l >= num3 - 1)
					{
						continue;
					}
					for (int m = num11 - num2; m <= num11 + num2; m++)
					{
						if (m >= 0 && m < num3 - 1)
						{
							zone.GenerationPoints[l, m].Occupied = true;
						}
					}
				}
			}
			IGenerationObject obj = zoneObjects.Objects[shelterType].Obj;
			generationCallback(obj, position, quaternion, zone);
			num++;
		}
	}

	private void GenerateObject(IGenerationObject generationObject, Vector3 position, Quaternion rotation, Zone zone)
	{
		GeneratedObject generatedObject = new GeneratedObject();
		generatedObject.Prefab = generationObject.gameObject;
		generatedObject.Position = position;
		generatedObject.Rotation = rotation;
		generatedObject.ImpostorParentPrefab = generationObject.gameObject.GetInterface<IImpostorParent>();
		zone.OnObjectGenerated(generatedObject);
	}

	private void CreateGeneratedPrefab(GeneratedObject objectData, Zone zone)
	{
		SaveablePrefab component = objectData.Prefab.GetComponent<SaveablePrefab>();
		GameObject gameObject;
		if (component != null)
		{
			uint prefabId = component.PrefabId;
			MiniGuid referenceId = MiniGuid.NewFrom(objectData.Position, prefabId, 48879);
			if (Game.Mode.IsClient() && component.IsMultiplayerEntity)
			{
				MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.ZoneGeneration, referenceId, ClientSetParent, null, zone);
				return;
			}
			gameObject = MultiplayerMng.Instantiate<SaveablePrefab>(prefabId, objectData.Position, objectData.Rotation, referenceId).gameObject;
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(objectData.Prefab, objectData.Position, objectData.Rotation);
		}
		gameObject.transform.SetParent(zone.SaveContainer, worldPositionStays: false);
		gameObject.transform.position = objectData.Position;
		gameObject.transform.rotation = objectData.Rotation;
		zone.OnObjectCreated(gameObject);
	}

	private void CreateLoadedPrefab(JObject data, Zone zone)
	{
		JObject field = data.GetField("prefab");
		if (field == null)
		{
			UnityEngine.Debug.LogError("ZoneLoader::LoadZoneAsync:: Error loading. No prefab id field found for object data.");
			return;
		}
		uint num = Prefabs.ConvertLegacyPrefabId(field);
		if (num == 0)
		{
			UnityEngine.Debug.LogError("ZoneLoader::LoadZoneAsync:: Error loading instantiated object with empty prefab id.");
			return;
		}
		string multiplayerPrefabName;
		bool flag = Prefabs.TryGetMultiplayerPrefabName(num, out multiplayerPrefabName);
		if (Game.Mode.IsClient() && flag)
		{
			MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.ZoneLoading, Prefabs.GetReferenceId(data), ClientLoad, data, zone);
		}
		else
		{
			Load(data, _position, zone, MultiplayerMng.Instantiate<SaveablePrefab>(num, Prefabs.GetReferenceId(data)));
		}
		_position += new Vector3(0f, 0f, 50f);
	}

	private void Load(JObject data, Vector3 position, Zone zone, SaveablePrefab instance)
	{
		LevelLoader.Instance.HashCodes.Add(instance.gameObject.GetHashCode());
		instance.transform.SetParent(zone.SaveContainer, worldPositionStays: false);
		instance.transform.position = position;
		string prefabName = instance.name;
		_internalDeferredLoading.Add(delegate
		{
			if (instance.gameObject == null)
			{
				UnityEngine.Debug.LogError("ZoneLoader::LoadZoneAsync:: Error loading prefab [" + prefabName + "]->Object was modified before loading began.\nCheck the object's Awake, Start, Update and FixedUpdate methods for missing [LevelLoader.IsLoading] checks.");
			}
			else
			{
				if (zone.IsMapEditor && !zone.HasVisited)
				{
					try
					{
						JSerializer.DeserializeInto(data.GetField("Transform"), instance.gameObject.transform);
						instance.CompleteTransformSerialization();
						instance.OnCustomMapLoaded();
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogError("ZoneLoader::LoadZoneAsync:: Error loading editor object [" + instance.name + "] Object has now been destroyed.\n\n" + ex);
						MultiplayerMng.Destroy(instance, DestroyDelay.LATE);
					}
				}
				else if (!SavingUtilities.Load(instance, data))
				{
					UnityEngine.Debug.LogError($"ZoneLoader::LoadZoneAsync:: Error loading [{instance.name}] [{instance.ReferenceId}]->Object has now been destroyed.");
					MultiplayerMng.Destroy(instance, DestroyDelay.LATE);
				}
				if (instance.gameObject != null)
				{
					zone.OnObjectLoaded(instance.gameObject);
				}
			}
		});
	}

	private void ClientLoad(MultiplayerZoneSync.Command command)
	{
		Load(command.Data, Vector3.zero, command.Zone, command.Instance);
	}

	private void ClientSetParent(MultiplayerZoneSync.Command command)
	{
		command.Instance.transform.SetParent(command.Zone.SaveContainer);
	}

	private void InvokeCallback(Action action, Zone zone)
	{
		LoadingActionUtilities.ExecuteDeferredAction(action);
	}

	private void ResetTemporaryLoadingPosition()
	{
		_position = new Vector3(0f, 5000f, 0f);
	}

	private float ProcessCollection_Async<T>(ICollection<T> collection, ProcessCollectionCallback<T> callback, Zone zone)
	{
		return Timing.WaitUntilDone(ProcessCollection(collection, callback, zone));
	}

	private CoroutineHandle ProcessCollection<T>(ICollection<T> collection, ProcessCollectionCallback<T> callback, Zone zone)
	{
		return Timing.RunCoroutine(ProcessCollection_Sequence(collection, callback, zone), Segment.Update, 100);
	}

	private IEnumerator<float> ProcessCollection_Sequence<T>(ICollection<T> collection, ProcessCollectionCallback<T> callback, Zone zone)
	{
		IEnumerator<T> enumerator = collection.GetEnumerator();
		int index = 0;
		bool stop = false;
		while (!stop)
		{
			_ = collection.Count;
			_ = index;
			int processingSteps = GetProcessingSteps(collection.Count, zone.transform.position);
			for (int i = 0; i < processingSteps; i++)
			{
				if (!enumerator.MoveNext())
				{
					stop = true;
					break;
				}
				callback?.Invoke(enumerator.Current, zone);
				index++;
			}
			yield return float.NegativeInfinity;
		}
		enumerator.Dispose();
	}

	private int GetProcessingSteps(int quantity, Vector3 position)
	{
		int b = ((ObjectProcessingMode == EObjectProcessingMode.Faster) ? 200 : 4);
		Transform closestPlayer = GetClosestPlayer(position);
		if (closestPlayer.IsNullOrDestroyed())
		{
			return 1;
		}
		float num = (position - closestPlayer.position).magnitude - 125f;
		num /= (float)_processes;
		return Mathf.Max(Mathf.CeilToInt((float)quantity / Mathf.Max(1f, num)), b);
	}

	private Transform GetClosestPlayer(Vector3 position)
	{
		if (PlayerRegistry.AllPlayers.IsNullOrDestroyed() || PlayerRegistry.AllPlayers.Count == 0)
		{
			return null;
		}
		IPlayer player = PlayerRegistry.AllPlayers[0];
		if (Game.Mode.GetPlayersCount() == 1)
		{
			return player.transform;
		}
		float num = float.PositiveInfinity;
		for (int i = 1; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			IPlayer player2 = PlayerRegistry.AllPlayers[i];
			float magnitude = (position - player2.transform.position).magnitude;
			if (magnitude < num)
			{
				num = magnitude;
				player = player2;
			}
		}
		return player.transform;
	}

	private void UpdateSessionToken_IsBusy(bool isBusy)
	{
		if (Game.Mode.IsServer())
		{
			if (isBusy && !MultiplayerMng.GetSessionToken().IsBusy)
			{
				MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: true);
			}
			else if (!isBusy && !LevelLoader.IsCreatingOrLoading)
			{
				MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
			}
		}
	}

	private IEnumerator<float> ClientLinking()
	{
		float startTime = Time.unscaledTime;
		bool shutdown = false;
		do
		{
			if (Time.unscaledTime - startTime > MultiplayerZoneSync.CLIENT_LINK_TIME && !shutdown)
			{
				shutdown = true;
				PlayerRegistry.LocalPeer.ReplicatedDisconnect(DisconnectDetail.Timeout_Linking, null, forceLocalShutdown: true);
				MultiplayerZoneSync.Instance.DumpNotCompleted();
			}
			yield return 0f;
		}
		while (!MultiplayerZoneSync.Instance.IsLinked());
	}
}
