using Beam.Crafting;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Intro;

public class PartRefiller : RefillerBase, ICraftingTypeProvider
{
	[SerializeField]
	private CraftingType _partRequired;

	public CraftingType PartRequired => _partRequired;

	public CraftingType GetCraftingType()
	{
		return _partRequired;
	}

	public override string GetInteractionDescription(IHaveFuel fuelable, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		if (!(obj.CraftingType == _partRequired))
		{
			return string.Empty;
		}
		return "AIRCRAFT_PART_REFILLER_INTERACTION_DESCRIPTION_FIX";
	}

	public override bool DoRefill(RefillArgs args)
	{
		if (!BeamTeam.IsNullOrDestroyed(args.Refillable) && args.Refillable.CanRefill() && !BeamTeam.IsNullOrDestroyed(args.Obj) && args.Obj.CraftingType == _partRequired)
		{
			args.Refillable.Refill(args.Refillable.FuelCapacity);
			args.Obj.Destroy();
			return true;
		}
		return false;
	}
}
