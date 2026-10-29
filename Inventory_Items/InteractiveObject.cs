using System;
using System.Runtime.CompilerServices;
using Beam.Events;
using Beam.Language;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beam;

public class InteractiveObject : BaseObject, ISelectable, IBase, IHasCraftingType, IInteractable, IUseable, IAnimatablePrimary, IDraggable, IPhysical, IPickupable, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial
{
	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _descriptionTerm = "NONE";

	[SerializeField]
	private InteractiveObjectClassification _classification;

	[Header("ANIMATIONS")]
	[SerializeField]
	private AnimationType _idle;

	[SerializeField]
	private AnimationType _primary;

	[SerializeField]
	private AnimationType _primaryOnObject;

	[Header("HOLDING POSITION")]
	[SerializeField]
	private Vector3 _localHoldingPosition;

	[SerializeField]
	private Vector3 _localHoldingRotation;

	[Header("USING")]
	[BitMask(typeof(UsingConstraint))]
	[SerializeField]
	private UsingConstraint _usingConstraints;

	[SerializeField]
	private bool _canPickup;

	private bool _isDraggable;

	[SerializeField]
	private bool _canDrag;

	[SerializeField]
	private bool _canUse;

	[SerializeField]
	private bool _canUseOnObjects;

	[SerializeField]
	private bool _singleUse;

	[FormerlySerializedAs("_isInteractive")]
	[SerializeField]
	private bool _canInteract;

	[SerializeField]
	private bool _canDamage;

	[SerializeField]
	private bool _destroyOnEmpty = true;

	[SerializeField]
	private bool _invincible;

	[SerializeField]
	private float _durabilityPoints;

	[SerializeField]
	private float _damage;

	private MaterialPropertyBlock _propertyBlock;

	[Header("PHYSICS")]
	[SerializeField]
	private bool _nonKinematic;

	[SerializeField]
	private CollisionDetectionMode _collisionDetectionMode;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidbody;

	[PREFAB_PROCESSOR_GetComponents]
	[SerializeField]
	private Collider[] _colliders;

	[PREFAB_PROCESSOR_GetComponentsInChildren]
	[SerializeField]
	private Renderer[] _renderers;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private LODGroup _lodGroup;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	private Burning _burning;

	private IPlayer _owner;

	[FormerlySerializedAs("_onUsed")]
	[SerializeField]
	[HideInInspector]
	private BaseActionUnityEvent _used;

	public InteractiveObjectClassification Classification => _classification;

	public AnimationType Idle => _idle;

	public AnimationType Primary => _primary;

	public AnimationType PrimaryOnObject => _primaryOnObject;

	public Vector3 LocalHoldingPosition => _localHoldingPosition;

	public Vector3 LocalHoldingRotation => _localHoldingRotation;

	public UsingConstraint UsingConstraints => _usingConstraints;

	public virtual bool CanPickUp
	{
		get
		{
			return _canPickup;
		}
		set
		{
			_canPickup = value;
		}
	}

	protected bool IsDraggable
	{
		set
		{
			_isDraggable = value;
			CanDrag = _isDraggable;
		}
	}

	public virtual bool CanDrag
	{
		get
		{
			return _canDrag;
		}
		set
		{
			_canDrag = value;
		}
	}

	public bool CanUse
	{
		get
		{
			return _canUse;
		}
		set
		{
			_canUse = value;
		}
	}

	public bool CanUseOnObjects
	{
		get
		{
			return _canUseOnObjects;
		}
		set
		{
			_canUseOnObjects = value;
		}
	}

	public bool SingleUse => _singleUse;

	public bool CanInteract
	{
		get
		{
			return _canInteract;
		}
		set
		{
			_canInteract = value;
		}
	}

	public bool CanDamage
	{
		get
		{
			return _canDamage;
		}
		set
		{
			_canDamage = value;
		}
	}

	public bool DestroyOnEmpty => _destroyOnEmpty;

	public float DurabilityPoints
	{
		get
		{
			return _durabilityPoints;
		}
		set
		{
			_durabilityPoints = value;
		}
	}

	public float OriginalDurabilityPoints { get; private set; }

	public float damage => _damage;

	public bool IsPickedUp { get; set; }

	public bool IsDragging { get; set; }

	public float CurrentDrag { get; set; }

	public float OriginalDrag { get; private set; }

	public float CurrentAngularDrag { get; set; }

	public float OriginalAngularDrag { get; private set; }

	public MaterialPropertyBlock PropertyBlock
	{
		get
		{
			if (_propertyBlock == null)
			{
				_propertyBlock = new MaterialPropertyBlock();
			}
			return _propertyBlock;
		}
	}

	public bool IsKinematic
	{
		get
		{
			return !_nonKinematic;
		}
		protected set
		{
			_nonKinematic = !value;
			this.SetKinematic(IsKinematic, base.IsOwner);
		}
	}

	CollisionDetectionMode IPhysical.CollisionDetectionMode
	{
		get
		{
			return _collisionDetectionMode;
		}
		set
		{
			_collisionDetectionMode = value;
		}
	}

	public Rigidbody rigidbody
	{
		get
		{
			if (_rigidbody == null)
			{
				_rigidbody = GetComponent<Rigidbody>();
			}
			return _rigidbody;
		}
	}

	public Collider[] colliders => _colliders;

	public Renderer[] renderers => _renderers;

	public LODGroup LodGroup => _lodGroup;

	public Selecting Selecting => _selecting;

	public Burning Burning
	{
		get
		{
			return _burning;
		}
		set
		{
			_burning = value;
		}
	}

	protected IPlayer Owner => _owner;

	public int OwnerId => PlayerUtilities.GetPlayerId_Safe(_owner);

	public int PreviousOwnerId { get; protected set; }

	public BaseActionUnityEvent Used => _used;

	public event Action<IPickupable> PickedUp;

	public event Action<IDraggable> BeginDragged;

	public event Action<IDraggable> EndDragged;

	public virtual void BeginDrag()
	{
		this.BeginDragged?.Invoke(this);
	}

	public virtual void EndDrag()
	{
		this.EndDragged?.Invoke(this);
	}

	protected override void Awake()
	{
		base.Awake();
		OriginalDurabilityPoints = _durabilityPoints;
		if (_rigidbody != null)
		{
			_isDraggable = _canDrag;
			OriginalDrag = _rigidbody.drag;
			OriginalAngularDrag = _rigidbody.angularDrag;
			CurrentDrag = OriginalDrag;
			CurrentAngularDrag = OriginalAngularDrag;
		}
		this.InitializeKinematic(IsKinematic, Game.Mode.IsMaster());
	}

	protected override void Start()
	{
		base.Start();
		EventManager.AddListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	private void EventManager_PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		OnPlayerDisconnect(disconnectedEvent);
	}

	protected virtual void OnPlayerDisconnect(PlayerDisconnectedEvent disconnectedEvent)
	{
		if (!Owner.IsNullOrDestroyed() && Owner.Id == disconnectedEvent.PlayerId)
		{
			Owner.Holder.ReleaseAndDestroy(this);
		}
	}

	public override void Attached()
	{
		base.Attached();
		this.InitializeKinematic(IsKinematic, base.IsOwner);
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		this.SetKinematic(IsKinematic, base.IsOwner);
	}

	public void SetOwner(IPlayer player)
	{
		_owner = player;
		if (player != null)
		{
			PreviousOwnerId = player.Id;
		}
	}

	public virtual void Hold(bool holding)
	{
		SetLOD((!holding) ? (-1) : 0);
	}

	public void SetLOD(int level = -1)
	{
		if (!(_lodGroup == null) && base.gameObject.activeInHierarchy)
		{
			_lodGroup.ForceLOD(level);
		}
	}

	public virtual bool Store()
	{
		IsPickedUp = true;
		this.PickedUp?.Invoke(this);
		colliders.EnableColliders(enabled: false, isTrigger: true);
		this.SetKinematic(kinematic: true, base.IsOwner);
		rigidbody.drag = OriginalDrag;
		CurrentDrag = OriginalDrag;
		return true;
	}

	public virtual void Release()
	{
		IsPickedUp = false;
		EnablePhysics();
	}

	public virtual bool ValidatePrimary(IBase obj)
	{
		return true;
	}

	public virtual void Use()
	{
		base.ActionEventData.Populate(this, null);
		base.ActionEventData.Position = Owner.transform.position;
		Used?.Invoke(this, base.ActionEventData);
	}

	public virtual void UseOnObject(IBase obj)
	{
		base.ActionEventData.Populate(this, obj);
		base.ActionEventData.Position = Owner.transform.position;
		Used?.Invoke(this, base.ActionEventData);
		DamageObject(obj);
	}

	public virtual bool Interact(IPlayer player)
	{
		return false;
	}

	public virtual bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	public virtual void DamageObject(IBase obj)
	{
		if (!_canDamage || !obj.IsDamageable)
		{
			return;
		}
		obj.Damage(_damage, this);
		if (_invincible)
		{
			return;
		}
		_durabilityPoints -= 1f;
		if (_durabilityPoints <= 0f)
		{
			if (!_destroyOnEmpty)
			{
				_durabilityPoints = 0f;
				base.DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -1);
				_canUse = false;
				_canUseOnObjects = false;
			}
			else
			{
				Destroy();
			}
		}
	}

	void ICraftingMaterial.Consume()
	{
		if (!Owner.IsNullOrDestroyed())
		{
			_owner.Holder.ReleaseAndDestroy(this);
			return;
		}
		SaveablePrefab @interface = base.gameObject.GetInterface<SaveablePrefab>();
		if (@interface.IsValid())
		{
			MultiplayerMng.Destroy(@interface);
		}
		else
		{
			BeamTeam.Destroy(base.gameObject);
		}
	}

	public override void Destroy()
	{
		if (!Owner.IsNullOrDestroyed())
		{
			_owner.Holder.ReleaseAndDestroy(this);
		}
		else
		{
			MultiplayerMng.Destroy(this);
		}
	}

	public virtual void OnDestroy()
	{
		this.PickedUp = null;
		this.BeginDragged = null;
		this.EndDragged = null;
		EventManager.RemoveListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	public virtual void SetRenderer(bool val)
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].enabled = val;
		}
	}

	public virtual void EnablePhysics()
	{
		CanDrag = _isDraggable;
		colliders.EnableColliders(enabled: true, isTrigger: false);
		this.CheckRigidbody(base.IsOwner);
		this.SetKinematic(IsKinematic, base.IsOwner);
	}

	public virtual void DisablePhysics()
	{
		CanDrag = false;
		colliders.EnableColliders(enabled: true, isTrigger: true);
		this.SetKinematic(kinematic: true, base.IsOwner);
	}

	public virtual void DisableCollisionsWithCollider(Collider collider)
	{
		for (int i = 0; i < base.AllColliders.Length; i++)
		{
			Physics.IgnoreCollision(base.AllColliders[i], collider, ignore: true);
		}
	}

	public virtual void EnableCollisionsWithCollider(Collider collider)
	{
		for (int i = 0; i < base.AllColliders.Length; i++)
		{
			Physics.IgnoreCollision(base.AllColliders[i], collider, ignore: false);
		}
	}

	public virtual string GetHealthTerm()
	{
		if (base.IsDamageable)
		{
			return "ITEM_DISPLAY_HEALTH_LABEL";
		}
		if (_canUse || _canUseOnObjects)
		{
			if (!_invincible)
			{
				return "ITEM_DISPLAY_DURABILITY_LABEL";
			}
			return "ITEM_DISPLAY_HEALTH_INVINCIBLE_LABEL";
		}
		return string.Empty;
	}

	public virtual float GetHealthUnits()
	{
		if (base.IsDamageable)
		{
			if (!Mathf.Approximately(base.HealthPoints, 0f))
			{
				return Mathf.Clamp01(base.HealthPoints / base.OriginalHealthPoints);
			}
			return 0f;
		}
		if (_canUse || _canUseOnObjects)
		{
			float result = (Mathf.Approximately(DurabilityPoints, 0f) ? 0f : Mathf.Clamp01(DurabilityPoints / OriginalDurabilityPoints));
			if (!_invincible)
			{
				return result;
			}
			return 1f;
		}
		return 1f;
	}

	public virtual float GetHealthRange()
	{
		return 1f;
	}

	public virtual string GetDisplayName()
	{
		return base.DisplayName;
	}

	string IDisplayableStorable.GetDescription()
	{
		return _descriptionTerm;
	}

	InteractiveObjectClassification ICraftingDisplayableStorable.GetClassification()
	{
		return _classification;
	}

	public virtual string GetInteractionDescription(int playerId, IBase obj)
	{
		if (!_canInteract)
		{
			return string.Empty;
		}
		return "ITEM_INTERACTION_DESCRIPTION_INTERACT";
	}

	public virtual string GetUsingDescription(IBase obj)
	{
		if (!_canUse)
		{
			return string.Empty;
		}
		return "ITEM_USING_DESCRIPTION_USE";
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if (_rigidbody != null)
		{
			JObject data = JSerializer.Serialize(_rigidbody);
			jObject.AddField("Rigidbody", data);
		}
		if (_colliders != null && _colliders.Length != 0)
		{
			JObject jObject2 = new JObject();
			for (int i = 0; i < _colliders.Length; i++)
			{
				JObject data2 = JSerializer.Serialize(_colliders[i]);
				jObject2.Add(data2);
			}
			jObject.AddField("Colliders", jObject2);
		}
		if ((bool)_burning)
		{
			JObject data3 = _burning.Save();
			jObject.AddField("Burning", data3);
		}
		jObject.AddField("durabilityPoints", JSerializer.Serialize(_durabilityPoints));
		jObject.AddField("isInteractive", JSerializer.Serialize(_canInteract));
		jObject.AddField("canUse", JSerializer.Serialize(_canUse));
		jObject.AddField("canUseOnObjects", JSerializer.Serialize(_canUseOnObjects));
		jObject.AddField("canDamage", JSerializer.Serialize(_canDamage));
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		if (_rigidbody != null)
		{
			LevelLoader.CurrentLoader.PhysicalLoading.Add(delegate
			{
				this.LoadRigidbody(data);
			});
		}
		if (_colliders != null && _colliders.Length != 0)
		{
			JObject field = data.GetField("Colliders");
			if (field != null && !field.IsNull())
			{
				int num = Mathf.Min(field.Children.Count, _colliders.Length);
				for (int i = 0; i < num; i++)
				{
					JObject data2 = field.Children[i];
					Collider instance = _colliders[i];
					JSerializer.DeserializeInto(data2, instance);
				}
			}
		}
		if ((bool)_burning)
		{
			JObject field2 = data.GetField("Burning");
			_burning.Load(field2);
		}
		float value = data.GetField("durabilityPoints").GetValue<float>();
		_durabilityPoints = value;
		bool value2 = data.GetField("isInteractive").GetValue<bool>();
		_canInteract = value2;
		bool value3 = data.GetField("canUse").GetValue<bool>();
		_canUse = value3;
		bool value4 = data.GetField("canUseOnObjects").GetValue<bool>();
		_canUseOnObjects = value4;
		bool value5 = data.GetField("canDamage").GetValue<bool>();
		_canDamage = value5;
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
