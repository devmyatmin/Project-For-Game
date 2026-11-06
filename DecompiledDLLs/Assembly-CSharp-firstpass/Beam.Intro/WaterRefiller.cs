using Beam.Utilities;
using UnityEngine;

namespace Beam.Intro;

public class WaterRefiller : RefillerBase
{
	public override string GetInteractionDescription(IHaveFuel fuelable, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		if (!IsRefillable(obj))
		{
			return string.Empty;
		}
		return "AIRCRAFT_WATER_REFILLER_INTERACTION_DESCRIPTION_STOCK_WATER";
	}

	public override bool DoRefill(RefillArgs args)
	{
		if (!BeamTeam.IsNullOrDestroyed(args.Refillable) && args.Refillable.CanRefill())
		{
			InteractiveObject_FOOD interactiveObject_FOOD = args.Obj as InteractiveObject_FOOD;
			if ((Object)(object)interactiveObject_FOOD != (Object)null && IsRefillable(interactiveObject_FOOD))
			{
				float num = args.Refillable.Refill(interactiveObject_FOOD.Servings);
				interactiveObject_FOOD.Servings = Mathf.RoundToInt((float)interactiveObject_FOOD.Servings - num);
				return true;
			}
		}
		return false;
	}

	private bool IsRefillable(IBase obj)
	{
		if (obj.CraftingType.InteractiveType != InteractiveType.FOOD_WATER_BOTTLE && obj.CraftingType.InteractiveType != InteractiveType.FOOD_WATER_SKIN)
		{
			return obj.CraftingType.InteractiveType == InteractiveType.FOOD_COCONUT_FLASK;
		}
		return true;
	}
}
