using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beam.Crafting;

public class ConstructionObject : InteractiveObject, ICraftable, IBase, IHasCraftingType, IPlaceable
{
	[Header("CRAFTING")]
	[SerializeField]
	private GhostCollider.ColliderType _colliderType;

	[SerializeField]
	private bool _craftImmediate;

	[SerializeField]
	private Mesh _ghostMesh;

	[SerializeField]
	private Renderer _ghostRenderer;

	[SerializeField]
	private GameObject _ghostColliders;

	[SerializeField]
	private float _placingDistance = 4f;

	[SerializeField]
	private bool _canPlace;

	[PREFAB_PROCESSOR_GetComponents]
	[SerializeField]
	private PlacementModifier[] _placementModifiers;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Connector _myConnector;

	[FormerlySerializedAs("_onCrafted")]
	[SerializeField]
	[HideInInspector]
	private InteracterUnityEvent _crafted;

	public int CreatorId { get; set; }

	public GhostCollider.ColliderType ColliderType => _colliderType;

	public bool CraftImmediate => _craftImmediate;

	public Mesh GhostMesh => _ghostMesh;

	public virtual Material[] GhostMaterials => _ghostRenderer.sharedMaterials;

	public GameObject GhostColliders
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_ghostColliders))
			{
				_ghostColliders = ((Component)this).gameObject;
			}
			return _ghostColliders;
		}
	}

	public float PlacingDistance => _placingDistance;

	public bool CanPlace
	{
		get
		{
			return _canPlace;
		}
		set
		{
			_canPlace = value;
		}
	}

	public virtual void OnBeginCrafting()
	{
	}

	public bool OwnerPlacing(Transform ghost, BeamRay ray, float yRotation)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		Placement placement = new Placement();
		placement.Position = ghost.position;
		placement.Rotation = ghost.rotation;
		for (int i = 0; i < _placementModifiers.Length; i++)
		{
			placement = _placementModifiers[i].Modify(placement, ray);
		}
		bool flag = true;
		if ((Object)(object)_myConnector != (Object)null)
		{
			for (int j = 0; j < _myConnector.Checks.Count; j++)
			{
				flag = _myConnector.Checks[j].Check(placement, null);
				if (!flag)
				{
					break;
				}
			}
		}
		ghost.position = placement.Position;
		ghost.rotation = placement.Rotation * Quaternion.Euler(new Vector3(0f, yRotation, 0f));
		return flag;
	}

	public bool ReplicantPlacing(Transform ghost, BeamRay ray, float yRotation)
	{
		return OwnerPlacing(ghost, ray, yRotation);
	}

	public virtual void Place()
	{
	}

	public virtual bool CheckProximity(Transform ghost)
	{
		return false;
	}

	public virtual void OnCrafted(IPlayer player)
	{
		((UnityEvent<IPlayer>)_crafted)?.Invoke(player);
	}

	public override void EnablePhysics()
	{
		base.EnablePhysics();
		if (Object.op_Implicit((Object)(object)rigidbody))
		{
			rigidbody.constraints = (RigidbodyConstraints)0;
		}
	}

	public override void DisablePhysics()
	{
		base.DisablePhysics();
		if (Object.op_Implicit((Object)(object)rigidbody))
		{
			rigidbody.constraints = (RigidbodyConstraints)126;
		}
	}

	public void SetRigidBodyMass(float mass)
	{
		if (Object.op_Implicit((Object)(object)rigidbody))
		{
			rigidbody.mass = mass;
		}
	}

	public float GetRigidBodyMass()
	{
		if (!Object.op_Implicit((Object)(object)rigidbody))
		{
			return 0f;
		}
		return rigidbody.mass;
	}
}
