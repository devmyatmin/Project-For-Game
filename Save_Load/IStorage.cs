using System;
using System.Collections.Generic;
using Beam.Crafting;
using Funlabs;

namespace Beam;

public interface IStorage<TStoredType>
{
	string Name { get; }

	bool IsOpened { get; }

	LoadState LoadState { get; }

	event Action<TStoredType> SelectionChanged;

	event Action<TStoredType> Popped;

	event Action<TStoredType, bool> Pushed;

	event Action Opened;

	event Action Closed;

	IEnumerable<TStoredType> GetStored();

	void ReserveFor(int playerId);

	void Open(int playerId);

	void Close();

	bool CanPush(TStoredType obj, bool notification = true);

	bool Push(TStoredType obj);

	void Pop(TStoredType obj);

	void PopNext(TStoredType obj);

	bool Has(TStoredType obj);

	void Select(TStoredType obj);

	T Get<T>() where T : class, TStoredType;

	T Get<T>(CraftingType craftingType) where T : class, TStoredType;

	T Get<T>(MiniGuid referenceId) where T : class, TStoredType;
}
