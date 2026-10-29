using Beam.Crafting;
using UnityEngine;

namespace Beam;

public class SimpleCraftingTypeProvider : ICraftingTypeProvider
{
	[SerializeField]
	private CraftingType _craftingType;

	public CraftingType GetCraftingType()
	{
		return _craftingType;
	}
}
