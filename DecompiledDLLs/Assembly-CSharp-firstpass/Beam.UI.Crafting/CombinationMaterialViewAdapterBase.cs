using Beam.Crafting;

namespace Beam.UI.Crafting;

public abstract class CombinationMaterialViewAdapterBase : ViewAdapterBase
{
	public abstract EMaterialQtyDisplayMode MaterialQtyDisplayMode { set; }

	public abstract void SetIcon(CraftingType type);

	public abstract void SetRequired(int value);

	public abstract void SetAvailable(int value);
}
