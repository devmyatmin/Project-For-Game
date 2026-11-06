using System;
using UnityEngine;

namespace Beam;

[Serializable]
public struct Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
{
	public Vector3 P0 = p0;

	public Vector3 P1 = p1;

	public Vector3 P2 = p2;
}
