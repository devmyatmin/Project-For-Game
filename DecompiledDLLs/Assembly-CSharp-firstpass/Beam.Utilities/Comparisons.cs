using UnityEngine;

namespace Beam.Utilities;

public static class Comparisons
{
	public static int FurthestFromPoint(Vector3 point, Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (a == Vector3.zero && b == Vector3.zero)
		{
			return 0;
		}
		Vector3 val = point - a;
		float sqrMagnitude = val.sqrMagnitude;
		val = point - b;
		float sqrMagnitude2 = val.sqrMagnitude;
		if (sqrMagnitude > sqrMagnitude2)
		{
			return -1;
		}
		if (sqrMagnitude2 > sqrMagnitude)
		{
			return 1;
		}
		return 0;
	}
}
