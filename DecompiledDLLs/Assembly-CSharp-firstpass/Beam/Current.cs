using UnityEngine;

namespace Beam;

public static class Current
{
	public static float Knots => 1f;

	public static float KmH => Knots * 1.852f;

	public static float Angle => Wind.Angle;

	public static Vector3 Direction
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return Wind.Direction;
		}
	}

	public static Vector3 GetVelocity()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Direction * (KmH / 3.6f * Time.deltaTime);
	}
}
