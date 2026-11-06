using System.Collections.Generic;
using System.Linq;
using Beam.Crafting;

namespace Beam.UI;

public class CombinationInfo
{
	public readonly string Name;

	public readonly string Term;

	public readonly string SpriteAssetPath;

	public readonly IList<MaterialInfo> Materials;

	public CombinationInfo(CraftingCombination combination)
	{
		Name = combination.Name;
		Term = combination.Term;
		SpriteAssetPath = combination.SpriteAssetPath;
		Materials = (from m in combination.Materials.ToLookup((CraftingType m) => m)
			select new MaterialInfo(m.Key, m.Count(), 0)).ToList();
	}
}
