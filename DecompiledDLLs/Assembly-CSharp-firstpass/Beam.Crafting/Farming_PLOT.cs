using System;
using Beam.Events;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Crafting;

[RequireComponent(typeof(Plot))]
public class Farming_PLOT : ConstructionObject, IHaveAttribute
{
	[Header("ATTRIBUTE")]
	[SerializeField]
	private IconType _type;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Plot _plot;

	[HideInInspector]
	public InteracterUnityEvent Planted;

	[HideInInspector]
	public UnityEvent Watered;

	[HideInInspector]
	public UnityEvent Cleared;

	IconType IHaveAttribute.Type => _type;

	public override string GetInteractionDescription(int playerId, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		if (obj.CraftingType.InteractiveType == InteractiveType.TOOLS_HOE && _plot.HasPlants())
		{
			return "ITEM_INTERACTION_DESCRIPTION_CLEAR";
		}
		if (IsRefillable(obj))
		{
			InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
			if ((Object)(object)interactiveObject_FOOD != (Object)null && interactiveObject_FOOD.Servings > 0 && _plot.CanWaterCrop())
			{
				return "ITEM_INTERACTION_DESCRIPTION_WATER";
			}
		}
		if (!_plot.Growing && (Object)(object)obj.gameObject.GetComponent<Plantable>() != (Object)null)
		{
			return "ITEM_INTERACTION_DESCRIPTION_PLANT " + obj.DisplayName;
		}
		return string.Empty;
	}

	float IHaveAttribute.GetValue()
	{
		return _plot.GetNormalizedWater();
	}

	protected override void Awake()
	{
		base.Awake();
		_plot.CropCreated += Plot_CropCreated;
	}

	public override void OnDestroy()
	{
		if ((Object)(object)_plot != (Object)null)
		{
			_plot.CropCreated -= Plot_CropCreated;
		}
		Planted = null;
		Watered = null;
		Cleared = null;
		base.OnDestroy();
	}

	private void Plot_CropCreated(PlantType type)
	{
		string text = Reflection.GetAttribute<DescriptionAttribute>(type).Text;
		DisplayNamePrefixes.AddOrIgnore(text + " - ", 0);
	}

	public override bool InteractWithObject(IPlayer player, IBase obj)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Plantable component = obj.gameObject.GetComponent<Plantable>();
		if ((Object)(object)component != (Object)null)
		{
			bool flag = _plot.Plant(component);
			if (flag)
			{
				EventManager.RaiseEvent(new CropPlantedEvent(transform.position, component.PlantType));
				player.Holder.ReleaseAndDestroy(obj as IPickupable);
				InteracterUnityEvent planted = Planted;
				if (planted == null)
				{
					return flag;
				}
				((UnityEvent<IPlayer>)planted).Invoke(player);
			}
			return flag;
		}
		if (obj is Constructable_HOE)
		{
			bool flag2 = _plot.ClearCrop();
			if (flag2)
			{
				DisplayNamePrefixes.Clear();
				UnityEvent cleared = Cleared;
				if (cleared == null)
				{
					return flag2;
				}
				cleared.Invoke();
			}
			return flag2;
		}
		if (IsRefillable(obj))
		{
			InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
			if ((Object)(object)interactiveObject_FOOD != (Object)null && interactiveObject_FOOD.Servings > 0)
			{
				bool flag3 = _plot.WaterCrop(1);
				if (flag3)
				{
					interactiveObject_FOOD.Servings--;
					UnityEvent watered = Watered;
					if (watered == null)
					{
						return flag3;
					}
					watered.Invoke();
				}
				return flag3;
			}
			return false;
		}
		return Interact(player);
	}

	public override void Damage(float damage, IBase damager)
	{
		if (_plot.HasPlants())
		{
			IPickupable pickupable = damager as IPickupable;
			if (!BeamTeam.IsNullOrDestroyed(pickupable))
			{
				IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
				if (!BeamTeam.IsNullOrDestroyed(playerFromId))
				{
					LocalizedNotification.Post(playerFromId, NotificationPriority.Low, 4f, "CONSTRUCTING_NTF_CANT_DESTROY_WITH_ATTACHED_DESC");
				}
			}
		}
		else
		{
			base.Damage(damage, damager);
		}
	}

	public override void Place()
	{
		base.Place();
		_plot.Place();
	}

	private bool IsRefillable(IBase obj)
	{
		if (obj.CraftingType.InteractiveType != InteractiveType.FOOD_WATER_BOTTLE && obj.CraftingType.InteractiveType != InteractiveType.FOOD_WATER_SKIN)
		{
			return obj.CraftingType.InteractiveType == InteractiveType.FOOD_COCONUT_FLASK;
		}
		return true;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		try
		{
			JObject jObject2 = _plot.Save();
			if (jObject2 != null)
			{
				jObject.AddField("Plot", jObject2);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Farming_PLOT::Save:: Error saving plot.\n" + ex));
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		try
		{
			JObject field = data.GetField("Plot");
			if (field != null && !field.IsNull())
			{
				_plot.Load(field);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Farming_PLOT::Load:: Error loading plot.\n" + ex));
		}
	}
}
