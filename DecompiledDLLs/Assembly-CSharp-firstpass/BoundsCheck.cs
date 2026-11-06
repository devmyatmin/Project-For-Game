using UnityEngine;

internal static class BoundsCheck
{
	public static readonly int TOP = 1;

	public static readonly int BOTTOM = 2;

	public static readonly int LEFT = 4;

	public static readonly int RIGHT = 8;

	public static int Check(Vector2 point, Rect bounds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if (point.x == bounds.xMin)
		{
			num |= LEFT;
		}
		if (point.x == bounds.xMax)
		{
			num |= RIGHT;
		}
		if (point.y == bounds.yMin)
		{
			num |= TOP;
		}
		if (point.y == bounds.yMax)
		{
			num |= BOTTOM;
		}
		return num;
	}
}
