using Beam.Crafting;

namespace Beam.UI;

public class MaterialInfo
{
	public readonly CraftingType Type;

	public readonly int Required;

	public readonly Property<int> Available;

	public MaterialInfo(CraftingType type, int required, int available)
	{
		Type = type;
		Required = required;
		Available = new Property<int>(available);
	}
}
