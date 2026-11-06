using System;
using UnityEngine;

namespace Beam;

[Serializable]
public class VehicleSeat
{
	[NonSerialized]
	public int PlayerId = -1;

	public Vector3 Position;

	public Quaternion Rotation;
}
