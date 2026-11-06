using UnityEngine;

namespace Delaunay.Geo;

public sealed class Circle
{
	public Vector2 center;

	public float radius;

	public Circle(float centerX, float centerY, float radius)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		center = new Vector2(centerX, centerY);
		this.radius = radius;
	}

	public override string ToString()
	{
		return "Circle (center: " + ((object)center/*cast due to constrained. prefix*/).ToString() + "; radius: " + radius + ")";
	}
}
