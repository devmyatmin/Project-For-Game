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
		return Vector3Tools.SqrDistanceOnAxis(base.transform.position, position, Vector3Tools.xz) < BeamMath.Sqr(250f);
	}

	public virtual bool InSaveBounds(Vector3 position)
	{
		if (InSaveBoundsRadius(position))
		{
			return InSaveBoundsHeight(position);
		}
		return false;
	}

	private bool InSaveBoundsRadius(Vector3 position)
	{
		return Vector3Tools.SqrDistanceOnAxis(base.transform.position, position, Vector3Tools.xz) < 24025f;
	}

	private bool InSaveBoundsHeight(Vector3 position)
	{
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
		base.gameObject.SetActive(value: false);
	}

	public void GenerateGridData()
	{
		_generationPoints = ZoneObjectGenerator.GenerateGridData(this, Seed);
	}

	public virtual void Enter()
	{
		Terrain.heightmapPixelError = 45f;
		if (WaveOverlay != null)
		{
			WaveOverlay.enabled = true;
		}
	}

	public void Exit()
	{
		Terrain.heightmapPixelError = 75f;
		if (WaveOverlay != null)
		{
			WaveOverlay.enabled = false;
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
		int parentInstanceId = instance.transform.position.Hash();
		TryGetImpostor(parentInstanceId, out var impostor);
		return impostor;
	}

	public void CreateImpostors()
	{
		if (_billboarding)
		{
			return;
		}
		for (int i = 0; i < _generatedObjects?.Count; i += ((i <= ImpostorManager.MIN_BILLBOARD_DENSITY) ? 1 : ImpostorManager.BILLBOARD_DENSITY_STEP))
		{
			GeneratedObject generatedObject = _generatedObjects[i];
			IImpostorParent impostorParentPrefab = generatedObject.ImpostorParentPrefab;
			if (!impostorParentPrefab.IsNullOrDestroyed() && impostorParentPrefab.Scope.HasFlag(ImposterScope.Terrain))
			{
				ImpostorBase impostorBase = CreateImpostor(impostorParentPrefab.Id, generatedObject.Position, generatedObject.Rotation);
				if (impostorBase != null)
				{
					impostorBase.SetRenderables(impostorParentPrefab.Renderables);
				}
			}
		}
		_billboarding = true;
	}

	public void LoadImpostors(JObject zoneObjectData)
	{
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
			IImpostorParent impostorParent = Prefabs.GetPrefab(field)?.GetInterface<IImpostorParent>();
			if (impostorParent.IsNullOrDestroyed() || !impostorParent.Scope.HasFlag(ImposterScope.Terrain))
			{
				continue;
			}
			ImpostorBase impostorBase = CreateImpostor(impostorParent.Id);
			if (impostorBase != null)
			{
				JObject field2 = jObject.GetField("Transform");
				Vector3 vector = JSerializer.Deserialize<Vector3>(field2.GetField("localPosition"));
				Quaternion quaternion = JSerializer.Deserialize<Quaternion>(field2.GetField("localRotation"));
				Vector3 value = JSerializer.Deserialize<Vector3>(field2.GetField("localScale"));
				value = value.Truncate();
				Vector3 vector2 = vector;
				Quaternion rotation = quaternion;
				Vector3 localScale = value;
				JObject field3 = jObject.GetField("Parent");
				if (field3 != null)
				{
					JObject field4 = field3.GetField("Transform");
					Vector3 vector3 = JSerializer.Deserialize<Vector3>(field4.GetField("localPosition"));
					Quaternion quaternion2 = JSerializer.Deserialize<Quaternion>(field4.GetField("localRotation"));
					Vector3 value2 = JSerializer.Deserialize<Vector3>(field4.GetField("localScale"));
					value2 = value2.Truncate();
					vector2 = vector3 + quaternion2 * vector;
					rotation = quaternion2 * quaternion;
					localScale = new Vector3(value2.x / value.x, value2.y / value.y, value2.z / value.z);
				}
				impostorBase.transform.position = base.transform.position + vector2;
				impostorBase.transform.rotation = rotation;
				impostorBase.transform.localScale = localScale;
				impostorBase.ParentInstanceId = impostorBase.transform.position.Hash();
				impostorBase.SetRenderables(impostorParent.Renderables);
			}
		}
		_billboarding = true;
	}

	public ImpostorBase CreateImpostor(int parentPrefabId, Vector3 position, Quaternion rotation)
	{
		ImpostorBase impostorBase = ImpostorManager.Create(parentPrefabId);
		if (impostorBase == null)
		{
			Debug.LogError("CreateImposter:: " + parentPrefabId);
			return null;
		}
		if (impostorBase == DummyImpostor.Instance)
		{
			return null;
		}
		impostorBase.transform.position = position;
		impostorBase.transform.rotation = rotation;
		impostorBase.ParentInstanceId = impostorBase.transform.position.Hash();
		_activeImposters.Add(impostorBase);
		return impostorBase;
	}

	private ImpostorBase CreateImpostor(int parentPrefabId)
	{
		ImpostorBase impostorBase = ImpostorManager.Create(parentPrefabId);
		if (impostorBase == null)
		{
			Debug.LogError("CreateImposter:: " + parentPrefabId);
			return null;
		}
		if (impostorBase == DummyImpostor.Instance)
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
