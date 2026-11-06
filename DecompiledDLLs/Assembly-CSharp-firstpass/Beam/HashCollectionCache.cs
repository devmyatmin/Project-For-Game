using System;
using System.Collections.Generic;

namespace Beam;

public static class HashCollectionCache
{
	public static Dictionary<Type, IList<IHashCollection>> _hashCollections = new Dictionary<Type, IList<IHashCollection>>();

	public static void Unload()
	{
		_hashCollections.Clear();
		_hashCollections = null;
	}

	public static void Add(IHashCollection instance)
	{
		Type type = instance.GetType();
		if (!_hashCollections.TryGetValue(type, out var value))
		{
			value = new List<IHashCollection>();
			_hashCollections.Add(type, value);
		}
		value.Add(instance);
	}

	public static void Remove(IHashCollection instance)
	{
		Type type = instance.GetType();
		if (_hashCollections.TryGetValue(type, out var value))
		{
			value.Remove(instance);
			if (_hashCollections[type].Count == 0)
			{
				_hashCollections.Remove(type);
			}
		}
	}

	public static bool HasHash(IHashCollection instance, int hash, bool includeSelf = false)
	{
		Type type = instance.GetType();
		if (_hashCollections.TryGetValue(type, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				IHashCollection hashCollection = value[i];
				if ((includeSelf || hashCollection != instance) && hashCollection.HasHash(hash))
				{
					return true;
				}
			}
		}
		return false;
	}
}
