using System.Collections.Generic;

namespace Beam.Crafting;

public class CraftingInfo
{
	public int CurrentCraftsmanshipLevel;

	public CraftingType CurrentType;

	public uint CurrentPrefabId;

	public IEnumerable<CraftingType> CurrentMaterials;
}
