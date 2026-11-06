using System.Collections.Generic;
using Beam.Crafting;

namespace Beam;

public interface ICraftingMaterialProvider
{
	CraftingType MaterialsType { get; set; }

	IEnumerable<IPickupable> GetCraftingMaterials();
}
