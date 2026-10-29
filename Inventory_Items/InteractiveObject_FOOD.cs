using System;
using System.Runtime.CompilerServices;
using Beam.Serialization.Json;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class InteractiveObject_FOOD : InteractiveObject, ISmokable, IAttachable, IPickupable, IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial, IPhysical, ISplittable
{
	private enum DiarrhoeaType
	{
		None,
		Raw
	}

	protected const float DAYS_TO_SPOIL = 1f;

	protected const float DAYS_TO_DESTROY = 3f;

	[SerializeField]
	private int _calories;

	[SerializeField]
	private int _fibres;

	[SerializeField]
	private int _fats;

	[SerializeField]
	private int _proteins;

	[SerializeField]
	private int _hydration;

	[SerializeField]
	private int _servings;

	[SerializeField]
	private int _caloriesPerServe;

	[SerializeField]
	private int _fibresPerServe;

	[SerializeField]
	private int _fatPerServe;

	[SerializeField]
	private int _proteinPerServe;

	[SerializeField]
	private int _hydrationPerServe;

	[SerializeField]
	private bool _startWithEmptyServings;

	[SerializeField]
	private bool _destroyOnEmptyServings = true;

	[SerializeField]
	private float _vomitChance;

	[SerializeField]
	private DiarrhoeaType _diarrhoeaType;

	[SerializeField]
	private bool _isPoisonous;

	[Header("ATTACHING")]
	[SerializeField]
	private bool _isAttached;

	[SerializeField]
	private bool _canAttach;

	[SerializeField]
	private StaticAttachingAnchor _attachingAnchor;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Cooking _cooking;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Smoking _smoking;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Splitting _splitting;

	[SerializeField]
	private bool _spoiled;

	[SerializeField]
	private bool _spoilable;

	[SerializeField]
	private bool _isGrowing;

	[SerializeField]
	private MeatProvenance _provenance;

	[SerializeField]
	private UnityEvent _onStored;

	[HideInInspector]
	public BaseActionUnityEvent Severed;

	protected bool _formatted;

	public int Calories
	{
		get
		{
			return _calories;
		}
		set
		{
			_calories = value;
		}
	}

	public int Fibres => _fibres;

	public int Fats => _fats;

	public int Proteins => _proteins;

	public int Hydration
	{
		get
		{
			return _hydration;
		}
		set
		{
			_hydration = value;
		}
	}

	public int Servings
	{
		get
		{
			return _servings;
		}
		set
		{
			_servings = value;
		}
	}

	public int HydrationPerServe => _hydrationPerServe;

	public bool IsAttached
	{
		get
		{
			return _isAttached;
		}
		protected set
		{
			_isAttached = value;
		}
	}

	public bool CanAttach
	{
		get
		{
			return _canAttach;
		}
		set
		{
			_canAttach = value;
		}
	}

	public IAttachingAnchor AttachingAnchor => _attachingAnchor;

	public Cooking Cooking => _cooking;

	public Smoking Smoking => _smoking;

	Splitting ISplittable.Splitting => _splitting;

	public bool Spoiled => _spoiled;

	public bool Spoilable
	{
		get
		{
			return _spoilable;
		}
		set
		{
			_spoilable = value;
		}
	}

	public bool IsGrowing
	{
		get
		{
			return _isGrowing;
		}
		set
		{
			_isGrowing = value;
		}
	}

	public int OriginalServings { get; private set; }

	public DateTime TimePicked { get; set; }

	public MeatProvenance Provenance
	{
		get
		{
			return _provenance;
		}
		set
		{
			_provenance = value;
			if (_provenance != 0)
			{
				string text = base.CraftingType.AttributeType.GetAttribute<DescriptionAttribute>().Text;
				string text2 = Provenance.GetAttribute<DescriptionAttribute>().Text;
				string text3 = base.CraftingType.InteractiveType.GetAttribute<DescriptionAttribute>().Text;
				string newDisplayName = (text + " " + text2 + " " + text3).Trim();
				SetNewDisplayName(newDisplayName);
			}
		}
	}

	public UnityEvent OnStored => _onStored;

	public event AttachableEventHandler<IAttachable> DetachedEvent;

	protected virtual void OnDetached(IAttachable sender)
	{
		this.DetachedEvent?.Invoke(sender);
		this.DetachedEvent = null;
	}

	protected override void Awake()
	{
		base.Awake();
		OriginalServings = _servings;
		if (_calories > 0 && _servings > 0)
		{
			_caloriesPerServe = _calories / _servings;
		}
		if (_hydration > 0 && _servings > 0)
		{
			_hydrationPerServe = _hydration / _servings;
		}
		if (_startWithEmptyServings)
		{
			_servings = 0;
		}
		TimePicked = GameTime.Now;
	}

	private void Selecting_Selected(IPlayer player)
	{
		CheckDisplayName();
	}

	public override void Hold(bool holding)
	{
		if (holding)
		{
			IsGrowing = false;
			CheckSpoiled();
		}
		base.Hold(holding);
	}

	public override bool Interact(IPlayer player)
	{
		if (_isAttached && player.Holder.Pickup(this))
		{
			return true;
		}
		return false;
	}

	private void CheckDisplayName()
	{
		CheckSpoiled();
		if (Servings > 0)
		{
			base.DisplayNamePrefixes.Remove("ITEM_DISPLAY_NAME_PREFIX_EMPTY");
		}
		else if (!_destroyOnEmptyServings && base.CanUse)
		{
			base.DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -2);
		}
	}

	public void CheckSpoiled()
	{
		if (!_spoiled && _spoilable && (!_cooking || !_cooking.IsBeingCooked) && (!_smoking || !_smoking.IsBeingSmoked) && (Provenance != 0 || !IsGrowing) && GameTime.GetDaysSince(TimePicked) > 1f)
		{
			Spoil();
		}
	}

	public void Spoil()
	{
		if (!_spoiled)
		{
			base.DisplayNamePrefixes.Clear();
			base.DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_SPOILED", 0);
			if (_cooking != null)
			{
				_cooking.SpoilManual();
			}
			ParticleManager.CreateParticle(Singleton<ParticleManager>.Instance.PARTICLE_FOOD_SPOIL, base.transform);
		}
		_spoiled = true;
		_ = base.IsPickedUp;
	}

	public override bool ValidatePrimary(IBase obj)
	{
		if (base.CraftingType.InteractiveType == InteractiveType.MEDICAL_ALOE)
		{
			Sunburn playerEffect = base.Owner.Statistics.GetPlayerEffect<Sunburn>();
			if (playerEffect != null)
			{
				playerEffect.ResetExposure();
				return base.ValidatePrimary(obj);
			}
		}
		else if (_servings > 0 && base.Owner.Statistics.CanEat)
		{
			return base.ValidatePrimary(obj);
		}
		return false;
	}

	public override void Use()
	{
		if (!base.Owner.IsNullOrDestroyed() && _servings > 0)
		{
			CheckSpoiled();
			float vomitChance = _vomitChance;
			if (_cooking != null)
			{
				vomitChance = Cooking.GetCookingProgress();
			}
			if (_smoking != null && _smoking.IsSmoked)
			{
				vomitChance = 0f;
			}
			if (_isPoisonous || _spoiled)
			{
				vomitChance = 100f;
			}
			if (_isPoisonous)
			{
				base.Owner.Statistics.ApplyStatusEffect(StatusEffect.POISON);
			}
			bool canCauseDiarrhoea = false;
			if (_diarrhoeaType != 0)
			{
				canCauseDiarrhoea = _diarrhoeaType == DiarrhoeaType.Raw && (Cooking == null || !Cooking.IsCooked);
			}
			base.Owner.Statistics.Eat(base.CraftingType.InteractiveType, Provenance, Mathf.Max(0, _caloriesPerServe), Mathf.Max(0, _fibres), Mathf.Max(0, _fats), Mathf.Max(0, _proteins), Mathf.Max(0, _hydrationPerServe), vomitChance, canCauseDiarrhoea);
			base.Use();
			_servings--;
			if (_servings == 0 && _destroyOnEmptyServings)
			{
				base.Owner.Holder.ReleaseAndDestroy(this);
			}
		}
	}

	public virtual void Kill()
	{
		TimePicked = GameTime.Now;
	}

	public override bool Store()
	{
		Detach();
		OnStored?.Invoke();
		return base.Store();
	}

	public virtual void Attach(IAttachingAnchor anchor)
	{
		if (!_isAttached)
		{
			_isAttached = true;
			DisablePhysics();
			CanDrag = false;
		}
	}

	public virtual void Detach()
	{
		if (_isAttached)
		{
			_isAttached = false;
			IsGrowing = false;
			OnDetached(this);
			base.ActionEventData.Populate(this, null);
			Severed?.Invoke(this, base.ActionEventData);
			CanDrag = true;
			TimePicked = GameTime.Now;
		}
	}

	public void DetachFromImpact()
	{
		Detach();
		EnablePhysics();
	}

	public override void Damage(float damage, IBase damager)
	{
		if (_isAttached)
		{
			DetachFromImpact();
		}
		else
		{
			base.Damage(damage, damager);
		}
	}

	public override void Destroy()
	{
		base.transform.CheckForProjectilesInChildren();
		if (!(_splitting != null) || !_splitting.Split())
		{
			base.Destroy();
		}
	}

	public override void OnDestroy()
	{
		this.DetachedEvent = null;
		base.OnDestroy();
	}

	public override string GetDisplayName()
	{
		CheckDisplayName();
		return base.GetDisplayName();
	}

	public override string GetHealthTerm()
	{
		if (!base.CanUse)
		{
			return string.Empty;
		}
		return "ITEM_DISPLAY_HEALTH_SERVINGS_LABEL";
	}

	public override float GetHealthUnits()
	{
		if (_servings != 0)
		{
			return Mathf.Clamp01((float)_servings / (float)OriginalServings);
		}
		return 0f;
	}

	public override float GetHealthRange()
	{
		return OriginalServings;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("servings", JSerializer.Serialize(_servings));
		jObject.AddField("isAttached", JSerializer.Serialize(_isAttached));
		jObject.AddField("TimePicked", JSerializer.Serialize(TimePicked));
		jObject.AddField("Spoilable", JSerializer.Serialize(_spoilable));
		jObject.AddField("Spoiled", JSerializer.Serialize(_spoiled));
		jObject.AddField("Provenance", JSerializer.Serialize(Provenance));
		jObject.AddField("isGrowing", JSerializer.Serialize(_isGrowing));
		if (_cooking != null)
		{
			JObject data = _cooking.Save();
			jObject.AddField("Cooking", data);
		}
		if (_smoking != null)
		{
			JObject data2 = _smoking.Save();
			jObject.AddField("Smoking", data2);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		if (data.GetField("isAttached").GetValue<bool>())
		{
			Attach(new StaticAttachingAnchor(base.transform.position, base.transform.rotation));
		}
		JObject field = data.GetField("Spoilable");
		_spoilable = field.GetValue<bool>();
		JObject field2 = data.GetField("TimePicked");
		TimePicked = JSerializer.Deserialize<DateTime>(field2);
		bool? flag = data.GetField("isGrowing")?.GetValue<bool>();
		if (flag.HasValue && flag == true)
		{
			_isGrowing = true;
		}
		if (_cooking != null)
		{
			JObject field3 = data.GetField("Cooking");
			_cooking.Load(field3);
		}
		if (_smoking != null)
		{
			JObject field4 = data.GetField("Smoking");
			_smoking.Load(field4);
		}
		CheckSpoiled();
		if (_spoiled && GameTime.GetDaysSince(TimePicked) > 3f)
		{
			LevelLoader.CurrentLoader.PostLoading.Add(delegate
			{
				if (!base.IsPickedUp)
				{
					MultiplayerMng.Destroy(this, DestroyDelay.LATE);
				}
			});
		}
		base.Load(data);
		int value = data.GetField("servings").GetValue<int>();
		_servings = value;
		if (data.GetField("Spoiled").GetValue<bool>())
		{
			Spoil();
		}
		JObject field5 = data.GetField("Provenance");
		if (field5 != null)
		{
			Provenance = JSerializer.Deserialize<MeatProvenance>(field5);
		}
		if (base.CraftingType.InteractiveType == InteractiveType.FOOD_COCONUT_GREEN)
		{
			base.CanUse = true;
			_servings = 1;
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
