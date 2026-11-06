namespace Beam.UI;

public class EmptyHandInventoryData : InventoryData
{
	public EmptyHandInventoryData()
		: base(null)
	{
	}

	public override string GetDisplayName()
	{
		return "INVENTORY_DATA_EMPTY_HANDS_DISPLAY_NAME";
	}

	public override string GetDescription()
	{
		return "INVENTORY_DATA_EMPTY_HANDS_DESC";
	}
}
