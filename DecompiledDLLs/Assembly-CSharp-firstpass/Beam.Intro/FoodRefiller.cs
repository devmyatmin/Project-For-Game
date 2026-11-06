using Beam.UI;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Intro;

public class FoodRefiller : RefillerBase
{
	public override string GetInteractionDescription(IHaveFuel fuelable, IBase obj)
	{
		InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
		if ((Object)(object)interactiveObject_FOOD != (Object)null && interactiveObject_FOOD.Calories > 0)
		{
			return "AIRCRAFT_FOOD_REFILLER_INTERACTION_DESCRIPTION_STOCK_FOOD";
		}
		return string.Empty;
	}

	public override bool DoRefill(RefillArgs args)
	{
		if (!BeamTeam.IsNullOrDestroyed(args.Refillable) && args.Refillable.CanRefill())
		{
			InteractiveObject_FOOD interactiveObject_FOOD = args.Obj as InteractiveObject_FOOD;
			if ((Object)(object)interactiveObject_FOOD != (Object)null)
			{
				if (interactiveObject_FOOD.Spoilable && interactiveObject_FOOD.Spoiled)
				{
					LocalizedNotification.Post(args.Player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_NO_SPOILED_FOOD_DESC");
				}
				else if (interactiveObject_FOOD.Calories > 0)
				{
					args.Refillable.Refill(interactiveObject_FOOD.Servings);
					interactiveObject_FOOD.Servings = 0;
					interactiveObject_FOOD.Destroy();
					return true;
				}
			}
		}
		return false;
	}
}
