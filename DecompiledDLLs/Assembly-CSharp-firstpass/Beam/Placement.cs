using UnityEngine;

namespace Beam;

public struct Placement(Vector3 position, Quaternion rotation)
{
	public Vector3 Position = position;

	public Quaternion Rotation = rotation;
}
