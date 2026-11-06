using System;
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
			if (_provenance != MeatProvenance.Other)
			{
				string text = Reflection.GetAttribute<DescriptionAttribute>(CraftingType.AttributeType).Text;
				string text2 = Reflection.GetAttribute<DescriptionAttribute>(Provenance).Text;
				string text3 = Reflection.GetAttribute<DescriptionAttribute>(CraftingType.InteractiveType).Text;
				string newDisplayName = (text + " " + text2 + " " + text3).Trim();
				SetNewDisplayName(newDisplayName);
			}
		}
	}

	public UnityEvent OnStored => _onStored;

	public event AttachableEventHandler<IAttachable> DetachedEvent;

	protected virtual void OnDetached(IAttachable sender)
	{
		DetachedEvent?.Invoke(sender);
		DetachedEvent = null;
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
			DisplayNamePrefixes.Remove("ITEM_DISPLAY_NAME_PREFIX_EMPTY");
		}
		else if (!_destroyOnEmptyServings && CanUse)
		{
			DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -2);
		}
	}

	public void CheckSpoiled()
	{
		if (!_spoiled && _spoilable && (!Object.op_Implicit((Object)(object)_cooking) || !_cooking.IsBeingCooked) && (!Object.op_Implicit((Object)(object)_smoking) || !_smoking.IsBeingSmoked) && (Provenance != MeatProvenance.Other || !IsGrowing) && GameTime.GetDaysSince(TimePicked) > 1f)
		{
			Spoil();
		}
	}

	public void Spoil()
	{
		if (!_spoiled)
		{
			DisplayNamePrefixes.Clear();
			DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_SPOILED", 0);
			if ((Object)(object)_cooking != (Object)null)
			{
				_cooking.SpoilManual();
			}
			ParticleManager.CreateParticle(Singleton<ParticleManager>.Instance.PARTICLE_FOOD_SPOIL, transform);
		}
		_spoiled = true;
		_ = IsPickedUp;
	}

	public override bool ValidatePrimary(IBase obj)
	{
		if (CraftingType.InteractiveType == InteractiveType.MEDICAL_ALOE)
		{
			Sunburn playerEffect = Owner.Statistics.GetPlayerEffect<Sunburn>();
			if ((Object)(object)playerEffect != (Object)null)
			{
				playerEffect.ResetExposure();
				return base.ValidatePrimary(obj);
			}
		}
		else if (_servings > 0 && Owner.Statistics.CanEat)
		{
			return base.ValidatePrimary(obj);
		}
		return false;
	}

	public override void Use()
	{
		if (!BeamTeam.IsNullOrDestroyed(Owner) && _servings > 0)
		{
			CheckSpoiled();
			float vomitChance = _vomitChance;
			if ((Object)(object)_cooking != (Object)null)
			{
				vomitChance = Cooking.GetCookingProgress();
			}
			if ((Object)(object)_smoking != (Object)null && _smoking.IsSmoked)
			{
				vomitChance = 0f;
			}
			if (_isPoisonous || _spoiled)
			{
				vomitChance = 100f;
			}
			if (_isPoisonous)
			{
				Owner.Statistics.ApplyStatusEffect(StatusEffect.POISON);
			}
			bool canCauseDiarrhoea = false;
			if (_diarrhoeaType != DiarrhoeaType.None)
			{
				canCauseDiarrhoea = _diarrhoeaType == DiarrhoeaType.Raw && ((Object)(object)Cooking == (Object)null || !Cooking.IsCooked);
			}
			Owner.Statistics.Eat(CraftingType.InteractiveType, Provenance, Mathf.Max(0, _caloriesPerServe), Mathf.Max(0, _fibres), Mathf.Max(0, _fats), Mathf.Max(0, _proteins), Mathf.Max(0, _hydrationPerServe), vomitChance, canCauseDiarrhoea);
			base.Use();
			_servings--;
			if (_servings == 0 && _destroyOnEmptyServings)
			{
				Owner.Holder.ReleaseAndDestroy(this);
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
		UnityEvent onStored = OnStored;
		if (onStored != null)
		{
			onStored.Invoke();
		}
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
			ActionEventData.Populate(this, null);
			((UnityEvent<IBase, IBaseActionEventData>)Severed)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
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
		Beam.Utilities.ExtensionMethods.CheckForProjectilesInChildren(transform);
		if (!((Object)(object)_splitting != (Object)null) || !_splitting.Split())
		{
			base.Destroy();
		}
	}

	public override void OnDestroy()
	{
		DetachedEvent = null;
		base.OnDestroy();
	}

	public override string GetDisplayName()
	{
		CheckDisplayName();
		return base.GetDisplayName();
	}

	public override string GetHealthTerm()
	{
		if (!CanUse)
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
		jObject.AddField("servings", JSerializer.Serialize((object)_servings));
		jObject.AddField("isAttached", JSerializer.Serialize((object)_isAttached));
		jObject.AddField("TimePicked", JSerializer.Serialize((object)TimePicked));
		jObject.AddField("Spoilable", JSerializer.Serialize((object)_spoilable));
		jObject.AddField("Spoiled", JSerializer.Serialize((object)_spoiled));
		jObject.AddField("Provenance", JSerializer.Serialize((object)Provenance));
		jObject.AddField("isGrowing", JSerializer.Serialize((object)_isGrowing));
		if ((Object)(object)_cooking != (Object)null)
		{
			JObject data = _cooking.Save();
			jObject.AddField("Cooking", data);
		}
		if ((Object)(object)_smoking != (Object)null)
		{
			JObject data2 = _smoking.Save();
			jObject.AddField("Smoking", data2);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (data.GetField("isAttached").GetValue<bool>())
		{
			Attach(new StaticAttachingAnchor(transform.position, transform.rotation));
		}
		JObject field = data.GetField("Spoilable");
		_spoilable = field.GetValue<bool>();
		JObject field2 = data.GetField("TimePicked");
		TimePicked = JSerializer.Deserialize<DateTime>(field2);
		JObject field3 = data.GetField("isGrowing");
		bool? flag = ((field3 != null) ? new bool?(field3.GetValue<bool>()) : ((bool?)null));
		if (flag.HasValue && flag == true)
		{
			_isGrowing = true;
		}
		if ((Object)(object)_cooking != (Object)null)
		{
			JObject field4 = data.GetField("Cooking");
			_cooking.Load(field4);
		}
		if ((Object)(object)_smoking != (Object)null)
		{
			JObject field5 = data.GetField("Smoking");
			_smoking.Load(field5);
		}
		CheckSpoiled();
		if (_spoiled && GameTime.GetDaysSince(TimePicked) > 3f)
		{
			LevelLoader.CurrentLoader.PostLoading.Add(() =>
			{
				if (!IsPickedUp)
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
		JObject field6 = data.GetField("Provenance");
		if (field6 != null)
		{
			Provenance = JSerializer.Deserialize<MeatProvenance>(field6);
		}
		if (CraftingType.InteractiveType == InteractiveType.FOOD_COCONUT_GREEN)
		{
			CanUse = true;
			_servings = 1;
		}
	}
}
