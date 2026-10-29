using System;
using System.Runtime.CompilerServices;
using Beam.Crafting;
using Beam.Language;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Intro;

public class Interactive_AIRCRAFT_STATION : ReplicableMonoBehaviour, IInteractable, IBase, IHasCraftingType, ISelectable, IHaveAttribute, IHaveFuel
{
	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _displayName;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	[Header("ATTRIBUTE")]
	[SerializeField]
	private IconType _type;

	private MiniGuid _referenceId;

	[Header("REFILLING VALUES")]
	[SerializeField]
	private float _maxValue = 1f;

	[SerializeField]
	private float _value = 0.7f;

	[SerializeField]
	private RefillerBase _refiller;

	[Header("STATION OBJECTIVE")]
	[SerializeField]
	private int _endGameObjectiveId = -1;

	private AircraftManager _aircraftManager;

	[Header("EVENTS")]
	public BoolUnityEvent Activated;

	public UnityEvent Filled;

	public string DisplayName => _displayName;

	public Selecting Selecting => _selecting;

	public IconType Type => _type;

	public Transform TransformParent { get; set; }

	public CraftingType CraftingType => CraftingType.Empty;

	public RaycastMode RaycastMode => RaycastMode.Collider;

	public bool CanParent { get; set; }

	public bool IsDamageable => false;

	public bool CanInteract { get; set; } = true;


	public override MiniGuid ReferenceId
	{
		get
		{
			if (_referenceId.IsDefault())
			{
				_referenceId = MiniGuid.AIRCRAFT_STATION(base.transform.position);
			}
			return _referenceId;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	float IHaveFuel.FuelCapacity => _maxValue;

	float IHaveFuel.Fuel => _value;

	public float GetValue()
	{
		return _value / _maxValue;
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		return _refiller.GetInteractionDescription(this, obj);
	}

	protected virtual void OnFilled()
	{
		CanInteract = false;
		Filled?.Invoke();
	}

	public void Initialize(AircraftManager aircraftManager)
	{
		_aircraftManager = aircraftManager;
		_aircraftManager.Proximity.EnteredProximity.AddListener(EndGameProximity_EnteredProximity);
		_aircraftManager.Proximity.LeftProximity.AddListener(EndGameProximity_LeftProximity);
		_aircraftManager.Objectives.ObjectivesCompleted.AddListener(EndGameObjectives_ObjectivesCompleted);
	}

	private void OnDestroy()
	{
		if (_aircraftManager != null)
		{
			_aircraftManager.Proximity?.EnteredProximity.RemoveListener(EndGameProximity_EnteredProximity);
			_aircraftManager.Proximity?.LeftProximity.RemoveListener(EndGameProximity_LeftProximity);
			_aircraftManager.Objectives?.ObjectivesCompleted.RemoveListener(EndGameObjectives_ObjectivesCompleted);
		}
	}

	public void Damage(float damage, IBase damager)
	{
	}

	public void Destroy()
	{
	}

	public void Parent()
	{
	}

	private void EndGameObjectives_ObjectivesCompleted(AircraftObjectivesCompletedArgs args)
	{
		Activated?.Invoke(arg0: false);
	}

	private void EndGameProximity_EnteredProximity()
	{
		Activated?.Invoke(arg0: true);
	}

	private void EndGameProximity_LeftProximity()
	{
		Activated?.Invoke(arg0: false);
	}

	public bool Interact(IPlayer player)
	{
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return _refiller.DoRefill(new RefillArgs
		{
			Player = player,
			Refillable = this,
			Obj = obj
		});
	}

	float IHaveFuel.Refill(float availableFuel)
	{
		float num = Mathf.Min(_maxValue - _value, availableFuel);
		if (num > 0f)
		{
			_value += num;
			_value = Mathf.Clamp(_value, 0f, _maxValue);
			if (_value == _maxValue)
			{
				_aircraftManager.Objectives.CompleteObjective(_endGameObjectiveId);
				OnFilled();
			}
		}
		return num;
	}

	public override JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Value", _value);
		return jObject;
	}

	public override void Load(JObject data)
	{
		_value = data.GetField("Value").GetValue<float>();
		if (_value == _maxValue)
		{
			OnFilled();
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	Transform IBase.get_transform()
	{
		return base.transform;
	}
}
