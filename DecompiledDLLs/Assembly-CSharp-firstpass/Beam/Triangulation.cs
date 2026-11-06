using UnityEngine;

namespace Beam;

public static class Triangulation
{
	private static Vector3 GetBarycentricCoordsAtPoint(Vector3 point, Triangle triangle)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = triangle.P1 - triangle.P0;
		Vector3 val2 = triangle.P2 - triangle.P0;
		Vector3 val3 = point - triangle.P0;
		float num = Vector3.Dot(val, val);
		float num2 = Vector3.Dot(val, val2);
		float num3 = Vector3.Dot(val2, val2);
		float num4 = Vector3.Dot(val3, val);
		float num5 = Vector3.Dot(val3, val2);
		float num6 = num * num3 - num2 * num2;
		float num7 = (num3 * num4 - num2 * num5) / num6;
		float num8 = (num * num5 - num2 * num4) / num6;
		return new Vector3(1f - num7 - num8, num7, num8);
	}

	public static float GetHeightAtPoint(Vector3 point, Triangle triangle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector3 barycentricCoordsAtPoint = GetBarycentricCoordsAtPoint(point, triangle);
		return barycentricCoordsAtPoint.x * triangle.P0.y + barycentricCoordsAtPoint.y * triangle.P1.y + barycentricCoordsAtPoint.z * triangle.P2.y;
	}
}
