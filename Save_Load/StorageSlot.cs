using System;
using System.Collections.Generic;
using Beam.Crafting;

namespace Beam;

public class StorageSlot<TStoredType> : IComparable<StorageSlot<TStoredType>>
{
	public class QuantityComparer : IComparer<StorageSlot<TStoredType>>
	{
		public int Compare(StorageSlot<TStoredType> a, StorageSlot<TStoredType> b)
		{
			if (b == null || b.Objects == null)
			{
				return 1;
			}
			if (a.Objects.Count > b.Objects.Count)
			{
				return 1;
			}
			if (a.Objects.Count < b.Objects.Count)
			{
				return -1;
			}
			return 0;
		}
	}

	private readonly int _index;

	private List<TStoredType> _objects = new List<TStoredType>();

	public CraftingType CraftingType { get; set; } = CraftingType.Empty;


	public IList<TStoredType> Objects => _objects;

	public static QuantityComparer QuantityComparison { get; } = new QuantityComparer();


	public StorageSlot(int index)
	{
		_index = index;
	}

	public int CompareTo(StorageSlot<TStoredType> other)
	{
		if (other == null)
		{
			return 1;
		}
		if (_index > other._index)
		{
			return 1;
		}
		if (_index < other._index)
		{
			return -1;
		}
		return 0;
	}
}
