using System;
using System.Collections.Generic;
using Beam;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Terrain;
using Beam.Utilities;
using Ceto;
using Funlabs;
using MEC;
using Photon.Bolt;
using UnityEngine;

public class StrandedWorld : Singleton<StrandedWorld>, IDynamicParentProvider, IZoneDataProvider, ITerrainProvider, ICheckBounds, IPersistentSaveable, ISaveable
{
	public const float ZONE_SPACING = 1.25f;

	public const float ZONE_SIZE = 500f;

	public const float ZONE_SIZE_HALF = 250f;

	public const float ZONE_TERRAIN_SIZE = 250f;

	public const float ZONE_TERRAIN_SIZE_HALF = 125f;

	public const float ZONE_TERRAIN_SIZE_HALF_SQR = 15625f;

	private const float ZONE_LOAD_UNLOAD_BUFFER = 10f;

	public const float ZONE_LOAD_DISTANCE = 240f;

	public const float ZONE_UNLOAD_DISTANCE = 250f;

	public const int ZONE_HEIGHTMAP_SIZE = 257;

	public const int ZONE_HEIGHTMAP_HEIGHT = 150;

	public static int LOCAL_WORLD_SEED;

	public const int TERRAIN_Y_POSITION = -100;

	public const int TERRAIN_RESOLUTION = 45;

	public const int TERRAIN_DETAIL_DISTANCE = 70;

	public const int WORLD_ZONES = 7;

	public const int WORLD_ZONES_SQUARED = 49;

	public const int LEGACY_WORLD_ZONES = 5;

	public const float LEGACY_ZONE_SIZE = 630f;

	public const float LEGACY_ZONE_SIZE_HALF = 315f;

	public const float IMPOSTOR_VIEW_DISTANCE = 1000f;

	private const float SQR_IMPOSTOR_VIEW_DISTANCE = 1000000f;

	private const float SQR_LOADING_BUFFER = 1600f;

	private static int[] _shelters = new int[3] { -1, -1, -1 };

	[SerializeField]
	private Zone[] _zones;

	[SerializeField]
	private Zone _nmlZone;

	private TerrainData[] _terrainDatas;

	public ZoneEvents ZoneEvents;

	public ProceduralObjectManager ProceduralObjectsManager;

	private ZoneLoader _zoneLoader = new ZoneLoader();

	private Vector3 _lastSavePosition;

	private Dictionary<string, JObject> _zoneSavedObjectsLookup = new Dictionary<string, JObject>();

	private Vector3 _playerPosition;

	[SerializeField]
	private Zone _zonePrefab;

	[SerializeField]
	private Transform _zoneContainer;

	public static int WORLD_SEED
	{
		get
		{
			if (!Game.Mode.IsMultiplayer())
			{
				return LOCAL_WORLD_SEED;
			}
			return PlayerRegistry.ServerPeer.WorldSeed;
		}
	}

	public Zone[] Zones => _zones;

	public Zone NmlZone => _nmlZone;

	public ZoneLoader ZoneLoader => _zoneLoader;

	public int ShelterCount { get; private set; }

	public event EmptyEventHandler WorldGenerated;

	public event Action<Zone> ZoneEntered;

	public Transform GetParent()
	{
		GameState state = Game.State;
		if ((uint)(state - 2) <= 1u)
		{
			return _nmlZone.SaveContainer;
		}
		return null;
	}

	public JObject GetZoneData(Zone zone)
	{
		JObject value = null;
		_zoneSavedObjectsLookup.TryGetValue(zone.gameObject.name, out value);
		return value;
	}

	public float GetHeight(Vector3 worldPosition)
	{
		Zone zone = GetZone(worldPosition);
		if (zone?.Terrain == null)
		{
			return -100f;
		}
		return zone.Terrain.SampleHeight(worldPosition) + -100f;
	}

	public Vector3 GetPoint(Vector3 worldPosition)
	{
		Vector3 result = worldPosition;
		Zone zone = GetZone(worldPosition);
		if (zone?.Terrain == null)
		{
			result.y = -100f;
			return result;
		}
		result.y = zone.Terrain.SampleHeight(worldPosition) + -100f;
		return result;
	}

	public Vector3 GetNormal(Vector3 worldPosition)
	{
		Zone zone = GetZone(worldPosition);
		if (zone?.Terrain == null)
		{
			return Vector3.up;
		}
		Vector3 vector = worldPosition - zone.Terrain.transform.position;
		return zone.Terrain.terrainData.GetInterpolatedNormal(Mathf.InverseLerp(0f, zone.Terrain.terrainData.size.x, vector.x), Mathf.InverseLerp(0f, zone.Terrain.terrainData.size.z, vector.z));
	}

	public bool InBounds(Vector3 worldPosition)
	{
		Zone zone = GetZone(worldPosition);
		if (zone == null)
		{
			return false;
		}
		return zone.InSaveBounds(worldPosition);
	}

	public static Zone GetZone(Vector3 worldPosition, bool fallbackToNml = false)
	{
		Zone zone = Singleton<StrandedWorld>.Instance?.GetZone_(worldPosition);
		if (!fallbackToNml)
		{
			return zone;
		}
		Zone zone2 = zone;
		if ((object)zone2 == null)
		{
			StrandedWorld instance = Singleton<StrandedWorld>.Instance;
			if ((object)instance == null)
			{
				return null;
			}
			zone2 = instance._nmlZone;
		}
		return zone2;
	}

	public int GetZoneId(Zone zone)
	{
		if (null == zone)
		{
			return -1;
		}
		for (int i = 0; i < Zones.Length; i++)
		{
			if (zone == Zones[i])
			{
				return i;
			}
		}
		return -1;
	}

	private Zone GetZone_(Vector3 worldPosition)
	{
		for (int i = 0; i < Zones.Length; i++)
		{
			Zone zone = Zones[i];
			if (zone.InBounds(worldPosition))
			{
				return zone;
			}
		}
		return null;
	}

	public static Zone.BiomeType GetBiome(Vector3 worldPosition)
	{
		return Singleton<StrandedWorld>.Instance?.GetBiome_(worldPosition) ?? Zone.BiomeType.RANDOM;
	}

	private Zone.BiomeType GetBiome_(Vector3 worldPosition)
	{
		return GetZone_(worldPosition)?.Biome ?? Zone.BiomeType.RANDOM;
	}

	protected void OnWorldGenerated()
	{
		this.WorldGenerated?.Invoke();
	}

	protected void OnZoneEntered(Zone zone)
	{
		this.ZoneEntered?.Invoke(zone);
	}

	private void Awake()
	{
		EventManager.AddListener<ZoneDownloadedEvent>(OnZoneDownloaded);
	}

	private void LoadZonePositions()
	{
		if (World.GenerationZonePositons == null)
		{
			World.CreateWorldZonePoints(WORLD_SEED);
			Debug.Log("StrandedWorld::CreateZones:: No Zone Positions Loaded - Creating New World Zone Positions");
		}
		for (int i = 0; i < 49; i++)
		{
			Vector3 zero = Vector3.zero;
			zero.x = World.GenerationZonePositons[i].x;
			zero.z = World.GenerationZonePositons[i].y;
			_zones[i].transform.position = zero;
			if (i == 0)
			{
				_zones[i].IsStartingIsland = true;
			}
		}
	}

	public float CreateWorld_Async()
	{
		return Timing.WaitUntilDone(CreateWorld());
	}

	public CoroutineHandle CreateWorld()
	{
		return Timing.RunCoroutine(CreateWorld_Sequence());
	}

	private IEnumerator<float> CreateWorld_Sequence()
	{
		if (!Game.Mode.IsMultiplayer())
		{
			EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_LOADING_WORLD"));
			yield return 0f;
			LoadWorld();
		}
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_LOADING_OBJECTS"));
		yield return 0f;
		LoadObjects();
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_LOADING_STARTING_ZONE"));
		yield return 0f;
		InitializeZones();
		int index = 0;
		Zone zone = FindClosestZone(Vector3.zero, out index);
		if (zone != null && index != -1)
		{
			JObject zoneData = GetZoneData(zone);
			if (zoneData != null)
			{
				zone.Enter();
				ZoneLoader.EObjectProcessingMode mode = _zoneLoader.ObjectProcessingMode;
				_zoneLoader.ObjectProcessingMode = ZoneLoader.EObjectProcessingMode.Faster;
				JObject field = zoneData.GetField("Objects");
				if (field != null && !field.IsNull())
				{
					zone.LoadImpostors(field);
					yield return _zoneLoader.LoadZone_Async(zone, zoneData);
				}
				else
				{
					_zoneLoader.GenerateZone(zone);
					zone.CreateImpostors();
					yield return _zoneLoader.CreateZoneObjects_Async(zone, index);
				}
				_zoneLoader.ObjectProcessingMode = mode;
			}
			else
			{
				Debug.LogError("StrandedWorld::LoadAsync:: Something went wrong. No starting zone data was found.");
			}
		}
		else
		{
			Debug.LogError("StrandedWorld::LoadAsync:: Something went wrong. No starting zone was found.");
		}
		Initialize_shelterTypes();
		SetShelterOnZones();
		SetSheltersOnWorldZones();
		OnWorldGenerated();
		OnZoneEntered(zone);
		EventManager.RaiseEvent(new AddStatsEvent(0, new Stats
		{
			IslandsExplored = 1
		}));
		SubscribeToZoneLoaderEvents();
		InvokeRepeating("PollZones", 0f, 0.3f);
	}

	private void SubscribeToZoneLoaderEvents()
	{
		_zoneLoader.CreatedZone += ZoneLoader_LoadedZone;
		_zoneLoader.LoadedZone += ZoneLoader_LoadedZone;
	}

	private void UnsubscribeFromZoneLoaderEvents()
	{
		if (_zoneLoader != null)
		{
			_zoneLoader.CreatedZone -= ZoneLoader_LoadedZone;
			_zoneLoader.LoadedZone -= ZoneLoader_LoadedZone;
		}
	}

	private void ZoneLoader_LoadedZone(Zone zone)
	{
		if (zone.Biome != Zone.BiomeType.MISSION)
		{
			EventManager.RaiseEvent(new AddStatsEvent(0, new Stats
			{
				IslandsHopped = 1
			}));
			if (!zone.HasVisited)
			{
				EventManager.RaiseEvent(new AddStatsEvent(0, new Stats
				{
					IslandsExplored = 1
				}));
			}
		}
	}

	protected override void OnDestroy()
	{
		UnsubscribeFromZoneLoaderEvents();
		this.WorldGenerated = null;
		this.ZoneEntered = null;
		EventManager.RemoveListener<ZoneDownloadedEvent>(OnZoneDownloaded);
		base.OnDestroy();
	}

	private void InitializeZones()
	{
		for (int i = 0; i < Zones.Length; i++)
		{
			Zone zone = Zones[i];
			zone.gameObject.SetActive(value: true);
			zone.Terrain.heightmapPixelError = 75f;
			zone.Terrain.basemapDistance = 180f;
			GetZoneData(zone)?.IsNull();
		}
	}

	private void LoadWorld()
	{
		LoadingResult loadingResult = World.LoadWorld();
		if (loadingResult.Succeeded)
		{
			ApplyWorldToZones();
		}
		else
		{
			Debug.LogError("StrandedWorld:: Error loading world: " + loadingResult.Message);
		}
	}

	public void ApplyWorldToZones()
	{
		LoadZonePositions();
		for (int i = 0; i < 49; i++)
		{
			Zone zone = Zones[i];
			Map map = World.MapList[i];
			zone.Terrain.terrainData.SetHeights(0, 0, map.HeightmapData);
			zone.ZoneName = map.EditorData.Name;
			zone.Id = map.EditorData.Id;
			zone.Version = map.EditorData.VersionNumber;
			zone.Biome = map.EditorData.Biome;
			zone.IsMapEditor = map.IsCustomMap();
			zone.IsUserMap = map.IsUserMap();
			zone.Seed = i;
			Texture2D height = WorldTools.CreateHeightMapTexture(zone.Terrain.terrainData);
			Texture2D shore = WorldTools.CreateShoreMaskTexture(zone.Terrain.terrainData);
			if (zone.Biome == Zone.BiomeType.ISLAND || zone.Biome == Zone.BiomeType.ISLAND_SMALL || zone.Biome == Zone.BiomeType.ISLAND_ROCK || zone.Biome == Zone.BiomeType.CARRIER)
			{
				zone.WaveOverlay = AddWaveOverlay(zone.gameObject, height, shore);
			}
			zone.SaveContainer.transform.SetAsLastSibling();
		}
	}

	private void LoadObjects(JObject rootData = null)
	{
		_zoneSavedObjectsLookup.Clear();
		if (Game.State == GameState.LOAD_GAME)
		{
			rootData = rootData ?? SaveManager.LoadSaveFile(Game.Mode);
			JObject persistentData = SaveManager.GetPersistentData(rootData);
			if (persistentData == null || persistentData.IsNull())
			{
				return;
			}
			JObject field = persistentData.GetField("StrandedWorld");
			if (field == null || field.IsNull())
			{
				Debug.LogError("StrandedWorld data is null or empty.");
				return;
			}
			JObject field2 = field.GetField("Zones");
			if (field2 == null || field2.IsNull())
			{
				Debug.LogError("Zones data is null or empty.");
				return;
			}
			for (int i = 0; i < 49; i++)
			{
				Zone zone = Zones[i];
				Map map = World.MapList[i];
				JObject jObject = field2.Children[i];
				string value = jObject.GetField("Id").GetValue<string>();
				int value2 = jObject.GetField("Version").GetValue<int>();
				if (map.EditorData.Id != value || map.EditorData.VersionNumber != value2)
				{
					JObject jObject2 = new JObject();
					jObject2.AddField("Name", map.EditorData.Name);
					jObject2.AddField("Id", map.EditorData.Id);
					jObject2.AddField("Version", map.EditorData.VersionNumber);
					if (map.IsCustomMap())
					{
						jObject2.AddField("Objects", map.ObjectData);
					}
					jObject = jObject2;
				}
				JObject field3 = jObject.GetField("Discovered");
				if (field3 != null)
				{
					bool value3 = field3.GetValue<bool>();
					zone.HasVisited = value3;
				}
				JObject field4 = jObject.GetField("Attributes");
				if (field4 != null)
				{
					SavingUtilities.Load(zone.Attributes, field4);
				}
				JObject field5 = jObject.GetField("Objects");
				if (field5 != null && !field5.IsNull())
				{
					zone.HasObjectsForBillboard = true;
				}
				_zoneSavedObjectsLookup.Add(zone.gameObject.name, jObject);
			}
			JObject jObject3 = field2.GetZoneData(_nmlZone.Id);
			if (jObject3 == null || jObject3.IsNull())
			{
				Debug.Log("No NML data. Recreating...");
				JObject jObject4 = new JObject();
				jObject4.AddField("Name", _nmlZone.gameObject.name);
				jObject4.AddField("Id", _nmlZone.Id);
				jObject4.AddField("Version", _nmlZone.Version);
				jObject4.AddField("Discovered", instance: false);
				jObject4.AddField("Objects", (JObject)null);
				jObject3 = jObject4;
			}
			_zoneSavedObjectsLookup.Add(_nmlZone.gameObject.name, jObject3);
			LoadShelterData(field);
		}
		else
		{
			if (Game.State != GameState.NEW_GAME)
			{
				return;
			}
			for (int j = 0; j < 49; j++)
			{
				Zone zone2 = Zones[j];
				Map map2 = World.MapList[j];
				JObject jObject5 = new JObject();
				jObject5.AddField("Name", map2.EditorData.Name);
				jObject5.AddField("Id", map2.EditorData.Id);
				jObject5.AddField("Version", map2.EditorData.VersionNumber);
				if (map2.IsCustomMap())
				{
					jObject5.AddField("Objects", map2.ObjectData);
					zone2.HasObjectsForBillboard = true;
				}
				_zoneSavedObjectsLookup.Add(zone2.gameObject.name, jObject5);
			}
		}
	}

	private void LoadShelterData(JObject strandedWorldData)
	{
		int[] array = new int[3] { -1, -1, -1 };
		int[] array2 = new int[3] { -1, -1, -1 };
		int num = -1;
		JObject field = strandedWorldData.GetField("Shelters");
		if (field != null)
		{
			JObject jObject = null;
			for (int i = 1; i < 4; i++)
			{
				int num2 = -1;
				int num3 = -1;
				jObject = field.GetField("Shelter" + i);
				if (jObject != null)
				{
					num2 = jObject.GetValue<int>();
				}
				jObject = field.GetField("Shelter" + i + "_zone");
				if (jObject != null)
				{
					num3 = jObject.GetValue<int>();
				}
				if (num2 == -1 || num3 == -1)
				{
					Debug.LogError("Load: Shelter " + i + "Data Not Found. Possible Old Save.");
					continue;
				}
				array[i - 1] = num2;
				array2[i - 1] = num3;
			}
			jObject = field.GetField("Wollie");
			if (jObject != null)
			{
				num = jObject.GetValue<int>();
			}
			if (num == -1)
			{
				Debug.LogError("Load: Wollie Data Not Found. Possible Old Save.");
			}
			for (int j = 0; j < _shelters.Length; j++)
			{
				int num4 = array2[j];
				_shelters[j] = array[j];
				if (_shelters[j] != -1)
				{
					Zones[num4].HasShelter = true;
					Zones[num4].ShelterType = _shelters[j];
				}
			}
			if (num != -1)
			{
				Zones[num].HasWollie = true;
			}
		}
		else
		{
			Debug.LogError("StrandedWorld::Load: Old Save Detected. Shelter Data not found.");
		}
	}

	private void PollZones()
	{
		if (PlayerRegistry.AllPeers.Count == 0 || LevelLoader.IsServerJoinInProgress)
		{
			return;
		}
		for (int i = 0; i < Zones.Length; i++)
		{
			Zone zone = Zones[i];
			bool active = PlayerRegistry.AllPlayers.Any_NonAlloc(InImpostorViewRange, zone);
			zone.Terrain.gameObject.SetActive(active);
			PollImposters(zone);
			if (!PollUnload(zone))
			{
				PollLoad(zone);
			}
		}
	}

	private void PollImposters(Zone zone)
	{
		if (zone.Loading || zone.Loaded)
		{
			return;
		}
		if (!zone.Billboarding)
		{
			if (!PlayerRegistry.AllPlayers.Any_NonAlloc(InImpostorLoadingRange, zone))
			{
				return;
			}
			if (zone.HasObjectsForBillboard)
			{
				JObject field = GetZoneData(zone).GetField("Objects");
				if (field != null && !field.IsNull())
				{
					zone.LoadImpostors(field);
				}
			}
			else if (_zoneLoader.GenerateZone(zone))
			{
				zone.CreateImpostors();
			}
		}
		else if (zone.Billboarding && !PlayerRegistry.AllPlayers.Any_NonAlloc(InImpostorViewRange, zone))
		{
			zone.ReleaseImpostors();
		}
	}

	private void PollZone(Zone zone)
	{
		PollUnload(zone);
	}

	private bool PollUnload(Zone zone)
	{
		if ((zone.Loading || zone.Loaded) && !PlayerRegistry.AllPlayers.Any_NonAlloc(InZoneUnLoadingBounds, zone))
		{
			if (zone.Loading)
			{
				_zoneLoader.AbortLoadingZone(zone);
			}
			else
			{
				SaveZone(zone);
				if (Game.Mode.IsServer())
				{
					MultiplayerZoneSync.Instance?.SendZone((byte)GetZoneId(zone), _zoneSavedObjectsLookup[zone.gameObject.name]);
				}
			}
			return true;
		}
		return false;
	}

	private bool PollLoad(Zone zone)
	{
		if (!zone.Loading && !zone.Loaded && PlayerRegistry.AllPlayers.Any_NonAlloc(InZoneLoadingBounds, zone))
		{
			OnZoneEntered(zone);
			LoadZone(zone);
			return true;
		}
		return false;
	}

	private void SaveZone(Zone zone, bool destroy = true, bool hardSave = false, bool handleOufOfGameBounds4SaveDynamicContainer = true)
	{
		bool flag = zone.Id == _nmlZone.Id;
		float timeScale = Time.timeScale;
		float fixedDeltaTime = Time.fixedDeltaTime;
		Time.fixedDeltaTime = 0f;
		Time.timeScale = 0f;
		JObject jObject = new JObject();
		JObject jObject2 = new JObject();
		SaveablePrefab[] componentsInChildren = zone.SaveContainer.GetComponentsInChildren<SaveablePrefab>(includeInactive: true);
		OrderByFurthestFromPoint(componentsInChildren, zone.transform.position);
		foreach (SaveablePrefab saveablePrefab in componentsInChildren)
		{
			if (!saveablePrefab.gameObject.activeInHierarchy || IsOutOfGameBounds(saveablePrefab.transform.position))
			{
				continue;
			}
			if (zone.InSaveBounds(saveablePrefab.transform.position))
			{
				if (Game.Mode.IsMaster())
				{
					ParentObjectToTransform(saveablePrefab, zone.SaveContainer);
					JObject jObject3 = SavingUtilities.SavePrefabToJObject(saveablePrefab);
					if (jObject3 != null)
					{
						jObject2.Add(jObject3);
					}
				}
				if (destroy)
				{
					MultiplayerMng.Destroy(saveablePrefab, DestroyDelay.NOW);
				}
			}
			else
			{
				ParentObjectToTransform(saveablePrefab, _nmlZone.SaveContainer);
			}
		}
		SaveablePrefab[] array = null;
		if (!flag)
		{
			array = _nmlZone.SaveContainer.GetComponentsInChildren<SaveablePrefab>(includeInactive: true);
			foreach (SaveablePrefab saveablePrefab2 in array)
			{
				if (!saveablePrefab2.gameObject.activeInHierarchy || IsOutOfGameBounds(saveablePrefab2.transform.position) || !zone.InSaveBounds(saveablePrefab2.transform.position))
				{
					continue;
				}
				if (Game.Mode.IsMaster())
				{
					ParentObjectToTransform(saveablePrefab2, zone.SaveContainer);
					JObject jObject4 = SavingUtilities.SavePrefabToJObject(saveablePrefab2);
					if (jObject4 != null)
					{
						jObject2.Add(jObject4);
					}
				}
				if (destroy)
				{
					MultiplayerMng.Destroy(saveablePrefab2, DestroyDelay.NOW);
				}
			}
		}
		if (Game.Mode.IsMaster())
		{
			try
			{
				Component[] objs = componentsInChildren;
				HandleOutOfGameBounds(objs);
				if (handleOufOfGameBounds4SaveDynamicContainer)
				{
					objs = array;
					HandleOutOfGameBounds(objs);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Error in SaveZone.HandleOutOfGameBounds: " + ex.Message + "\n" + ex.StackTrace);
			}
			jObject.AddField("Name", zone.gameObject.name);
			jObject.AddField("Id", zone.Id);
			jObject.AddField("Version", zone.Version);
			jObject.AddField("Discovered", zone.HasVisited);
			JObject jObject5 = SavingUtilities.Save(zone.Attributes);
			if (jObject5 != null)
			{
				jObject.AddField("Attributes", jObject5);
			}
			jObject.AddField("Objects", jObject2);
			if (!_zoneSavedObjectsLookup.ContainsKey(zone.gameObject.name))
			{
				_zoneSavedObjectsLookup.Add(zone.gameObject.name, null);
			}
			_zoneSavedObjectsLookup[zone.gameObject.name] = jObject;
		}
		if (!hardSave)
		{
			zone.Exit();
			zone.HasObjectsForBillboard = true;
			zone.Loaded = false;
		}
		Time.fixedDeltaTime = fixedDeltaTime;
		Time.timeScale = timeScale;
		_lastSavePosition = zone.transform.position;
	}

	private void OnZoneDownloaded(ZoneDownloadedEvent @event)
	{
		_zoneSavedObjectsLookup[Zones[@event.ZoneId].gameObject.name] = @event.Zone;
	}

	private void LoadZone(Zone zone)
	{
		JObject jObject = _zoneSavedObjectsLookup[zone.gameObject.name];
		JObject field = jObject.GetField("Objects");
		if (field != null && !field.IsNull())
		{
			_zoneLoader.LoadZone(zone, jObject);
		}
		else
		{
			_zoneLoader.CreateZone(zone);
		}
	}

	private void Initialize_shelterTypes()
	{
		for (int i = 0; i < _shelters.Length; i++)
		{
			_shelters[i] = -1;
		}
		System.Random random = new System.Random(WORLD_SEED);
		int num = 0;
		for (int j = 0; j < _shelters.Length; j++)
		{
			int num2;
			do
			{
				num2 = random.Next(0, 8);
				for (int k = 0; k < _shelters.Length; k++)
				{
					if (num2 == _shelters[k])
					{
						num2 = -1;
						break;
					}
				}
			}
			while (num2 == -1);
			_shelters[num] = num2;
			num++;
		}
	}

	private void SetShelterOnZones()
	{
		for (int i = 0; i < Zones.Length; i++)
		{
			Zones[i].HasShelter = false;
		}
		int num = CountViableMaps();
		if (num == 0)
		{
			return;
		}
		List<int> list = new List<int>(num);
		for (int j = 0; j < num; j++)
		{
			int num2;
			do
			{
				num2 = _zoneLoader.Random.Next(0, Zones.Length);
			}
			while (list.Contains(num2) || !IsMapViable(num2));
			list.Add(num2);
		}
		for (int k = 0; k < list.Count; k++)
		{
			Zones[list[k]].HasShelter = true;
		}
		SetWollieOnShelterZone(list);
	}

	private bool IsMapViable(int randIndex)
	{
		if (!Zones[randIndex].IsMapEditor && !Zones[randIndex].IsStartingIsland && !Zones[randIndex].HasShelter)
		{
			return !Zones[randIndex].Id.Contains("INTERNAL_MISSION");
		}
		return false;
	}

	private int CountViableMaps()
	{
		int num = 0;
		for (int i = 0; i < Zones.Length; i++)
		{
			if (IsMapViable(i))
			{
				num++;
			}
		}
		return Math.Min(_shelters.Length, num);
	}

	private void SetSheltersOnWorldZones()
	{
		int num = 0;
		for (int i = 0; i < Zones.Length; i++)
		{
			if (Zones[i].HasShelter)
			{
				Zones[i].ShelterType = _shelters[num];
				num++;
				if (num >= _shelters.Length)
				{
					break;
				}
			}
		}
		ShelterCount = num;
	}

	private void SetWollieOnShelterZone(List<int> sheltersOnWorld)
	{
		int index = _zoneLoader.Random.Next(0, sheltersOnWorld.Count);
		Zones[sheltersOnWorld[index]].HasWollie = true;
	}

	private static void OrderByFurthestFromPoint(SaveablePrefab[] arr, Vector3 point)
	{
		for (int i = 0; i < arr.Length - 1; i++)
		{
			for (int j = i + 1; j < arr.Length; j++)
			{
				SaveablePrefab saveablePrefab = arr[i];
				SaveablePrefab saveablePrefab2 = arr[j];
				if (saveablePrefab.CanReorder && saveablePrefab2.CanReorder)
				{
					float sqrMagnitude = (point - saveablePrefab.transform.position).sqrMagnitude;
					float sqrMagnitude2 = (point - saveablePrefab2.transform.position).sqrMagnitude;
					if (sqrMagnitude < sqrMagnitude2)
					{
						arr[i] = saveablePrefab2;
						arr[j] = saveablePrefab;
					}
				}
			}
		}
	}

	private Zone FindClosestZone(Vector3 position)
	{
		int index = 0;
		return FindClosestZone(position, out index);
	}

	private Zone FindClosestZone(Vector3 position, out int index)
	{
		index = -1;
		Zone result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < Zones.Length; i++)
		{
			float num2 = Vector3.Distance(position, Zones[i].transform.position);
			if (num2 < num)
			{
				num = num2;
				result = Zones[i];
				index = i;
			}
		}
		return result;
	}

	public Zone FindZone(string name)
	{
		for (int i = 0; i < Zones.Length; i++)
		{
			if (Zones[i].Id.Equals(name))
			{
				return Zones[i];
			}
		}
		return null;
	}

	public Zone FindEndGameZone()
	{
		for (int i = 0; i < Zones.Length; i++)
		{
			if (Zones[i].Id.Equals("INTERNAL_MISSION_3"))
			{
				return Zones[i];
			}
		}
		return null;
	}

	private bool InZoneUnLoadingBounds(IPlayer player, Zone zone)
	{
		return Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(zone.transform.position.x, zone.transform.position.z)) < 250f;
	}

	private bool InZoneLoadingBounds(IPlayer player, Zone zone)
	{
		return Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(zone.transform.position.x, zone.transform.position.z)) < 240f;
	}

	private bool InImpostorLoadingRange(IPlayer player, Zone zone)
	{
		return Vector3Tools.SqrDistanceOnAxis(player.transform.position, zone.transform.position, Vector3Tools.xz) < 1000000f;
	}

	private bool InImpostorViewRange(IPlayer player, Zone zone)
	{
		return Vector3Tools.SqrDistanceOnAxis(player.transform.position, zone.transform.position, Vector3Tools.xz) < 1001600f;
	}

	private void ParentObjectToTransform(SaveablePrefab sp, Transform parent)
	{
		BaseObject baseObject = sp as BaseObject;
		if (baseObject != null)
		{
			if (baseObject.CanParent)
			{
				baseObject.transform.parent = null;
				baseObject.TransformParent = parent;
				baseObject.Parent();
			}
		}
		else
		{
			sp.transform.parent = parent;
		}
	}

	private AddWaveOverlay AddWaveOverlay(GameObject zone, Texture2D height, Texture2D shore)
	{
		GameObject obj = new GameObject("WaveOverlay");
		obj.transform.parent = zone.transform;
		obj.transform.localPosition = Vector3.zero;
		AddWaveOverlay addWaveOverlay = obj.AddComponent<AddWaveOverlay>();
		addWaveOverlay.width = 250f;
		addWaveOverlay.height = 250f;
		addWaveOverlay.heightTexture = new OverlayHeightTexture();
		addWaveOverlay.heightTexture.tex = shore;
		addWaveOverlay.heightTexture.mask = height;
		addWaveOverlay.heightTexture.alpha = 0f;
		addWaveOverlay.heightTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
		addWaveOverlay.heightTexture.maskAlpha = 0.9f;
		addWaveOverlay.normalTexture = new OverlayNormalTexture();
		addWaveOverlay.normalTexture.tex = shore;
		addWaveOverlay.normalTexture.mask = height;
		addWaveOverlay.normalTexture.alpha = 0f;
		addWaveOverlay.normalTexture.maskAlpha = 0.4f;
		addWaveOverlay.normalTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
		addWaveOverlay.foamTexture = new OverlayFoamTexture();
		addWaveOverlay.foamTexture.tex = shore;
		addWaveOverlay.foamTexture.mask = height;
		addWaveOverlay.foamTexture.alpha = 0.4f;
		addWaveOverlay.foamTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
		addWaveOverlay.clipTexture = new OverlayClipTexture();
		addWaveOverlay.clipTexture.tex = shore;
		addWaveOverlay.clipTexture.ignoreQuerys = true;
		return addWaveOverlay;
	}

	public static int GetRandowWorldSeed()
	{
		string text = (Environment.TickCount & 0x7FFFFFFF).ToString();
		return int.Parse(text.Substring(0, Mathf.Min(text.Length, 8)));
	}

	public static void SetRandomWorldSeed()
	{
		SetWorldSeed(0);
	}

	public static void SetWorldSeed(int seed)
	{
		if (seed == 0)
		{
			LOCAL_WORLD_SEED = GetRandowWorldSeed();
			Debug.Log("StrandedWorld::SetWorldSeed:: Random [" + LOCAL_WORLD_SEED + "]");
		}
		else
		{
			LOCAL_WORLD_SEED = seed;
			Debug.Log("StrandedWorld::SetWorldSeed:: Custom [" + LOCAL_WORLD_SEED + "]");
		}
	}

	public static void SetWorldSeed(string seedValue)
	{
		if (string.IsNullOrEmpty(seedValue))
		{
			SetWorldSeed(0);
			return;
		}
		string s = seedValue.Substring(0, Mathf.Min(seedValue.Length, 8));
		int result = 0;
		if (!int.TryParse(s, out result))
		{
			Debug.LogWarning("StrandedWorld::SetWorldSeed:: Could not parse seed value. Seed likely contains invalid characters.");
		}
		SetWorldSeed(result);
	}

	public static GameObject AddDebugAxis(Vector3 position, Quaternion rotation)
	{
		return null;
	}

	public static GameObject AddDebugAxis(Vector3 position, Quaternion rotation, string text)
	{
		return null;
	}

	public System.Random GenerateLocalSeed(Vector3 objectPosition, int addedValue = 0)
	{
		Vector3 zero = Vector3.zero;
		Vector3 b = new Vector3(objectPosition.x, 0f, objectPosition.z);
		float num = Mathf.Atan2(b.z - zero.z, b.x - zero.x) * 180f / MathF.PI;
		int num2 = (int)Vector3.Distance(zero, b);
		addedValue++;
		return new System.Random(WORLD_SEED + ((int)num + num2) * addedValue);
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("WorldSeed", JSerializer.Serialize(WORLD_SEED));
		JObject jObject2 = new JObject();
		for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			IPlayer player = PlayerRegistry.AllPlayers[i];
			JObject jObject3 = new JObject();
			jObject3.AddField("Position", JSerializer.Serialize(player.transform.position));
			jObject3.AddField("ZoneId", JSerializer.Serialize(GetZone(player.transform.position, fallbackToNml: true).Id));
			jObject2.Add(jObject3);
		}
		jObject.AddField("PlayersData", jObject2);
		Zone[] zones = Zones;
		foreach (Zone zone in zones)
		{
			if (zone.Loaded)
			{
				SaveZone(zone, destroy: false, hardSave: true, handleOufOfGameBounds4SaveDynamicContainer: false);
			}
		}
		SaveZone(_nmlZone, destroy: false, hardSave: true);
		HandleOutOfGameBounds_NoMansLand();
		JObject jObject4 = new JObject();
		for (int k = 0; k < 49; k++)
		{
			JObject zoneData = GetZoneData(Zones[k]);
			jObject4.Add(zoneData);
		}
		JObject zoneData2 = GetZoneData(_nmlZone);
		jObject4.Add(zoneData2);
		jObject.AddField("Zones", jObject4);
		jObject.AddField("ShelterCount", JSerializer.Serialize(ShelterCount));
		JObject jObject5 = new JObject();
		int num = 1;
		int instance = -1;
		for (int l = 0; l < Zones.Length; l++)
		{
			Zone zone2 = Zones[l];
			if (zone2.HasShelter)
			{
				jObject5.AddField("Shelter" + num, zone2.ShelterType);
				jObject5.AddField("Shelter" + num + "_zone", l);
				num++;
			}
			if (zone2.HasWollie)
			{
				instance = l;
			}
		}
		jObject5.AddField("Wollie", instance);
		jObject.AddField("Shelters", jObject5);
		return jObject;
	}

	public void Load(JObject data)
	{
	}

	public float LoadWorld_Async(JObject rootData, JObject data)
	{
		return Timing.WaitUntilDone(LoadWorld(rootData, data));
	}

	private CoroutineHandle LoadWorld(JObject rootData, JObject data)
	{
		return Timing.RunCoroutine(LoadWorld_Sequence(rootData, data));
	}

	public IEnumerator<float> LoadWorld_Sequence(JObject rootData, JObject data)
	{
		if (!Game.Mode.IsMultiplayer())
		{
			EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_LOADING_WORLD"));
			yield return 0f;
			LoadWorld();
		}
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_LOADING_OBJECTS"));
		yield return 0f;
		LoadObjects(rootData);
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_LOADING_CURRENT_ZONE"));
		yield return 0f;
		InitializeZones();
		JObject field = data.GetField("ShelterCount");
		if (field != null)
		{
			ShelterCount = field.GetValue<int>();
		}
		JObject playersData = data.GetField("PlayersData");
		Vector3 position2;
		if (playersData != null && !playersData.IsNull())
		{
			for (int i = 0; i < playersData.Children.Count; i++)
			{
				JObject field2 = playersData.Children[i].GetField("Position");
				if (field2 != null)
				{
					Vector3 position = JSerializer.Deserialize<Vector3>(field2);
					yield return LoadStartingZone(position, fallbackToNml: true);
				}
			}
		}
		else if (LegacyUtilities.LoadStrandedWorldPlayerPosition_PreCoop(data, out position2))
		{
			yield return LoadStartingZone(position2);
		}
		if (!_nmlZone.Loaded && GetZoneData(_nmlZone).IsValid())
		{
			yield return LoadStartingZone(_nmlZone);
		}
		OnWorldGenerated();
		SubscribeToZoneLoaderEvents();
		InvokeRepeating("PollZones", 0.3f, 0.3f);
		Debug.Log("StrandedWorld::LoadWorld_Sequence:: Loaded world.");
	}

	private float LoadStartingZone(Vector3 position, bool fallbackToNml = false)
	{
		Zone zone = GetZone(position, fallbackToNml);
		return LoadStartingZone(zone);
	}

	private float LoadStartingZone(Zone zone)
	{
		if (zone != null)
		{
			if (!zone.Loaded)
			{
				return Timing.WaitUntilDone(Timing.RunCoroutine(LoadStartingZone_Sequence(zone)));
			}
		}
		else
		{
			Debug.LogError("StrandedWorld::LoadStartingZone:: Something went wrong. No zone was found.");
		}
		return 0f;
	}

	private IEnumerator<float> LoadStartingZone_Sequence(Zone zone)
	{
		JObject zoneData = GetZoneData(zone);
		if (zoneData != null)
		{
			zone.Enter();
			JObject field = zoneData.GetField("Objects");
			if (field != null && !field.IsNull())
			{
				ZoneLoader.EObjectProcessingMode mode = _zoneLoader.ObjectProcessingMode;
				_zoneLoader.ObjectProcessingMode = ZoneLoader.EObjectProcessingMode.Faster;
				zone.LoadImpostors(field);
				yield return _zoneLoader.LoadZone_Async(zone, zoneData);
				OnZoneEntered(zone);
				_zoneLoader.ObjectProcessingMode = mode;
			}
			else
			{
				Debug.LogError("StrandedWorld::LoadStartingZone_Sequence:: Zone [" + zone.ZoneName + "] [Objects] field is null or empty.");
			}
		}
		else
		{
			Debug.LogError("StrandedWorld::LoadStartingZone_Sequence:: Something went wrong. Zone [" + zone.ZoneName + "] data is null or empty.");
		}
	}

	private bool IsOutOfGameBounds(Vector3 position)
	{
		if (!(position.y > 300f) && !(position.y < -25f) && !(position.x > 2500f) && !(position.x < -2500f) && !(position.z > 2500f))
		{
			return position.z < -2500f;
		}
		return true;
	}

	private bool HandleOutOfGameBounds(Component obj)
	{
		if (IsOutOfGameBounds(obj.transform.position))
		{
			if (obj.gameObject.TryGetComponent<BoltEntity>(out var component))
			{
				if (component.IsOwner)
				{
					MultiplayerMng.DestroyMultiplayerEntity(component.gameObject, DestroyDelay.LATE);
				}
				else
				{
					component.gameObject.SetActive(value: false);
				}
			}
			else
			{
				BeamTeam.Destroy(obj.gameObject);
			}
			return true;
		}
		return false;
	}

	private void HandleOutOfGameBounds(Component[] objs)
	{
		if (objs.IsNullOrDestroyed())
		{
			return;
		}
		for (int i = 0; i < objs.Length; i++)
		{
			Component component = objs[i];
			if (!component.IsNullOrDestroyed() && component.gameObject.activeInHierarchy)
			{
				HandleOutOfGameBounds(objs[i]);
			}
		}
	}

	public void HandleOutOfGameBounds_NoMansLand()
	{
		try
		{
			Rigidbody[] componentsInChildren = _nmlZone.SaveContainer.GetComponentsInChildren<Rigidbody>();
			Component[] objs = componentsInChildren;
			HandleOutOfGameBounds(objs);
		}
		catch (Exception ex)
		{
			Debug.LogError("Error in HandleOutOfGameBounds_NoMansLand: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void HandleOutOfGameBounds(Zone zone, bool handleNmlContainer = false)
	{
		if (!zone.IsNullOrDestroyed() && zone.Loaded)
		{
			try
			{
				Rigidbody[] componentsInChildren = zone.SaveContainer.GetComponentsInChildren<Rigidbody>();
				Component[] objs = componentsInChildren;
				HandleOutOfGameBounds(objs);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error in HandleOutOfGameBounds: " + ex.Message + "\n" + ex.StackTrace);
			}
		}
		if (handleNmlContainer && zone != _nmlZone)
		{
			HandleOutOfGameBounds_NoMansLand();
		}
	}
}
