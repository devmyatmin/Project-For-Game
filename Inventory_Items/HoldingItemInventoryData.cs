using Beam.Utilities;

namespace Beam.UI;

public class HoldingItemInventoryData : InventoryData
{
	public HoldingItemInventoryData(IPickupable data)
		: base(data)
	{
	}

	public override string GetDisplayName()
	{
		if (!Value.IsNullOrDestroyed())
		{
			return base.GetDisplayName();
		}
		return "INVENTORY_DATA_HOLDING_DISPLAY_NAME";
	}

	public override string GetDescription()
	{
		if (!Value.IsNullOrDestroyed())
		{
			return base.GetDescription();
		}
		return "INVENTORY_DATA_HOLDING_DESC";
	}
}
