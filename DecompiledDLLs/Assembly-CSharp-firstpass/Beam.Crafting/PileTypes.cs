using System.Collections.Generic;

namespace Beam.Crafting;

public static class PileTypes
{
	private static readonly IList<CraftingType> PILE_TYPES = new List<CraftingType>
	{
		new CraftingType(AttributeType.None, InteractiveType.CRAFTING_STICK),
		new CraftingType(AttributeType.None, InteractiveType.CRAFTING_LOG),
		new CraftingType(AttributeType.None, InteractiveType.CRAFTING_ROCK),
		new CraftingType(AttributeType.None, InteractiveType.CRAFTING_LEAVES),
		new CraftingType(AttributeType.None, InteractiveType.CRAFTING_LASHING),
		new CraftingType(AttributeType.None, InteractiveType.CRAFTING_CLAY),
		new CraftingType(AttributeType.None, InteractiveType.CRAFTING_PALM_FROND),
		new CraftingType(AttributeType.None, InteractiveType.FOOD_COCONUT_GREEN),
		new CraftingType(AttributeType.None, InteractiveType.FOOD_COCONUT_DRINKABLE)
	};

	public static bool IsPileType(this CraftingType craftingType)
	{
		return PILE_TYPES.Contains(craftingType);
	}
}
