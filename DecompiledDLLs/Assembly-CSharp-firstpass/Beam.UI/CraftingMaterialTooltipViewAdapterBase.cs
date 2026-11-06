using UnityEngine;

namespace Beam.UI;

public abstract class CraftingMaterialTooltipViewAdapterBase : ViewAdapterBase
{
	public abstract void SetPosition(Vector2 screenPosition);

	public abstract void SetDescription(string text);
}
