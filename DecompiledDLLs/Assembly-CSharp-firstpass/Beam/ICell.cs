using System;
using UnityEngine;

namespace Beam;

public interface ICell
{
	int Id { get; }

	Vector3 Position { get; }

	Vector3 Size { get; }

	event Action<int> LodChanged;
}
