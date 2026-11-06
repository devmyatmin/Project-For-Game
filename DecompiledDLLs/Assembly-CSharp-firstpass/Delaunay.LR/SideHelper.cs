namespace Delaunay.LR;

public class SideHelper
{
	public static Side Other(Side leftRight)
	{
		if (leftRight != Side.Left)
		{
			return Side.Left;
		}
		return Side.Right;
	}
}
