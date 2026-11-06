using UnityEngine;

namespace RootMotion;

public class AxisTools
{
	public static Vector3 ToVector3(Axis axis)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return axis switch
		{
			Axis.X => Vector3.right, 
			Axis.Y => Vector3.up, 
			_ => Vector3.forward, 
		};
	}

	public static Axis ToAxis(Vector3 v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Abs(v.x);
		float num2 = Mathf.Abs(v.y);
		float num3 = Mathf.Abs(v.z);
		Axis result = Axis.X;
		if (num2 > num && num2 > num3)
		{
			result = Axis.Y;
		}
		if (num3 > num && num3 > num2)
		{
			result = Axis.Z;
		}
		return result;
	}

	public static Axis GetAxisToPoint(Transform t, Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 axisVectorToPoint = GetAxisVectorToPoint(t, worldPosition);
		if (axisVectorToPoint == Vector3.right)
		{
			return Axis.X;
		}
		if (axisVectorToPoint == Vector3.up)
		{
			return Axis.Y;
		}
		return Axis.Z;
	}

	public static Axis GetAxisToDirection(Transform t, Vector3 direction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 axisVectorToDirection = GetAxisVectorToDirection(t, direction);
		if (axisVectorToDirection == Vector3.right)
		{
			return Axis.X;
		}
		if (axisVectorToDirection == Vector3.up)
		{
			return Axis.Y;
		}
		return Axis.Z;
	}

	public static Vector3 GetAxisVectorToPoint(Transform t, Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return GetAxisVectorToDirection(t, worldPosition - t.position);
	}

	public static Vector3 GetAxisVectorToDirection(Transform t, Vector3 direction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return GetAxisVectorToDirection(t.rotation, direction);
	}

	public static Vector3 GetAxisVectorToDirection(Quaternion r, Vector3 direction)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		direction = direction.normalized;
		Vector3 result = Vector3.right;
		float num = Mathf.Abs(Vector3.Dot(r * Vector3.right, direction));
		float num2 = Mathf.Abs(Vector3.Dot(r * Vector3.up, direction));
		if (num2 > num)
		{
			result = Vector3.up;
		}
		float num3 = Mathf.Abs(Vector3.Dot(r * Vector3.forward, direction));
		if (num3 > num && num3 > num2)
		{
			result = Vector3.forward;
		}
		return result;
	}
}
