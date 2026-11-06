using System;
using System.Collections.Generic;

public class ObservableSortedList<TKey, TValue>
{
	private readonly IDictionary<TKey, TValue> _collection;

	public int Count => _collection.Count;

	public event Action CollectionChanged;

	protected virtual void OnCollectionChanged()
	{
		CollectionChanged?.Invoke();
	}

	public ObservableSortedList(IDictionary<TKey, TValue> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		_collection = collection;
	}

	public void AddOrIgnore(TKey key, TValue value = default(TValue))
	{
		if (!_collection.ContainsKey(key))
		{
			_collection.Add(key, value);
			OnCollectionChanged();
		}
	}

	public void Remove(TKey key)
	{
		if (_collection.Remove(key))
		{
			OnCollectionChanged();
		}
	}

	public void Clear()
	{
		_collection.Clear();
		OnCollectionChanged();
	}

	public override string ToString()
	{
		return string.Join(" ", _collection.Keys);
	}
}
