using System;
using Beam.Crafting;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class Constructing_STILL_COLLECTOR : ReplicableMonoBehaviour, IInteractable, IBase, IHasCraftingType, IHaveAttribute, ISelectable, ISaveable
{
	private const int MAX_WATER = 4;

	[SerializeField]
	private Constructing_STILL _constructingStill;

	private MiniGuid _referenceId;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	[Header("ATTRIBUTE")]
	[SerializeField]
	private IconType _type;

	private int _water;

	private string _displayName = "";

	public DamageUnityEvent Damaged;

	public UnityEvent Drank;

	public UnityEvent WaterRemoved;

	public int MaxWater => 4;

	public override MiniGuid ReferenceId
	{
		get
		{
			if (_referenceId.IsDefault() && BeamTeam.IsValid(_constructingStill.Structure))
			{
				_referenceId = MiniGuid.NewFrom(_constructingStill.Structure.ReferenceId);
			}
			return _referenceId;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public Selecting Selecting => _selecting;

	IconType IHaveAttribute.Type => _type;

	public int Water => _water;

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

	public CraftingType CraftingType { get; } = new CraftingType(AttributeType.None, InteractiveType.CRAFTING_WATER_STILL_COLLECTOR);

	public RaycastMode RaycastMode { get; }

	public bool IsDamageable => true;

	float IHaveAttribute.GetValue()
	{
		return (float)_water / 4f;
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return "ITEM_INTERACTION_DESCRIPTION_DRINK";
		}
		if (IsRefillable(obj))
		{
			return "ITEM_INTERACTION_DESCRIPTION_COLLECT";
		}
		return "ITEM_INTERACTION_DESCRIPTION_DRINK";
	}

	private void OnDestroy()
	{
		((UnityEventBase)Damaged).RemoveAllListeners();
		Damaged = null;
		Drank = null;
		WaterRemoved = null;
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
		if (_water <= 0)
		{
			LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "WATER_STILL_COLLECTOR_NTF_EMPTY_DESC");
			return false;
		}
		Drink(player);
		return true;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return false;
		}
		if (_water <= 0)
		{
			LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "WATER_STILL_COLLECTOR_NTF_EMPTY_DESC");
			return false;
		}
		InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
		if ((Object)(object)interactiveObject_FOOD != (Object)null && IsRefillable(interactiveObject_FOOD))
		{
			if (interactiveObject_FOOD.Servings < interactiveObject_FOOD.OriginalServings)
			{
				interactiveObject_FOOD.Servings++;
				interactiveObject_FOOD.Servings = Mathf.Clamp(interactiveObject_FOOD.Servings, 0, interactiveObject_FOOD.OriginalServings);
				RemoveWater(1);
				return true;
			}
			LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "WATER_STILL_COLLECTOR_NTF_FULL_DESC");
			return false;
		}
		return Interact(player);
	}

	private void RemoveWater(int qty)
	{
		_water -= qty;
		UnityEvent waterRemoved = WaterRemoved;
		if (waterRemoved != null)
		{
			waterRemoved.Invoke();
		}
	}

	private void Drink(IPlayer player)
	{
		RemoveWater(1);
		int num = 100;
		player.Statistics.Eat(InteractiveType.CRAFTING_WATER_STILL, MeatProvenance.Other, 0, 0, 0, 0, num);
		UnityEvent drank = Drank;
		if (drank != null)
		{
			drank.Invoke();
		}
	}

	public void Refill(float availableWater)
	{
		int num = Mathf.Min(4 - _water, (int)availableWater);
		if (num > 0)
		{
			_water += num;
			_water = Mathf.Clamp(_water, 0, 4);
		}
	}

	private bool IsRefillable(IBase obj)
	{
		if (obj.CraftingType.InteractiveType != InteractiveType.FOOD_WATER_BOTTLE && obj.CraftingType.InteractiveType != InteractiveType.FOOD_WATER_SKIN)
		{
			return obj.CraftingType.InteractiveType == InteractiveType.FOOD_COCONUT_FLASK;
		}
		return true;
	}

	public new JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Water", _water);
		return jObject;
	}

	public new void Load(JObject data)
	{
		int value = data.GetField("Water").GetValue<int>();
		_water = value;
	}
}
