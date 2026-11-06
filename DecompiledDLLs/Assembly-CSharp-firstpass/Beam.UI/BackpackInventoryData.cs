namespace Beam.UI;

public class BackpackInventoryData : InventoryData
{
	public BackpackInventoryData()
		: base(null)
	{
	}

	public override string GetDisplayName()
	{
		return "INVENTORY_DATA_BACKPACK_DISPLAY_NAME";
	}

	public override string GetDescription()
	{
		return "INVENTORY_DATA_BACKPACK_DESC";
	}

	public override string GetClassification()
	{
		return "INVENTORY_DATA_BACKPACK_CLASSIFICATION";
	}
}
