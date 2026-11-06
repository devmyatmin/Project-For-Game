using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Language;
using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beam;

public class BaseObject : SaveablePrefab, IBase, IHasCraftingType
{
	private Transform _transform;

	[SerializeField]
	[FormerlySerializedAs("_cType")]
	private CraftingType _craftingType = new CraftingType(AttributeType.INVALID, InteractiveType.INVALID);

	private ObservableSortedList<string, int> _displayNamePrefixes = new ObservableSortedList<string, int>(new SortedList<string, int>());

	private string _originalDisplayName;

	private string _cachedDisplayName;

	private string _displayName = "";

	[Header("INTERACTING")]
	[SerializeField]
	private RaycastMode _raycastMode = RaycastMode.Rigidbody;

	[SerializeField]
	private bool _isDamageable;

	[SerializeField]
	private float _healthPoints;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	[HideInInspector]
	private MaterialRefunder _materialRefunder;

	[FormerlySerializedAs("_onDamaged")]
	[SerializeField]
	[HideInInspector]
	private BaseActionUnityEvent _damaged;

	private BaseActionEventData _actionEventData = new BaseActionEventData();

	protected int LastPlayerIdToDamageThis = -1;

	public Transform transform => _transform ?? (_transform = ((Component)this).transform);

	public Transform TransformParent { get; set; }

	public CraftingType CraftingType => _craftingType;

	public ObservableSortedList<string, int> DisplayNamePrefixes => _displayNamePrefixes;

	[LocalizationTermPreview]
	protected string DisplayNameTerms => _craftingType.ToString();

	public string DisplayName
	{
		get
		{
			InitDisplayName();
			return _cachedDisplayName;
		}
		set
		{
			_displayName = value;
			OnDisplayNameChanged();
		}
	}

	public RaycastMode RaycastMode => _raycastMode;

	public bool IsDamageable
	{
		get
		{
			return _isDamageable;
		}
		set
		{
			_isDamageable = value;
		}
	}

	public float HealthPoints
	{
		get
		{
			return _healthPoints;
		}
		set
		{
			_healthPoints = value;
		}
	}

	protected float OriginalHealthPoints { get; set; }

	public BaseActionUnityEvent Damaged => _damaged;

	protected BaseActionEventData ActionEventData => _actionEventData;

	private void DisplayNamePrefixes_CollectionChanged()
	{
		CacheDisplayName();
	}

	private void InitDisplayName()
	{
		if (string.IsNullOrEmpty(_displayName))
		{
			SetNewDisplayName(DisplayNameTerms);
		}
	}

	private void CacheDisplayName()
	{
		if (DisplayNamePrefixes.Count > 0)
		{
			_cachedDisplayName = string.Join(" ", new object[2] { DisplayNamePrefixes, _displayName });
		}
		else
		{
			_cachedDisplayName = _displayName;
		}
	}

	protected virtual void OnDisplayNameChanged()
	{
		CacheDisplayName();
	}

	protected void SetNewDisplayName(string displayName)
	{
		_displayName = displayName;
		_originalDisplayName = _displayName;
		CacheDisplayName();
	}

	public bool HasCustomDisplayName()
	{
		return !_displayName.Equals(_originalDisplayName, StringComparison.InvariantCulture);
	}

	protected virtual void OnDamaged(float value, IBase damager)
	{
		_actionEventData.Populate(this, damager);
		_actionEventData.Value = value;
		((UnityEvent<IBase, IBaseActionEventData>)Damaged)?.Invoke((IBase)this, (IBaseActionEventData)_actionEventData);
	}

	protected virtual void Awake()
	{
		_transform = transform;
		OriginalHealthPoints = HealthPoints;
		InitDisplayName();
		DisplayNamePrefixes.CollectionChanged += DisplayNamePrefixes_CollectionChanged;
	}

	protected virtual void Start()
	{
		Parent();
	}

	public virtual void Parent()
	{
		if ((Object)(object)transform.parent == (Object)null)
		{
			if ((Object)(object)TransformParent == (Object)null)
			{
				TransformParent = Singleton<StrandedWorld>.Instance?.NmlZone.SaveContainer;
			}
			transform.parent = TransformParent;
		}
	}

	public virtual void Damage(float damage, IBase damager)
	{
		OnDamaged(damage, damager);
		IPickupable pickupable = damager as IPickupable;
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			LastPlayerIdToDamageThis = pickupable.PreviousOwnerId;
		}
		HealthPoints -= damage;
		if (HealthPoints <= 0f)
		{
			if ((Object)(object)_materialRefunder != (Object)null)
			{
				_materialRefunder.RefundMaterials(LastPlayerIdToDamageThis);
			}
			Destroy();
		}
	}

	public void Damage(DamageUnityEventArgs args)
	{
		Damage(args.Damage, args.Damager);
	}

	public virtual void Destroy()
	{
		BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = JSerializer.Serialize((Component)(object)transform);
		jObject.AddField("Transform", data);
		if (!_displayName.Equals(_originalDisplayName, StringComparison.InvariantCultureIgnoreCase))
		{
			jObject.AddField("dName", JSerializer.Serialize((object)_displayName));
		}
		if (!Mathf.Approximately(_healthPoints, OriginalHealthPoints))
		{
			jObject.AddField("healthPoints", JSerializer.Serialize((object)_healthPoints));
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JSerializer.DeserializeInto(data.GetField("Transform"), (Component)(object)transform);
		CompleteTransformSerialization();
		JObject field = data.GetField("dName");
		if (field != null)
		{
			string value = field.GetValue<string>();
			DisplayName = value;
		}
		JObject field2 = data.GetField("healthPoints");
		if (field2 != null)
		{
			_healthPoints = field2.GetValue<float>();
		}
	}
}
