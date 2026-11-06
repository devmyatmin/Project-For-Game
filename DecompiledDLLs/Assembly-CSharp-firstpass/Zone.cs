using System.Collections.Generic;
using Beam;
using Beam.Rendering;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Terrain;
using Beam.Utilities;
using Ceto;
using UnityEngine;

public class Zone : MonoBehaviour
{
	public enum BiomeType
	{
		RANDOM,
		DEEP_SEA,
		MEDIUM,
		SHALLOW,
		ISLAND,
		ISLAND_SMALL,
		ISLAND_ROCK,
		MISSION,
		CARRIER
	}

	public const float SAVE_BOUNDS_MAX_HEIGHT = 300f;

	public const float SAVE_BOUNDS_MIN_HEIGHT = -25f;

	private const float SAVE_BOUNDS_BUFFER = 30f;

	public const float SAVE_BOUNDS_RADIUS = 155f;

	public const float SQR_SAVE_BOUNDS_RADIUS = 24025f;

	public ZoneAttributes Attributes = new ZoneAttributes();

	[SerializeField]
	private Terrain _terrain;

	public Transform SaveContainer;

	[SerializeField]
	private BiomeType _biome;

	public string ZoneName;

	public string Id;

	public int Version;

	public bool IsStartingIsland;

	private GenerationPoints[,] _generationPoints;

	private readonly List<GeneratedObject> _generatedObjects = new List<GeneratedObject>();

	private bool _billboarding;

	private readonly List<ImpostorBase> _activeImposters = new List<ImpostorBase>();

	public Terrain Terrain => _terrain;

	public BiomeType Biome
	{
		get
		{
			return _biome;
		}
		set
		{
			_biome = value;
		}
	}

	public int Seed { get; set; }

	public bool Loading { get; set; }

	public bool Loaded { get; set; }

	public bool IsMapEditor { get; set; }

	public bool IsUserMap { get; set; }

	public bool HasObjectsForBillboard { get; set; }

	public bool HasShelter { get; set; }

	public int ShelterType { get; set; }

	public bool HasWollie { get; set; }

	public bool HasVisited { get; set; }

	public GenerationPoints[,] GenerationPoints => _generationPoints;

	public IList<GeneratedObject> GeneratedObjects => _generatedObjects;

	public AddWaveOverlay WaveOverlay { get; set; }

	public bool Billboarding => _billboarding;

	public bool InBounds(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Vector3Tools.SqrDistanceOnAxis(((Component)this).transform.position, position, Vector3Tools.xz) < BeamMath.Sqr(250f);
	}

	public virtual bool InSaveBounds(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (InSaveBoundsRadius(position))
		{
			return InSaveBoundsHeight(position);
		}
		return false;
	}

	private bool InSaveBoundsRadius(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Vector3Tools.SqrDistanceOnAxis(((Component)this).transform.position, position, Vector3Tools.xz) < 24025f;
	}

	private bool InSaveBoundsHeight(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (position.y < 300f)
		{
			return position.y > -25f;
		}
		return false;
	}

	private void Awake()
	{
		_terrain.detailObjectDistance = 70f;
		_terrain.heightmapPixelError = 45f;
		((Component)this).gameObject.SetActive(false);
	}

	public void GenerateGridData()
	{
		_generationPoints = ZoneObjectGenerator.GenerateGridData(this, Seed);
	}

	public virtual void Enter()
	{
		Terrain.heightmapPixelError = 45f;
		if ((Object)(object)WaveOverlay != (Object)null)
		{
			((Behaviour)WaveOverlay).enabled = true;
		}
	}

	public void Exit()
	{
		Terrain.heightmapPixelError = 75f;
		if ((Object)(object)WaveOverlay != (Object)null)
		{
			((Behaviour)WaveOverlay).enabled = false;
		}
	}

	public void OnObjectGenerated(GeneratedObject objectData)
	{
		_generatedObjects.Add(objectData);
	}

	public void OnObjectCreated(GameObject instance)
	{
	}

	public void OnObjectLoaded(GameObject instance)
	{
	}

	public ImpostorBase FindImpostor(GameObject instance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int parentInstanceId = Vector3Tools.Hash(instance.transform.position);
		TryGetImpostor(parentInstanceId, out var impostor);
		return impostor;
	}

	public void CreateImpostors()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (_billboarding)
		{
			return;
		}
		for (int i = 0; i < _generatedObjects?.Count; i += ((i <= ImpostorManager.MIN_BILLBOARD_DENSITY) ? 1 : ImpostorManager.BILLBOARD_DENSITY_STEP))
		{
			GeneratedObject generatedObject = _generatedObjects[i];
			IImpostorParent impostorParentPrefab = generatedObject.ImpostorParentPrefab;
			if (!BeamTeam.IsNullOrDestroyed(impostorParentPrefab) && impostorParentPrefab.Scope.HasFlag(ImposterScope.Terrain))
			{
				ImpostorBase impostorBase = CreateImpostor(impostorParentPrefab.Id, generatedObject.Position, generatedObject.Rotation);
				if ((Object)(object)impostorBase != (Object)null)
				{
					impostorBase.SetRenderables(impostorParentPrefab.Renderables);
				}
			}
		}
		_billboarding = true;
	}

	public void LoadImpostors(JObject zoneObjectData)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		if (_billboarding)
		{
			return;
		}
		for (int i = 0; i < zoneObjectData.Children.Count; i += ((i <= ImpostorManager.MIN_BILLBOARD_DENSITY) ? 1 : ImpostorManager.BILLBOARD_DENSITY_STEP))
		{
			JObject jObject = zoneObjectData.Children[i];
			JObject field = jObject.GetField("prefab");
			if (field == null)
			{
				continue;
			}
			SaveablePrefab prefab = Prefabs.GetPrefab(field);
			IImpostorParent impostorParent = ((prefab != null) ? BeamTeam.GetInterface<IImpostorParent>((Component)(object)prefab) : null);
			if (BeamTeam.IsNullOrDestroyed(impostorParent) || !impostorParent.Scope.HasFlag(ImposterScope.Terrain))
			{
				continue;
			}
			ImpostorBase impostorBase = CreateImpostor(impostorParent.Id);
			if ((Object)(object)impostorBase != (Object)null)
			{
				JObject field2 = jObject.GetField("Transform");
				Vector3 val = JSerializer.Deserialize<Vector3>(field2.GetField("localPosition"));
				Quaternion val2 = JSerializer.Deserialize<Quaternion>(field2.GetField("localRotation"));
				Vector3 value = JSerializer.Deserialize<Vector3>(field2.GetField("localScale"));
				value = Vector3Tools.Truncate(value);
				Vector3 val3 = val;
				Quaternion rotation = val2;
				Vector3 localScale = value;
				JObject field3 = jObject.GetField("Parent");
				if (field3 != null)
				{
					JObject field4 = field3.GetField("Transform");
					Vector3 val4 = JSerializer.Deserialize<Vector3>(field4.GetField("localPosition"));
					Quaternion val5 = JSerializer.Deserialize<Quaternion>(field4.GetField("localRotation"));
					Vector3 value2 = JSerializer.Deserialize<Vector3>(field4.GetField("localScale"));
					value2 = Vector3Tools.Truncate(value2);
					val3 = val4 + val5 * val;
					rotation = val5 * val2;
					localScale = new Vector3(value2.x / value.x, value2.y / value.y, value2.z / value.z);
				}
				((Component)impostorBase).transform.position = ((Component)this).transform.position + val3;
				((Component)impostorBase).transform.rotation = rotation;
				((Component)impostorBase).transform.localScale = localScale;
				impostorBase.ParentInstanceId = Vector3Tools.Hash(((Component)impostorBase).transform.position);
				impostorBase.SetRenderables(impostorParent.Renderables);
			}
		}
		_billboarding = true;
	}

	public ImpostorBase CreateImpostor(int parentPrefabId, Vector3 position, Quaternion rotation)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		ImpostorBase impostorBase = ImpostorManager.Create(parentPrefabId);
		if ((Object)(object)impostorBase == (Object)null)
		{
			Debug.LogError((object)("CreateImposter:: " + parentPrefabId));
			return null;
		}
		if ((Object)(object)impostorBase == (Object)(object)DummyImpostor.Instance)
		{
			return null;
		}
		((Component)impostorBase).transform.position = position;
		((Component)impostorBase).transform.rotation = rotation;
		impostorBase.ParentInstanceId = Vector3Tools.Hash(((Component)impostorBase).transform.position);
		_activeImposters.Add(impostorBase);
		return impostorBase;
	}

	private ImpostorBase CreateImpostor(int parentPrefabId)
	{
		ImpostorBase impostorBase = ImpostorManager.Create(parentPrefabId);
		if ((Object)(object)impostorBase == (Object)null)
		{
			Debug.LogError((object)("CreateImposter:: " + parentPrefabId));
			return null;
		}
		if ((Object)(object)impostorBase == (Object)(object)DummyImpostor.Instance)
		{
			return null;
		}
		_activeImposters.Add(impostorBase);
		return impostorBase;
	}

	private bool TryGetImpostor(int parentInstanceId, out ImpostorBase impostor)
	{
		impostor = null;
		for (int i = 0; i < _activeImposters.Count; i++)
		{
			ImpostorBase impostorBase = _activeImposters[i];
			if (impostorBase.ParentInstanceId == parentInstanceId)
			{
				impostor = impostorBase;
				return true;
			}
		}
		return false;
	}

	public void ReleaseImpostors()
	{
		for (int i = 0; i < _activeImposters.Count; i++)
		{
			ImpostorManager.Release(_activeImposters[i]);
		}
		_activeImposters.Clear();
		_billboarding = false;
	}
}
