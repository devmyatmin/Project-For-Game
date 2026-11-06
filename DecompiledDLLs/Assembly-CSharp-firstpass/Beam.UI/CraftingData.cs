using Beam.Crafting;
using UnityEngine;

namespace Beam.UI;

public class CraftingData : Data<CraftingCombination>
{
	public override string GetDisplayName()
	{
		return Value.Term;
	}

	public override string GetDescription()
	{
		return Value.Description;
	}

	public Sprite GetIcon()
	{
		return Icons.GetSmallIcon(new IconKey
		{
			CraftingType = Value.CraftingType
		});
	}
}
