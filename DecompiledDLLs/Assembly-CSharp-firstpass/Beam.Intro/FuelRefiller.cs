using Beam.Utilities;
using UnityEngine;

namespace Beam.Intro;

public class FuelRefiller : RefillerBase
{
	public override string GetInteractionDescription(IHaveFuel fuelable, IBase obj)
	{
		if (!BeamTeam.IsNullOrDestroyed(fuelable) && fuelable.CanRefill() && !BeamTeam.IsNullOrDestroyed(obj) && obj.CraftingType.InteractiveType == InteractiveType.TOOLS_JERRYCAN)
		{
			InteractiveObject_FUELCAN interactiveObject_FUELCAN = obj as InteractiveObject_FUELCAN;
			if ((Object)(object)interactiveObject_FUELCAN != (Object)null && interactiveObject_FUELCAN.Fuel > 0f)
			{
				return "AIRCRAFT_FUEL_REFILLER_INTERACTION_DESCRIPTION_REFILL_PLANE";
			}
		}
		return string.Empty;
	}

	public override bool DoRefill(RefillArgs args)
	{
		if (!BeamTeam.IsNullOrDestroyed(args.Refillable) && args.Refillable.CanRefill() && !BeamTeam.IsNullOrDestroyed(args.Obj) && args.Obj.CraftingType.InteractiveType == InteractiveType.TOOLS_JERRYCAN)
		{
			InteractiveObject_FUELCAN interactiveObject_FUELCAN = args.Obj as InteractiveObject_FUELCAN;
			if ((Object)(object)interactiveObject_FUELCAN != (Object)null && interactiveObject_FUELCAN.Fuel > 0f)
			{
				interactiveObject_FUELCAN.Fill(args.Refillable);
				return true;
			}
		}
		return false;
	}
}
