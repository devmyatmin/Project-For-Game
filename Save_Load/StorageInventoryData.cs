using Beam.Language;

namespace Beam.UI;

public class StorageInventoryData<TData> : InventoryData
{
	private readonly ISlotStorage<TData> _storage;

	public StorageInventoryData(ISlotStorage<TData> storage)
		: base(null)
	{
		_storage = storage;
	}

	public override string GetDisplayName()
	{
		return _storage.Name;
	}

	public override string GetDescription()
	{
		return Localization.GetLanguageHandler().Localize("INVENTORY_DATA_STORAGE_DESC", _storage.Name);
	}

	public override string GetClassification()
	{
		return "INVENTORY_DATA_STORAGE_CLASSIFICATION";
	}
}
