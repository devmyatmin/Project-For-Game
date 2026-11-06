using System;
using Beam.Crafting;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class Constructing_FUEL_STILL_COLLECTOR : ReplicableMonoBehaviour, IInteractable, IBase, IHasCraftingType, IHaveAttribute, IHaveFuel, ISelectable, ISaveable
{
	[SerializeField]
	private Constructing_FUEL_STILL_BOILER _constructingStillBoiler;

	private MiniGuid _referenceId;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	private float _fuel;

	private string _displayName = "";

	public DamageUnityEvent Damaged;

	public override MiniGuid ReferenceId
	{
		get
		{
			if (_referenceId.IsDefault() && BeamTeam.IsValid(_constructingStillBoiler.Structure))
			{
				_referenceId = MiniGuid.NewFrom(_constructingStillBoiler.Structure.ReferenceId);
			}
			return _referenceId;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public Selecting Selecting => _selecting;

	public IconType Type { get; } = IconType.Water;

	public float FuelCapacity => 4f;

	public float Fuel => _fuel;

	public bool CanInteract { get; set; } = true;

	public string DisplayName
	{
		get
		{
			if (string.IsNullOrEmpty(_displayName))
			{
				_displayName = CraftingType.ToString();
			}
			return _displayName;
		}
	}

	public bool CanParent { get; set; }

	public Transform TransformParent { get; set; }

	public CraftingType CraftingType { get; } = new CraftingType(AttributeType.None, InteractiveType.CRAFTING_FUEL_STILL_COLLECTOR);

	public RaycastMode RaycastMode { get; }

	public bool IsDamageable => true;

	public float GetValue()
	{
		return _fuel / FuelCapacity;
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		if (_fuel == 0f || BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		if (obj.CraftingType.InteractiveType == InteractiveType.TOOLS_JERRYCAN)
		{
			InteractiveObject_FUELCAN interactiveObject_FUELCAN = obj as InteractiveObject_FUELCAN;
			if ((Object)(object)interactiveObject_FUELCAN != (Object)null && interactiveObject_FUELCAN.CanRefill())
			{
				return "ITEM_INTERACTION_DESCRIPTION_REFILL";
			}
		}
		return string.Empty;
	}

	private void OnDestroy()
	{
		((UnityEventBase)Damaged).RemoveAllListeners();
		Damaged = null;
	}

	public void Damage(float damage, IBase damager)
	{
		((UnityEvent<DamageUnityEventArgs>)Damaged)?.Invoke(new DamageUnityEventArgs(damage, damager));
	}

	public void Destroy()
	{
	}

	public void Parent()
	{
	}

	public bool Interact(IPlayer player)
	{
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return false;
		}
		InteractiveObject_FUELCAN interactiveObject_FUELCAN = obj as InteractiveObject_FUELCAN;
		if ((Object)(object)interactiveObject_FUELCAN != (Object)null && interactiveObject_FUELCAN.CanRefill())
		{
			Fill(interactiveObject_FUELCAN);
			return true;
		}
		return Interact(player);
	}

	public void Refill(float availableFuel)
	{
		((IHaveFuel)this).Refill(availableFuel);
	}

	float IHaveFuel.Refill(float availableFuel)
	{
		float num = Mathf.Min(FuelCapacity - _fuel, availableFuel);
		if (num > 0f)
		{
			_fuel += num;
			_fuel = Mathf.Clamp(_fuel, 0f, FuelCapacity);
		}
		return num;
	}

	public float Fill(IHaveFuel fuelable)
	{
		int num = Mathf.CeilToInt(fuelable.Refill(_fuel));
		if (num > 0)
		{
			_fuel -= num;
			_fuel = Mathf.Clamp(_fuel, 0f, FuelCapacity);
		}
		return num;
	}

	public new JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Fuel", _fuel);
		return jObject;
	}

	public new void Load(JObject data)
	{
		int value = data.GetField("Fuel").GetValue<int>();
		_fuel = value;
	}
}
