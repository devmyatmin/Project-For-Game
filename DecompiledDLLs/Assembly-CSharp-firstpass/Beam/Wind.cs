using System;
using UnityEngine;

namespace Beam;

public static class Wind
{
	private static float _angle = 305f;

	public static float KmH => 9f;

	public static float Angle
	{
		get
		{
			return _angle;
		}
		set
		{
			_angle = value;
			if (_angle > 360f)
			{
				_angle = 0f;
			}
			if (_angle < 0f)
			{
				_angle = 360f;
			}
		}
	}

	public static Vector3 Direction
	{
		get
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			float num = Angle * (float)Math.PI / 180f;
			float num2 = 0f - Mathf.Cos(num);
			float num3 = Mathf.Sin(num);
			Vector3 val = new Vector3(num2, 0f, num3);
			return val.normalized;
		}
	}

	public static Vector3 GetVelocity()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Direction * (KmH / 3.6f * Time.deltaTime);
	}
}
