using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class ProjectileObject : ConstructionObject, IPickupable, IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial, IDraggable, IPhysical, ISelectable, IProjectile
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private ProjectileMovement _projectileMovement;

	[SerializeField]
	private BaseObjectProxy _proxy;

	private float _mass;

	private float _drag;

	private float _angularDrag;

	private RigidbodyInterpolation _interpolation;

	private CollisionDetectionMode _collision;

	public ProjectileMovement ProjectileMovement => _projectileMovement;

	public BaseObjectProxy Proxy => _proxy;

	protected override void Awake()
	{
		base.Awake();
		_projectileMovement.Initialize(this);
		((Behaviour)_projectileMovement).enabled = false;
		CacheRigidbodyParameters();
		if (!LevelLoader.IsLoading())
		{
			rigidbody.useGravity = true;
		}
	}

	private void CacheRigidbodyParameters()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		_mass = rigidbody.mass;
		_drag = rigidbody.drag;
		_angularDrag = rigidbody.angularDrag;
		_interpolation = rigidbody.interpolation;
		_collision = rigidbody.collisionDetectionMode;
	}

	public override void Attached()
	{
		base.Attached();
		rigidbody.isKinematic = !IsOwner;
	}

	protected override void OnPlayerDisconnect(PlayerDisconnectedEvent disconnectedEvent)
	{
		bool flag = !BeamTeam.IsNullOrDestroyed(Owner);
		base.OnPlayerDisconnect(disconnectedEvent);
		if (!flag && !BeamTeam.IsNullOrDestroyed(_projectileMovement.Thrower) && _projectileMovement.Thrower.Id == disconnectedEvent.PlayerId)
		{
			MultiplayerMng.Destroy(this);
		}
	}

	public override bool Store()
	{
		Reset();
		return base.Store();
	}

	protected override void OnTransformParentChanged()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		base.OnTransformParentChanged();
		if (!BeamTeam.IsNullOrDestroyed(rigidbody))
		{
			rigidbody.interpolation = (RigidbodyInterpolation)((ParentId != TransformId.Unknown) ? ((int)_interpolation) : 0);
		}
	}

	public virtual void Reset()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		global::ExtensionMethods.EnableColliders(colliders, enabled: true, isTrigger: false);
		this.CheckRigidbody(IsOwner);
		rigidbody.mass = _mass;
		rigidbody.drag = _drag;
		rigidbody.angularDrag = _angularDrag;
		rigidbody.interpolation = _interpolation;
		rigidbody.collisionDetectionMode = _collision;
		_projectileMovement.Reset();
		((Behaviour)_projectileMovement).enabled = false;
		CanParent = true;
		CanDrag = true;
	}

	public virtual void Pierce(ProjectileHit hit)
	{
		Transform val = hit.Transform;
		if (BeamTeam.IsValid(hit.Object) && ((Component)hit.Transform).gameObject.layer != Layers.PROJECTILE_HITBOXES)
		{
			val = hit.Object.transform;
		}
		if (((Component)val).CompareTag("Terrain"))
		{
			this.SetKinematic(kinematic: true);
			CanDrag = false;
			return;
		}
		bool isTrigger = BeamTeam.IsValid(hit.Object) && ((Component)hit.Object.transform).CompareTag("Shark");
		global::ExtensionMethods.EnableColliders(colliders, enabled: true, isTrigger);
		this.RemoveRigidbody(IsOwner, immediate: true);
		if ((Object)(object)val != (Object)null)
		{
			this.ParentTo(val);
		}
		CanParent = false;
		CanDrag = false;
	}

	public virtual void LocalFaunaPierce()
	{
		global::ExtensionMethods.EnableColliders(colliders, enabled: true, isTrigger: true);
		this.RemoveRigidbody(IsOwner, immediate: true);
		CanParent = false;
		CanDrag = false;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if (!BeamTeam.IsNullOrDestroyed(_projectileMovement.AttachedBase))
		{
			ISaveablePrefab saveablePrefab = _projectileMovement.AttachedBase as ISaveablePrefab;
			if (!BeamTeam.IsNullOrDestroyed(saveablePrefab))
			{
				JObject jObject2 = new JObject();
				jObject2.AddField("AttachedId", saveablePrefab.ReferenceId.ToString());
				jObject2.AddField("Parent", ((Object)transform.parent).name);
				jObject.AddField("ProjectileAttached", jObject2);
			}
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		CacheRigidbodyParameters();
		JObject attachedData = data.GetField("ProjectileAttached");
		if (!attachedData)
		{
			return;
		}
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			JObject field = attachedData.GetField("AttachedId");
			BaseObject baseObject = MultiplayerMng.FindObject<BaseObject>(Beam.Utilities.ExtensionMethods.ToMiniGuid(field.GetValue<string>()));
			if ((Object)(object)baseObject != (Object)null)
			{
				_projectileMovement.AttachedBase = baseObject;
				JObject field2 = attachedData.GetField("Parent");
				Transform val = Beam.Utilities.ExtensionMethods.FindNestedChild(baseObject.transform, field2.GetValue<string>());
				ProjectileHit hit = new ProjectileHit(val, baseObject);
				Pierce(hit);
				JSerializer.DeserializeInto(data.GetField("Transform"), (Component)(object)transform);
				CompleteTransformSerialization();
			}
			else
			{
				Debug.LogError((object)$"Projectile:: Couldn't find attached [{field}]");
				MultiplayerMng.Destroy(this, DestroyDelay.LATE);
			}
		});
	}
}
