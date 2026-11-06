using System;
using UnityEngine;

namespace Beam;

public interface ICellObject : ISaveable
{
	int PrefabId { get; }

	event Action<ICellObject> Freed;

	event Action<ICellObject> Destroyed;

	void Subscribe(ICell cell);

	void Unsubscribe(ICell cell);

	void Destroy();

	void Free();

	Bounds GetBounds();
}
