using System.Collections.Generic;
using UnityEngine;

namespace Delaunay.Geo;

public sealed class Polygon
{
	private List<Vector2> _vertices;

	public Polygon(List<Vector2> vertices)
	{
		_vertices = vertices;
	}

	public float Area()
	{
		return Mathf.Abs(SignedDoubleArea() * 0.5f);
	}

	public Winding Winding()
	{
		float num = SignedDoubleArea();
		if (num < 0f)
		{
			return Delaunay.Geo.Winding.Clockwise;
		}
		if (num > 0f)
		{
			return Delaunay.Geo.Winding.CounterClockwise;
		}
		return Delaunay.Geo.Winding.None;
	}

	private float SignedDoubleArea()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		int count = _vertices.Count;
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			int index = (i + 1) % count;
			Vector2 val = _vertices[i];
			Vector2 val2 = _vertices[index];
			num += val.x * val2.y - val2.x * val.y;
		}
		return num;
	}
}
