using System;
using System.Collections.Generic;
using System.Linq;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beam.Crafting;

public class Constructing : BaseObject, IConstructable, ICraftable, IBase, IHasCraftingType, IPlaceable, ISelectable
{
	[Serializable]
	public class ConstructingLevel
	{
		public GameObject LevelGameObject;
	}

	public enum ERotationMode
	{
		None,
		Free
	}

	private class ReplicateCreateConstruction : I_MultiplayerMessage
	{
		[I_Replicate]
		public Construction_STRUCTURE Structure;

		[Replicate]
		public int StructureLayer;

		[Replicate]
		public uint PrefabId;

		[Replicate]
		public MiniGuid ReferenceId;

		[Replicate]
		public Vector3 Position;

		[Replicate]
		public Quaternion Rotation;

		public override void OnPeer()
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			((Component)Structure).gameObject.layer = StructureLayer;
			Constructing constructing = MultiplayerMng.Instantiate<Constructing>(PrefabId, ReferenceId);
			constructing.transform.position = Position;
			constructing.transform.rotation = Rotation;
			constructing.Structure = Structure;
			constructing.Place();
		}
	}

	[SerializeField]
	private bool handlePlayerColission;

	public const float UNIT_WIDTH = 0.3f;

	public const float ONE_UNIT = 3f;

	public const float ONE_HALF_UNIT = 1.5f;

	public const float CONNECTOR_PANEL_DIAMTER = 0.6f;

	public const float CONNECTOR_FOUNDATION_DIAMETER = 1.2f;

	public const float SNAPPING_THRESHOLD = 0.2f;

	public const float SPHERECAST_RADIUS = 0.3f;

	[PREFAB_PROCESSOR_GetComponent]
	[HideInInspector]
	[SerializeField]
	private Selecting _selecting;

	[Header("CRAFTING")]
	[SerializeField]
	private GhostCollider.ColliderType _colliderType;

	[SerializeField]
	private bool _craftImmediate;

	[SerializeField]
	private GameObject _structurePrefab;

	private Connector _snappedConnector;

	private Connector _prevSnappedConnector;

	[SerializeField]
	private float _placingDistance = 4f;

	[SerializeField]
	private ERotationMode _rotationMode;

	[SerializeField]
	private int _rotationAngle = 90;

	private float _proximityDistance = 4.5f;

	private Placement _previousPlacement = new Placement();

	[SerializeField]
	private GameObject _ghostObject;

	[SerializeField]
	private Mesh _ghostMesh;

	[SerializeField]
	private Renderer _ghostRenderer;

	private Construction_STRUCTURE _structure;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Connector _myConnector;

	[HideInInspector]
	[SerializeField]
	private Connector[] _myConnectors;

	[FormerlySerializedAs("_onCrafted")]
	[SerializeField]
	[HideInInspector]
	private InteracterUnityEvent _crafted;

	[Header("PROXIMITY CHECKING")]
	[SerializeField]
	private ConstructingProximityCheckMode _proximityChecking;

	private IConstructingProximityCheck _proximityCheck;

	[PREFAB_PROCESSOR_GetComponents]
	[SerializeField]
	private PlacementModifier[] _placementModifiers;

	[SerializeField]
	private LayerMask _connectorLayerMask;

	public float MaximumHealthPoints;

	[SerializeField]
	private ConstructingLevel[] _constructingLevels;

	[SerializeField]
	private Collider[] _constructingLevelColliders;

	[PREFAB_PROCESSOR_GetComponents]
	[SerializeField]
	private Collider[] _colliders;

	[SerializeField]
	[FormerlySerializedAs("nestedPlacingCollider")]
	private bool _nestedPlacingCollider;

	private bool _snapped;

	private int _previousLevel;

	[HideInInspector]
	public Vector3 Pos;

	[HideInInspector]
	public Quaternion Rot;

	public Selecting Selecting => _selecting;

	public int CreatorId { get; set; }

	public GhostCollider.ColliderType ColliderType => _colliderType;

	public bool CraftImmediate => _craftImmediate;

	protected GameObject StructurePrefab => _structurePrefab;

	public Connector SnappedConnector
	{
		get
		{
			return _snappedConnector;
		}
		set
		{
			_snappedConnector = value;
		}
	}

	public int Walls
	{
		get
		{
			int num = 0;
			Connector[] myConnectors = _myConnectors;
			foreach (Connector connector in myConnectors)
			{
				if (connector.Type == ConnectorType.WALL && connector.CanSet && !connector.IsValid)
				{
					num++;
				}
			}
			return num;
		}
	}

	public float PlacingDistance => _placingDistance;

	public ERotationMode RotationMode => _rotationMode;

	protected float ProximityDistance
	{
		get
		{
			return _proximityDistance;
		}
		set
		{
			_proximityDistance = value;
		}
	}

	public Mesh GhostMesh => _ghostMesh;

	public Material[] GhostMaterials => _ghostRenderer.sharedMaterials;

	public Construction_STRUCTURE Structure
	{
		get
		{
			return _structure;
		}
		set
		{
			_structure = value;
			CanParent = (Object)(object)_structure == (Object)null;
		}
	}

	public Connector MyConnector => _myConnector;

	public Connector[] MyConnectors => _myConnectors;

	protected LayerMask ConnectorLayerMask
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _connectorLayerMask;
		}
	}

	public Collider[] Colliders => _colliders;

	public bool IsSnapped => _snapped;

	public bool IsStructurePrefabAMultiplayerEntity()
	{
		if (!((Object)(object)_structurePrefab == (Object)null))
		{
			return _structurePrefab.GetComponent<SaveablePrefab>()?.IsMultiplayerEntity ?? false;
		}
		return false;
	}

	public void SetSnapped(Connector snappedConnector)
	{
		_snapped = (Object)(object)snappedConnector != (Object)null;
		_prevSnappedConnector = _snappedConnector;
		_snappedConnector = snappedConnector;
	}

	protected override void Awake()
	{
		base.Awake();
		InitializeProximityCheck();
		CheckHealth();
	}

	private void InitializeProximityCheck()
	{
		if (_proximityChecking == ConstructingProximityCheckMode.None)
		{
			_proximityCheck = new NoConstructingProximityCheck();
		}
		else
		{
			_proximityCheck = new DefaultConstructingProximityCheck();
		}
	}

	public virtual void OnBeginCrafting()
	{
	}

	public virtual bool OwnerPlacing(Transform ghost, BeamRay ray, float yRotation)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		Placement placement = new Placement();
		placement.Position = ghost.position;
		placement.Rotation = ghost.rotation;
		for (int i = 0; i < _placementModifiers.Length; i++)
		{
			placement = _placementModifiers[i].Modify(placement, ray);
		}
		bool result = false;
		_prevSnappedConnector = _snappedConnector;
		_snappedConnector = null;
		_snapped = false;
		IList<Connector> componentSphereCastAll = BeamPhysics.GetComponentSphereCastAll<Connector>(ray.BaseRay, 0.3f, ray.Length, _connectorLayerMask.value);
		if (componentSphereCastAll.Count > 0)
		{
			for (int j = 0; j < componentSphereCastAll.Count; j++)
			{
				Connector connector = componentSphereCastAll[j];
				_snapped = _myConnector.ConnectTo(connector, placement);
				if (!_snapped)
				{
					continue;
				}
				placement = CheckPlacement(placement);
				_snappedConnector = connector;
				if ((Object)(object)_snappedConnector != (Object)(object)_prevSnappedConnector)
				{
					Ghost component = ((Component)ghost).GetComponent<Ghost>();
					IList<Constructing> constructions;
					if (!_snappedConnector.Constructing.Structure.DisableGhostCollisionOnSnap)
					{
						IList<Constructing> list = _snappedConnector.Constructing.Structure.Constructions.Where((Constructing c) => c is Construction_RAFT).ToList();
						constructions = list;
					}
					else
					{
						constructions = _snappedConnector.Constructing.Structure.Constructions;
					}
					component.SetCollisionForConstructions(constructions, ignore: true);
				}
				result = true;
				break;
			}
		}
		else
		{
			result = _myConnector.ConnectTo(null, placement);
		}
		if (Object.op_Implicit((Object)(object)_prevSnappedConnector) && (Object)(object)_prevSnappedConnector != (Object)(object)_snappedConnector && Object.op_Implicit((Object)(object)_prevSnappedConnector.Constructing) && Object.op_Implicit((Object)(object)_prevSnappedConnector.Constructing.Structure))
		{
			((Component)ghost).GetComponent<Ghost>().SetCollisionForConstructions(_prevSnappedConnector.Constructing.Structure.Constructions, ignore: false);
		}
		_prevSnappedConnector = null;
		if (_snapped)
		{
			Quaternion val = placement.Rotation;
			if (_rotationMode != ERotationMode.None)
			{
				float num = Mathf.Round(yRotation / (float)_rotationAngle) * (float)_rotationAngle;
				val *= Quaternion.Euler(new Vector3(0f, num, 0f));
			}
			ghost.position = placement.Position;
			ghost.rotation = val;
		}
		else
		{
			ghost.position = placement.Position;
			ghost.rotation = placement.Rotation;
			if (_rotationMode != ERotationMode.None)
			{
				ghost.rotation *= Quaternion.Euler(new Vector3(0f, yRotation, 0f));
			}
		}
		return result;
	}

	public virtual bool ReplicantPlacing(Transform ghost, BeamRay ray, float yRotation)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		Placement placement = new Placement();
		placement.Position = ghost.position;
		placement.Rotation = ghost.rotation;
		for (int i = 0; i < _placementModifiers.Length; i++)
		{
			placement = _placementModifiers[i].Modify(placement, ray);
		}
		bool flag = false;
		if (_snapped)
		{
			_myConnector.ConnectTo(_snappedConnector, placement);
			placement = CheckPlacement(placement);
			if ((Object)(object)_snappedConnector != (Object)(object)_prevSnappedConnector && _snappedConnector.Constructing.Structure.DisableGhostCollisionOnSnap)
			{
				Ghost component = ((Component)ghost).GetComponent<Ghost>();
				IList<Constructing> constructions;
				if (!_snappedConnector.Constructing.Structure.DisableGhostCollisionOnSnap)
				{
					IList<Constructing> list = _snappedConnector.Constructing.Structure.Constructions.Where((Constructing c) => c is Construction_RAFT).ToList();
					constructions = list;
				}
				else
				{
					constructions = _snappedConnector.Constructing.Structure.Constructions;
				}
				component.SetCollisionForConstructions(constructions, ignore: true);
			}
			flag = true;
		}
		else
		{
			flag = _myConnector.ConnectTo(null, placement);
		}
		if (Object.op_Implicit((Object)(object)_prevSnappedConnector) && (Object)(object)_prevSnappedConnector != (Object)(object)_snappedConnector && Object.op_Implicit((Object)(object)_prevSnappedConnector.Constructing) && Object.op_Implicit((Object)(object)_prevSnappedConnector.Constructing.Structure))
		{
			((Component)ghost).GetComponent<Ghost>().SetCollisionForConstructions(_prevSnappedConnector.Constructing.Structure.Constructions, ignore: false);
		}
		_prevSnappedConnector = null;
		if (_snapped)
		{
			Quaternion val = placement.Rotation;
			if (_rotationMode != ERotationMode.None)
			{
				float num = Mathf.Round(yRotation / (float)_rotationAngle) * (float)_rotationAngle;
				val *= Quaternion.Euler(new Vector3(0f, num, 0f));
			}
			ghost.position = placement.Position;
			ghost.rotation = val;
		}
		else
		{
			ghost.position = placement.Position;
			ghost.rotation = placement.Rotation;
			if (_rotationMode != ERotationMode.None)
			{
				ghost.rotation *= Quaternion.Euler(new Vector3(0f, yRotation, 0f));
			}
		}
		return flag;
	}

	public virtual void Place()
	{
		IList<Connector> connectorsAtPlacement = GetConnectorsAtPlacement();
		if (true)
		{
			DoPlace(connectorsAtPlacement);
		}
		else
		{
			Dismantle();
		}
	}

	public virtual void OnCrafted(IPlayer player)
	{
		((UnityEvent<IPlayer>)_crafted)?.Invoke(player);
	}

	protected virtual IList<Connector> GetConnectorsAtPlacement()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return BeamPhysics.GetComponentsInBounds<Connector>(_colliders, LayerMask.op_Implicit(_connectorLayerMask));
	}

	protected virtual void DoPlace(IList<Connector> connectors)
	{
		UpdateConnectorsAtPlacement(connectors);
		CheckStructure();
	}

	protected virtual void UpdateConnectorsAtPlacement(IList<Connector> connectors)
	{
		for (int i = 0; i < connectors.Count; i++)
		{
			Connector connector = connectors[i];
			if (_nestedPlacingCollider)
			{
				connector.Refresh(((Component)this).gameObject);
			}
			else
			{
				connector.Refresh();
			}
		}
		for (int j = 0; j < _myConnectors.Length; j++)
		{
			Connector connector2 = _myConnectors[j];
			if (_nestedPlacingCollider)
			{
				connector2.Refresh(((Component)this).gameObject);
			}
			else
			{
				connector2.Refresh();
			}
		}
	}

	protected virtual bool CheckPlacementIsSupported(IList<Connector> connectors)
	{
		return true;
	}

	public bool CheckProximity(Transform ghost)
	{
		return _proximityCheck.CheckProximity(this, ghost, _proximityDistance);
	}

	public virtual void CheckStructure()
	{
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_snappedConnector != (Object)null)
		{
			Structure = _snappedConnector.Constructing.Structure;
		}
		else if (BeamTeam.IsNullOrDestroyed(Structure))
		{
			uint prefabId = StructurePrefab.GetComponent<SaveablePrefab>().PrefabId;
			Structure = MultiplayerMng.Instantiate<Construction_STRUCTURE>(prefabId, MiniGuid.NewFrom(ReferenceId), Singleton<StrandedWorld>.Instance?.NmlZone.SaveContainer);
			Structure.transform.position = transform.position;
			Structure.transform.rotation = transform.rotation;
			Structure.transform.localScale = Vector3.one;
			((Component)Structure).gameObject.layer = ((Component)this).gameObject.layer;
			if (MPExtensions.IsServer(Game.Mode) && Prefabs.IsMultiplayerEntity(prefabId))
			{
				ReplicateCreateConstruction replicateCreateConstruction = new ReplicateCreateConstruction();
				replicateCreateConstruction.Structure = Structure;
				replicateCreateConstruction.StructureLayer = ((Component)this).gameObject.layer;
				replicateCreateConstruction.PrefabId = PrefabId;
				replicateCreateConstruction.ReferenceId = ReferenceId;
				replicateCreateConstruction.Position = transform.position;
				replicateCreateConstruction.Rotation = transform.rotation;
				replicateCreateConstruction.Post();
			}
		}
		transform.parent = null;
		TransformParent = Structure.transform;
		Parent();
		transform.localScale = Vector3.one;
		Structure.AddConstruction(this);
	}

	protected virtual Placement CheckPlacement(Placement placement)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _previousPlacement.Position - placement.Position;
		if (val.magnitude > 0.2f)
		{
			_previousPlacement.Position = placement.Position;
			_previousPlacement.Rotation = placement.Rotation;
		}
		return _previousPlacement;
	}

	public bool HasMaxHealth()
	{
		return HealthPoints == MaximumHealthPoints;
	}

	public virtual void CheckHealth()
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		HealthPoints = Mathf.Min(HealthPoints, MaximumHealthPoints);
		if (_constructingLevels == null || _constructingLevels.Length == 0)
		{
			return;
		}
		int num = Mathf.FloorToInt((HealthPoints - 25f) / MaximumHealthPoints / 0.33f);
		num = Mathf.Max(0, Mathf.Min(num, _constructingLevels.Length - 1));
		bool flag = false;
		for (int i = 0; i < _constructingLevels.Length; i++)
		{
			ConstructingLevel obj = _constructingLevels[i];
			bool flag2 = i == num;
			obj.LevelGameObject.SetActive(flag2);
			if (!flag2)
			{
				continue;
			}
			for (int j = 0; j < _constructingLevelColliders.Length; j++)
			{
				if (_previousLevel == _constructingLevels.Length - 2 && num == _constructingLevels.Length - 1 && handlePlayerColission)
				{
					Bounds bounds = _constructingLevelColliders[j].bounds;
					Vector3 center = bounds.center;
					bounds = _constructingLevelColliders[j].bounds;
					Collider[] array = Physics.OverlapBox(center, bounds.extents * 0.7f);
					foreach (Collider val in array)
					{
						if (((Component)val).gameObject.layer == Layers.PLAYER && ((object)val).GetType() == typeof(CharacterController))
						{
							if (FindSuitableTranslatePoint(val, out var translatePoint) && !flag)
							{
								((Component)val).transform.Translate(translatePoint, (Space)0);
								flag = true;
							}
							else
							{
								((Component)val).transform.Translate(Vector3.up);
							}
						}
					}
				}
				_previousLevel = num;
				_constructingLevelColliders[j].isTrigger = num != _constructingLevels.Length - 1;
			}
		}
	}

	public override void Damage(float damage, IBase damager)
	{
		base.Damage(damage, damager);
		CheckHealth();
	}

	public virtual void Dismantle()
	{
		Beam.Utilities.ExtensionMethods.CheckForProjectilesInChildren(transform);
		((Component)this).gameObject.SetActive(false);
		MultiplayerMng.Destroy(this);
	}

	public override void Destroy()
	{
		IList<Connector> connectorsAtPlacement = GetConnectorsAtPlacement();
		Dismantle();
		UpdateConnectorsAtPlacement(connectorsAtPlacement);
		if (Object.op_Implicit((Object)(object)Structure))
		{
			Structure.RemoveConstruction(this);
		}
	}

	public int GetConnectorId(Connector connector)
	{
		if ((Object)(object)connector == (Object)(object)_myConnector)
		{
			return 0;
		}
		for (int i = 0; i < _myConnectors.Length; i++)
		{
			if ((Object)(object)_myConnectors[i] == (Object)(object)connector)
			{
				return i + 1;
			}
		}
		return -1;
	}

	public Connector GetConnectorById(int id)
	{
		if (id < 0)
		{
			return null;
		}
		if (id == 0)
		{
			return _myConnector;
		}
		return _myConnectors[id - 1];
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if ((Object)(object)Structure != (Object)null)
		{
			JObject jObject2 = new JObject();
			jObject2.AddField("Transform", JSerializer.Serialize((Component)(object)Structure.transform));
			jObject.AddField("Parent", jObject2);
		}
		JObject jObject3 = new JObject();
		for (int i = 0; i < _myConnectors.Length; i++)
		{
			try
			{
				JObject data = _myConnectors[i].Save();
				jObject3.Add(data);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Constructing::Load:: Error saving a connector [" + ((Object)_myConnectors[i]).name + "]\n" + ex));
			}
		}
		jObject.AddField("Connectors", jObject3);
		return jObject;
	}

	public override void Load(JObject data)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data);
		CheckHealth();
		JObject field = data.GetField("Transform");
		Pos = JSerializer.Deserialize<Vector3>(field.GetField("localPosition"));
		Rot = JSerializer.Deserialize<Quaternion>(field.GetField("localRotation"));
		JObject field2 = data.GetField("Connectors");
		for (int i = 0; i < _myConnectors.Length; i++)
		{
			if (i >= field2.Children.Count)
			{
				Debug.LogError((object)("Missing data for connector '" + ((Object)_myConnectors[i]).name + "'"));
				continue;
			}
			try
			{
				JObject jObject = field2.Children[i];
				if (jObject != null)
				{
					_myConnectors[i].Load(jObject);
				}
				else
				{
					Debug.LogError((object)("No saved data for connector [" + ((Object)_myConnectors[i]).name + "]"));
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Error loading a connector [" + ((Object)_myConnectors[i]).name + "]\n" + ex));
			}
		}
	}

	private bool FindSuitableTranslatePoint(Collider hit, out Vector3 translatePoint)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		CharacterController component = ((Component)hit).GetComponent<CharacterController>();
		Vector3 position = ((Component)hit).transform.position;
		Bounds bounds = ((Collider)component).bounds;
		Vector3 val = position + (bounds.size.y - component.radius) * 1.5f * Vector3.up;
		Vector3 position2 = ((Component)hit).transform.position;
		bounds = ((Collider)component).bounds;
		Vector3 val2 = position2 + (bounds.size.y + component.radius) * 0.5f * Vector3.up;
		LayerMask physicsLayerMask = GetPhysicsLayerMask(Layers.PLAYER);
		physicsLayerMask = LayerMask.op_Implicit(LayerMask.op_Implicit(physicsLayerMask) ^ LayerMask.GetMask(new string[1] { LayerMask.LayerToName(Layers.PLAYER) }));
		Collider[] array = Physics.OverlapCapsule(val, val2, component.radius, LayerMask.op_Implicit(physicsLayerMask));
		if (array.Length == 0)
		{
			translatePoint = Vector3.up;
			return true;
		}
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				for (int k = -1; k < 2; k++)
				{
					Vector3 val3 = new Vector3((float)j, 0f, (float)k);
					if (!Physics.CheckCapsule(val + val3, val2 + val3, component.radius, LayerMask.op_Implicit(physicsLayerMask)))
					{
						translatePoint = Vector3.up + val3;
						return true;
					}
				}
			}
		}
		translatePoint = Vector3.zero;
		return false;
	}

	private LayerMask GetPhysicsLayerMask(int currentLayer)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < 32; i++)
		{
			if (!Physics.GetIgnoreLayerCollision(currentLayer, i))
			{
				num |= 1 << i;
			}
		}
		return LayerMask.op_Implicit(num);
	}
}
