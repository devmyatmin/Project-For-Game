using UnityEngine;

namespace AmplifyImpostors;

public static class BoundsEx
{
	public static Bounds Transform(this Bounds bounds, Matrix4x4 matrix)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
		Vector3 extents = bounds.extents;
		Vector3 val = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
		Vector3 val2 = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
		Vector3 val3 = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
		extents.x = Mathf.Abs(val.x) + Mathf.Abs(val2.x) + Mathf.Abs(val3.x);
		extents.y = Mathf.Abs(val.y) + Mathf.Abs(val2.y) + Mathf.Abs(val3.y);
		extents.z = Mathf.Abs(val.z) + Mathf.Abs(val2.z) + Mathf.Abs(val3.z);
		Bounds result = default;
		result.center = center;
		result.extents = extents;
		return result;
	}
}
