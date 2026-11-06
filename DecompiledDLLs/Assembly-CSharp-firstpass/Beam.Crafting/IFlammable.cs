namespace Beam.Crafting;

public interface IFlammable : IBase, IHasCraftingType
{
	bool IsBurning { get; }
}
