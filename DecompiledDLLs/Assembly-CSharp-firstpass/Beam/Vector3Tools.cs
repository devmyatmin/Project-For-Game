using System;
using UnityEngine;

namespace Beam;

public static class Vector3Tools
{
	public static Vector3 xy
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(1f, 1f, 0f);
		}
	}

	public static Vector3 xz
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(1f, 0f, 1f);
		}
	}

	public static Vector3 yz
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(0f, 1f, 1f);
		}
	}

	public static Vector3 Mask(this Vector3 a, Vector3 axis)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(a.x * axis.x, a.y * axis.y, a.z * axis.z);
	}

	public static float DistanceOnAxis(Vector3 a, Vector3 b, Vector3 axis)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Mask(a - b, axis);
		return val.magnitude;
	}

	public static float SqrDistanceOnAxis(Vector3 a, Vector3 b, Vector3 axis)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Mask(a - b, axis);
		return val.sqrMagnitude;
	}

	public static double Truncate(float value)
	{
		return Math.Truncate(value * 1000f) / 1000.0;
	}

	public static Vector3 Truncate(this Vector3 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Truncate(value.x);
		float num2 = (float)Truncate(value.y);
		float num3 = (float)Truncate(value.z);
		return new Vector3(num, num2, num3);
	}

	public static int Hash(this Vector3 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Truncate(value.x);
		float num2 = (float)Truncate(value.y);
		float num3 = (float)Truncate(value.z);
		return num.GetHashCode() ^ (num2.GetHashCode() << 2) ^ (num3.GetHashCode() >> 2);
	}
}
