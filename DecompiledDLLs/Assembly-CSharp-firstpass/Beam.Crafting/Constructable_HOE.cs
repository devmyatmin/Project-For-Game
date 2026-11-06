namespace Beam.Crafting;

public class Constructable_HOE : ConstructionObject
{
	public override void Hold(bool holding)
	{
		Owner.Crafter.State = (holding ? CrafterState.FARMING : CrafterState.CRAFTING);
		base.Hold(holding);
	}
}
