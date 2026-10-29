using System.Collections.Generic;

namespace Beam;

public interface ISlotStorage<TStoredType> : IMultiplayerStorage<TStoredType>, IStorage<TStoredType>, IMultiplayerStorageReference
{
	int SlotCount { get; }

	IEnumerable<StorageSlot<TStoredType>> GetSlots(IComparer<StorageSlot<TStoredType>> comparer = null);
}
