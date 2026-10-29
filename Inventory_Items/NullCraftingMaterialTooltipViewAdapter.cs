using UnityEngine;

namespace Beam.UI;

public class NullCraftingMaterialTooltipViewAdapter : CraftingMaterialTooltipViewAdapterBase
{
	private static NullCraftingMaterialTooltipViewAdapter _Instance = new NullCraftingMaterialTooltipViewAdapter();

	public static NullCraftingMaterialTooltipViewAdapter Instance => _Instance;

	public override void SetPosition(Vector2 screenPosition)
	{
	}

	public override void SetDescription(string text)
	{
	}
}
