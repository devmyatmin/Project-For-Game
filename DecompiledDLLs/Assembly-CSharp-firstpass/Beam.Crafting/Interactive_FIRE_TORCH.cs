using System;
using System.Collections;
using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Crafting;

public class Interactive_FIRE_TORCH : ConstructionObject, IIgnitable, IBase, IHasCraftingType, IFlammable, IAttachable, IPickupable, IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial, IPhysical, IHaveAttribute, IInteractable
{
	[Header("ATTRIBUTE")]
	[SerializeField]
	private IconType _type;

	[SerializeField]
	private Light _light;

	[SerializeField]
	private ParticleSystem[] _particles;

	[SerializeField]
	private float _fuelHours;

	[Header("ATTACHING")]
	[SerializeField]
	private bool _canAttach;

	[SerializeField]
	private StaticAttachingAnchor _attachingAnchor;

	private bool _isBurning;

	private bool _isAttached;

	private float _originalFuelHours;

	[HideInInspector]
	public BaseActionUnityEvent Ignited;

	[HideInInspector]
	public BaseActionUnityEvent Extinguished;

	IconType IHaveAttribute.Type => _type;

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

	public bool IsBurning => _isBurning;

	public bool IsAttached => _isAttached;

	public event AttachableEventHandler<IAttachable> DetachedEvent;

	public override string GetInteractionDescription(int playerId, IBase obj)
	{
		if (_isBurning)
		{
			return "ITEM_INTERACTION_DESCRIPTION_EXTINGUISH";
		}
		return string.Empty;
	}

	public override string GetHealthTerm()
	{
		return "ITEM_DISPLAY_HEALTH_FUEL_LABEL";
	}

	public override float GetHealthUnits()
	{
		if (!Mathf.Approximately(_fuelHours, 0f))
		{
			return Mathf.Clamp01(_fuelHours / _originalFuelHours);
		}
		return 0f;
	}

	float IHaveAttribute.GetValue()
	{
		return _fuelHours / _originalFuelHours;
	}

	protected virtual void OnDetached(IAttachable sender)
	{
		DetachedEvent?.Invoke(sender);
		DetachedEvent = null;
	}

	protected override void Start()
	{
		base.Start();
		_originalFuelHours = _fuelHours;
		if (!LevelLoader.IsLoading())
		{
			Initialize();
		}
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		Initialize();
	}

	private void Initialize()
	{
		UpdateTexture();
		Singleton<GameTime>.Instance.HourChanged += GameTime_HourChanged;
	}

	public override void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)Singleton<GameTime>.Instance))
		{
			Singleton<GameTime>.Instance.HourChanged -= GameTime_HourChanged;
		}
		DetachedEvent = null;
		base.OnDestroy();
	}

	private void GameTime_HourChanged(float hours)
	{
		if (_isBurning)
		{
			BurnManual(hours);
		}
	}

	public override bool Store()
	{
		Detach();
		if (!BeamTeam.IsNullOrDestroyed(Owner) && Owner.Holder.Storage.Has(this))
		{
			Extinguish();
		}
		return base.Store();
	}

	public void Attach(IAttachingAnchor anchor)
	{
		if (!_isAttached)
		{
			_isAttached = true;
			DisablePhysics();
		}
	}

	public void Detach()
	{
		if (_isAttached)
		{
			_isAttached = false;
			OnDetached(this);
		}
	}

	public override bool Interact(IPlayer player)
	{
		if (_isBurning)
		{
			Extinguish();
		}
		return true;
	}

	public override bool InteractWithObject(IPlayer player, IBase obj)
	{
		if (_isBurning)
		{
			IIgnitable ignitable = obj as IIgnitable;
			if (!BeamTeam.IsNullOrDestroyed(ignitable))
			{
				ignitable.Ignite();
				return true;
			}
		}
		else
		{
			IFlammable flammable = obj as IFlammable;
			if (!BeamTeam.IsNullOrDestroyed(flammable) && flammable.IsBurning)
			{
				Ignite();
				return true;
			}
		}
		return false;
	}

	public void Ignite()
	{
		if (!_isBurning && _fuelHours > 0f)
		{
			((MonoBehaviour)this).StartCoroutine(Burn());
			ActionEventData.Populate(this, null);
			((UnityEvent<IBase, IBaseActionEventData>)Ignited)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
		}
	}

	private void BurnManual(float hours)
	{
		Mathf.Clamp(_fuelHours, 0f, hours);
		_fuelHours -= hours;
		_fuelHours = Mathf.Max(0f, _fuelHours);
	}

	private IEnumerator Burn()
	{
		_isBurning = true;
		DisplayBurnVisuals();
		while (_isBurning)
		{
			RemoveFuel();
			if (transform.position.y < 0f)
			{
				Extinguish();
			}
			yield return null;
		}
	}

	private void DisplayBurnVisuals()
	{
		((Component)_light).gameObject.SetActive(true);
		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i].Play();
		}
	}

	private void OnEnable()
	{
		if (_isBurning)
		{
			((MonoBehaviour)this).StartCoroutine(Burn());
			DisplayBurnVisuals();
		}
	}

	private void RemoveFuel()
	{
		_fuelHours -= 1f * GameTime.DeltaTimeHour;
		CheckFuel();
		UpdateTexture();
	}

	private void CheckFuel()
	{
		if (_fuelHours <= 0f)
		{
			DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", 0);
			Extinguish();
		}
	}

	private void Extinguish()
	{
		if (_isBurning)
		{
			_isBurning = false;
			((Component)_light).gameObject.SetActive(false);
			for (int i = 0; i < _particles.Length; i++)
			{
				_particles[i].Stop();
			}
			ActionEventData.Populate(this, null);
			((UnityEvent<IBase, IBaseActionEventData>)Extinguished)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
		}
	}

	private void UpdateTexture()
	{
		float num = 1f - BeamMath.Normalize(_fuelHours, 0f, _originalFuelHours);
		renderers[0].material.SetFloat("_Blend", num);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("FuelHours", JSerializer.Serialize((object)_fuelHours));
		jObject.AddField("IsBurning", JSerializer.Serialize((object)_isBurning));
		jObject.AddField("TimeLeft", JSerializer.Serialize((object)GameTime.Now));
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		_fuelHours = data.GetField("FuelHours").GetValue<float>();
		_isBurning = data.GetField("IsBurning").GetValue<bool>();
		if (_isBurning)
		{
			DateTime a = JSerializer.Deserialize<DateTime>(data.GetField("TimeLeft"));
			float elapsed = GameTime.GetHoursSince(a);
			LevelLoader.CurrentLoader.PostLoading.Add(() =>
			{
				BurnManual(elapsed);
				if (_fuelHours > 0f)
				{
					((MonoBehaviour)this).StartCoroutine(Burn());
				}
				else
				{
					CheckFuel();
				}
			});
		}
		CheckFuel();
		Initialize();
	}
}
