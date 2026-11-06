using Beam;
using Beam.Crafting;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

public class ConstructionObject_SPEAR : ProjectileObject, IChargeable, IUseable, IAnimatablePrimary, IBase, IHasCraftingType, IReloadable, IAnimatableSecondary, IProjectile
{
	private class ReplicateKillAndAttach : I_MultiplayerMessage
	{
		[I_Replicate]
		public SaveablePrefab Spearead;

		[Replicate]
		public ConstructionObject_SPEAR Spear;

		public override void OnPeer()
		{
			Spear.KillAndAttach((ISpearedInstance)Spearead);
		}
	}

	private class ReplicateLocalFaunaCreateSpearedInstanceOnMaster : MultiplayerMessage
	{
		[Replicate]
		public ConstructionObject_SPEAR Spear;

		[Replicate]
		public uint PrefabId;

		[Replicate]
		public Vector3 Position;

		[Replicate]
		public Quaternion Rotation;

		public override void OnPeer()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Spear.ReplicatedKillAndAttach(LocalFaunaCreateSpeared(PrefabId, Position, Rotation));
		}
	}

	[SerializeField]
	private Transform _spearAttachingAnchor;

	[SerializeField]
	private bool _canUseSecondary;

	[SerializeField]
	private bool _canSpearFish;

	private bool _reloaded = true;

	[Header("Secondary Animations")]
	[SerializeField]
	private AnimationType _secondaryIdle;

	[SerializeField]
	private AnimationType _secondary;

	[SerializeField]
	private AnimationType _reloadAnimation;

	[SerializeField]
	private float _spearSpeed;

	private ISpearedInstance _attachedChild;

	[SerializeField]
	[HideInInspector]
	private BaseActionUnityEvent _charged;

	[SerializeField]
	[HideInInspector]
	private BaseActionUnityEvent _usedSecondary;

	public bool CanUseSecondary
	{
		get
		{
			return _canUseSecondary;
		}
		set
		{
			_canUseSecondary = value;
		}
	}

	public bool Reloaded => _reloaded;

	public AnimationType SecondaryIdle => _secondaryIdle;

	public AnimationType Secondary => _secondary;

	public AnimationType ReloadAnimation => _reloadAnimation;

	BaseActionUnityEvent IChargeable.Charged => _charged;

	BaseActionUnityEvent IChargeable.UsedSecondary => _usedSecondary;

	public override bool ValidatePrimary(IBase obj)
	{
		if (BeamTeam.IsValid(_attachedChild))
		{
			CollectAttached();
			return false;
		}
		return base.ValidatePrimary(obj);
	}

	public override void Use()
	{
		base.Use();
	}

	private void CollectAttached()
	{
		if (BeamTeam.IsValid(_attachedChild) && (Owner.Holder.Storage.CanPush(_attachedChild.PickupableObject) || BeamTeam.IsNullOrDestroyed(Owner.Holder.CurrentObject)))
		{
			Owner.Holder.Pickup(_attachedChild.PickupableObject);
		}
	}

	public bool ValidateSecondary(IBase obj)
	{
		return BeamTeam.IsNullOrDestroyed(_attachedChild);
	}

	public void PreSecondary()
	{
		ActionEventData.Populate(this, null);
		((UnityEvent<IBase, IBaseActionEventData>)_charged).Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
	}

	public void UseSecondary()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		ActionEventData.Populate(this, null);
		ActionEventData.Position = Owner.transform.position;
		((UnityEvent<IBase, IBaseActionEventData>)_usedSecondary).Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
		IPlayer owner = Owner;
		BeamRay ray = owner.PlayerCamera.GetRay(7f);
		float huntingMultiplier = owner.PlayerSkills.HuntingMultiplier;
		owner.Holder.DropCurrent();
		float num = Vector3.Distance(transform.position, ray.OriginPoint);
		((Component)transform).transform.position = ray.OriginPoint;
		((Component)transform).transform.LookAt(ray.EndPoint);
		Transform val = ((Component)transform).transform;
		val.position += ((Component)transform).transform.forward * num;
		((Behaviour)ProjectileMovement).enabled = true;
		ProjectileMovement.Fire(owner);
		rigidbody.AddForce(transform.forward * _spearSpeed * huntingMultiplier, (ForceMode)1);
	}

	public void PostSecondary()
	{
	}

	public bool CanReloadWith(IPickupable pickupable)
	{
		return false;
	}

	public void Reload(IPickupable pickupable)
	{
	}

	public override void DamageObject(IBase obj)
	{
		if (!StabObject(obj) && !BeamTeam.IsNullOrDestroyed(((Component)obj.transform).GetComponent<Shootable>()))
		{
			base.DamageObject(obj);
		}
	}

	private bool StabObject(IBase obj)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		ISpearable spearable = obj as ISpearable;
		if (BeamTeam.IsNullOrDestroyed(spearable) || !spearable.CanSpear)
		{
			return false;
		}
		if (!_canSpearFish && spearable.Type == InteractiveType.ANIMALS_FISH)
		{
			return false;
		}
		if (spearable.IsLocalFauna && spearable.LocalFaunaCheckHit(_spearAttachingAnchor.position))
		{
			LocalFaunaHandleSpearedInstanceOnMaster(spearable.LocalFaunaSpearedInstancePrefabId, obj.transform.position, obj.transform.rotation);
			spearable.LocalFaunaRelease();
			return true;
		}
		ISpearedInstance spearedInstance = spearable.GetSpearedInstance(_spearAttachingAnchor.position);
		if (KillAndAttach(spearedInstance))
		{
			return true;
		}
		return false;
	}

	private void LocalFaunaHandleSpearedInstanceOnMaster(uint prefabId, Vector3 position, Quaternion rotation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMaster(Game.Mode))
		{
			ReplicatedKillAndAttach(LocalFaunaCreateSpeared(prefabId, position, rotation));
			return;
		}
		ReplicateLocalFaunaCreateSpearedInstanceOnMaster replicateLocalFaunaCreateSpearedInstanceOnMaster = new ReplicateLocalFaunaCreateSpearedInstanceOnMaster();
		replicateLocalFaunaCreateSpearedInstanceOnMaster.Spear = this;
		replicateLocalFaunaCreateSpearedInstanceOnMaster.PrefabId = prefabId;
		replicateLocalFaunaCreateSpearedInstanceOnMaster.Position = position;
		replicateLocalFaunaCreateSpearedInstanceOnMaster.Rotation = rotation;
		replicateLocalFaunaCreateSpearedInstanceOnMaster.Post();
	}

	private bool KillAndAttach(ISpearedInstance speared)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(speared))
		{
			speared.Kill(OwnerId);
			IPhysical physicsObject = speared.PhysicsObject;
			IDraggable draggable = physicsObject as IDraggable;
			if (!BeamTeam.IsNullOrDestroyed(draggable))
			{
				draggable.CanDrag = false;
			}
			physicsObject.RemoveRigidbody((physicsObject as ISaveablePrefab)?.IsOwner ?? true);
			Beam.Utilities.ExtensionMethods.CheckForProjectilesInChildren(physicsObject.transform);
			physicsObject.transform.parent = null;
			physicsObject.TransformParent = _spearAttachingAnchor;
			physicsObject.Parent();
			physicsObject.CanParent = false;
			physicsObject.transform.localPosition = Vector3.zero;
			physicsObject.transform.localRotation = Quaternion.identity;
			speared.PickupableObject.PickedUp += SpearedInstance_PickedUp;
			_attachedChild = speared;
			return true;
		}
		return false;
	}

	private static ISpearedInstance LocalFaunaCreateSpeared(uint prefabId, Vector3 position, Quaternion rotation)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Interactive_FISH interactive_FISH = MultiplayerMng.Instantiate<Interactive_FISH>(prefabId, MiniGuid.New());
		interactive_FISH.transform.position = position;
		interactive_FISH.transform.rotation = rotation;
		interactive_FISH.Parent();
		return interactive_FISH;
	}

	private void SpearedInstance_PickedUp(IPickupable sender)
	{
		if (!BeamTeam.IsNullOrDestroyed(_attachedChild))
		{
			_attachedChild.PhysicsObject.EnablePhysics();
			_attachedChild.PhysicsObject.CanParent = true;
			_attachedChild.PickupableObject.PickedUp -= SpearedInstance_PickedUp;
			_attachedChild = null;
		}
	}

	private void ReplicatedKillAndAttach(ISpearedInstance speared)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		bool flag = KillAndAttach(speared);
		if (MPExtensions.IsMultiplayer(Game.Mode) & flag)
		{
			ReplicateKillAndAttach replicateKillAndAttach = new ReplicateKillAndAttach();
			replicateKillAndAttach.Spearead = (SaveablePrefab)speared;
			replicateKillAndAttach.Spear = this;
			replicateKillAndAttach.Post();
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if (!BeamTeam.IsNullOrDestroyed(_attachedChild))
		{
			if (!BeamTeam.IsNullOrDestroyed(Owner) && (Owner.Holder.Storage.Has(this) || (!BeamTeam.IsNullOrDestroyed(Owner.Holder.CurrentObject) && Owner.Holder.CurrentObject.Equals(this))))
			{
				JObject jObject2 = SavingUtilities.SavePrefabToJObject(_attachedChild.PickupableObject);
				if (jObject2 != null)
				{
					jObject.AddField("AttachedChildData", jObject2);
				}
			}
			else
			{
				jObject.AddField("AttachedChild", JSerializer.Serialize((object)_attachedChild.PickupableObject.ReferenceId.ToString()));
			}
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("AttachedChildData");
		if (field != null)
		{
			SavingUtilities.LoadPrefabFromJData(MultiplayerZoneSync.Usage.Spear, field, null, (SaveablePrefab instance) =>
			{
				KillAndAttach((ISpearedInstance)instance);
			});
			return;
		}
		JObject field2 = data.GetField("AttachedChild");
		if (!field2)
		{
			return;
		}
		MiniGuid childReferenceData = Beam.Utilities.ExtensionMethods.ToMiniGuid(field2.GetValue<string>());
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			InteractiveObject interactiveObject = MultiplayerMng.FindObject<InteractiveObject>(childReferenceData);
			if ((Object)(object)interactiveObject != (Object)null)
			{
				KillAndAttach((ISpearedInstance)interactiveObject);
			}
			else
			{
				Debug.LogError((object)$"InteractiveObject_SPEAR::Load:: Error finding child [{childReferenceData}]");
			}
		});
	}
}
