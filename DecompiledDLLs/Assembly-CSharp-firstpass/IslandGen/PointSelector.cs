using System.Collections.Generic;
using Delaunay;
using UnityEngine;

namespace IslandGen;

public class PointSelector
{
	public static int NumLloydRelaxations = 2;

	public static List<Vector2> GenerateRandom(int size, int numPoints)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		List<Vector2> list = new List<Vector2>(numPoints - 1);
		for (int i = 0; i < numPoints; i++)
		{
			list.Add(new Vector2(Random.Range(10f, (float)(size - 10)), Random.Range(10f, (float)(size - 10))));
		}
		list.TrimExcess();
		return list;
	}

	public static List<Vector2> GenerateRelaxed(int size, int numPoints)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		List<Vector2> list = GenerateRandom(size, numPoints);
		List<Vector2> list2 = null;
		for (int i = 0; i < NumLloydRelaxations; i++)
		{
			Voronoi voronoi = new Voronoi(list, null, new Rect(0f, 0f, (float)size, (float)size));
			for (int j = 0; j < list.Count; j++)
			{
				Vector2 p = list[j];
				list2 = voronoi.Region(p);
				p.x = 0f;
				p.y = 0f;
				foreach (Vector2 item in list2)
				{
					p.x += item.x;
					p.y += item.y;
				}
				p.x /= list2.Count;
				p.y /= list2.Count;
				list2.Clear();
			}
			voronoi.Dispose();
		}
		return list;
	}
}
