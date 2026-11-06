namespace Beam;

public interface ISelectable : IBase, IHasCraftingType
{
	Selecting Selecting { get; }
}
