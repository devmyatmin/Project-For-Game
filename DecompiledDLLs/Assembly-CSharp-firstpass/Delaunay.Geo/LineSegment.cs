using System;
using UnityEngine;

namespace Delaunay.Geo;

public sealed class LineSegment
{
	public Nullable p0;

	public Nullable p1;

	public static int CompareLengthsMax(LineSegment segment0, LineSegment segment1)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Distance(segment0.p0.Value, segment0.p1.Value);
		float num2 = Vector2.Distance(segment1.p0.Value, segment1.p1.Value);
		if (num < num2)
		{
			return 1;
		}
		if (num > num2)
		{
			return -1;
		}
		return 0;
	}

	public static int CompareLengths(LineSegment edge0, LineSegment edge1)
	{
		return -CompareLengthsMax(edge0, edge1);
	}

	public LineSegment(Nullable p0, Nullable p1)
	{
		this.p0 = p0;
		this.p1 = p1;
	}
}
