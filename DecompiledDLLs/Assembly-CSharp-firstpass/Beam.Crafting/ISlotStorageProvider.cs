namespace Beam.Crafting;

public interface ISlotStorageProvider<TStoredType>
{
	ISlotStorage<TStoredType> GetSlotStorage();
}
