using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Delaunay;
using Delaunay.Geo;
using UnityEngine;

namespace IslandGen;

public class Island
{
	public enum IslandTypes
	{
		Radial,
		Perlin,
		Blob
	}

	public enum PointTypes
	{
		Random,
		Relaxed
	}

	public static class BiomeProperties
	{
		public static Dictionary<Biome, Color> Colors = new Dictionary<Biome, Color>
		{
			{
				Biome.Ocean,
				HexToColor("44447a")
			},
			{
				Biome.Lake,
				HexToColor("336699")
			},
			{
				Biome.Beach,
				HexToColor("605647")
			},
			{
				Biome.Land,
				HexToColor("a09077")
			}
		};

		private static Color HexToColor(string hex)
		{
			byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
			return new Color32(r, g, b, byte.MaxValue);
		}
	}

	public enum Biome
	{
		Ocean,
		Lake,
		Beach,
		Land
	}

	public static float LakeThreshold = 0.3f;

	public List<Vector2> Points { get; private set; }

	public List<Center> Centers { get; private set; }

	public List<Corner> Corners { get; private set; }

	public List<Edge> Edges { get; private set; }

	public float Size { get; private set; }

	public int NumPoints { get; private set; }

	public IslandTypes IslandType { get; private set; }

	public PointTypes PointType { get; private set; }

	public Island(float size)
	{
		size = Size;
		NumPoints = 1;
		Points = new List<Vector2>();
		Centers = new List<Center>();
		Corners = new List<Corner>();
		Edges = new List<Edge>();
		Reset();
	}

	private void Reset()
	{
		Points.Clear();
		Points.Capacity = 0;
		if (Edges != null)
		{
			foreach (Edge edge in Edges)
			{
				edge.D0 = null;
				edge.D1 = null;
				edge.V0 = null;
				edge.V1 = null;
			}
			Edges.Clear();
			Edges.TrimExcess();
		}
		if (Centers != null)
		{
			foreach (Center center in Centers)
			{
				center.Neighbors.Clear();
				center.Corners.Clear();
				center.Borders.Clear();
				center.Neighbors.TrimExcess();
				center.Corners.TrimExcess();
				center.Borders.TrimExcess();
			}
			Centers.Clear();
			Centers.TrimExcess();
		}
		if (Corners != null)
		{
			foreach (Corner corner in Corners)
			{
				corner.Adjacent.Clear();
				corner.Touches.Clear();
				corner.Protrudes.Clear();
				corner.Downslope = null;
				corner.Watershed = null;
				corner.Adjacent.TrimExcess();
				corner.Touches.TrimExcess();
				corner.Protrudes.TrimExcess();
			}
			Corners.Clear();
			Corners.TrimExcess();
		}
		if (Points == null)
		{
			Points = new List<Vector2>();
		}
		if (Edges == null)
		{
			Edges = new List<Edge>();
		}
		if (Centers == null)
		{
			Centers = new List<Center>();
		}
		if (Corners == null)
		{
			Corners = new List<Corner>();
		}
	}

	public void NewIsland(IslandTypes islandType, PointTypes pointType, int numPoints, int seed, float size)
	{
		Reset();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		Size = size;
		IslandType = islandType;
		PointType = pointType;
		NumPoints = numPoints;
		switch (PointType)
		{
		case PointTypes.Relaxed:
			Points = PointSelector.GenerateRelaxed((int)Size, numPoints);
			break;
		case PointTypes.Random:
			Points = PointSelector.GenerateRandom((int)Size, numPoints);
			break;
		}
		Voronoi voronoi = new Voronoi(Points, null, new Rect(0f, 0f, Size, Size));
		BuildGraph(Points, voronoi);
		ImproveCorners();
		voronoi.Dispose();
		voronoi = null;
		Points = null;
		AssignCornerElevations();
		AssignOceanCoastAndLand();
		RedistributeElevations(LandCorners(Corners));
		for (int i = 0; i < Corners.Count; i++)
		{
			Corner corner = Corners[i];
			if (corner.Ocean || corner.Coast)
			{
				corner.Elevation = 0f;
			}
		}
		AssignPolygonElevations();
		CalculateDownslopes();
		CalculateWatersheds();
		CreateRivers();
		AssignBiomes();
		UnityEngine.Random.state = state;
	}

	private void ImproveCorners()
	{
		List<Vector2> list = new List<Vector2>(Corners.Count);
		while (list.Count < Corners.Count)
		{
			list.Add(default(Vector2));
		}
		foreach (Corner corner in Corners)
		{
			if (corner.Border)
			{
				list[corner.Index] = corner.Point;
				continue;
			}
			Vector2 value = default(Vector2);
			foreach (Center touch in corner.Touches)
			{
				value.x += touch.Point.x;
				value.y += touch.Point.y;
			}
			value.x /= corner.Touches.Count;
			value.y /= corner.Touches.Count;
			list[corner.Index] = value;
		}
		for (int i = 0; i < Corners.Count; i++)
		{
			Corners[i].Point = list[i];
		}
		foreach (Edge edge in Edges)
		{
			if (edge.V0 != null && edge.V1 != null)
			{
				edge.Midpoint = Vector2.Lerp(edge.V0.Point, edge.V1.Point, 0.5f);
			}
		}
	}

	private void BuildGraph(List<Vector2> points, Voronoi voronoi)
	{
		Dictionary<Vector2, Center> dictionary = new Dictionary<Vector2, Center>();
		foreach (Vector2 point in points)
		{
			Center center2 = new Center();
			center2.Index = Centers.Count;
			center2.Point = point;
			center2.Neighbors = new List<Center>();
			center2.Borders = new List<Edge>();
			center2.Corners = new List<Corner>();
			Centers.Add(center2);
			dictionary.Add(point, center2);
		}
		foreach (Center center3 in Centers)
		{
			voronoi.Region(center3.Point);
		}
		List<List<Corner>> cornerIsland = new List<List<Corner>>();
		Func<Vector2?, Corner> func = delegate(Vector2? point)
		{
			int num = 0;
			if (!point.HasValue || !point.HasValue)
			{
				return null;
			}
			Vector2 value = point.Value;
			for (num = (int)value.x - 1; num <= (int)value.x + 1; num++)
			{
				if (num >= 0 && num < cornerIsland.Count)
				{
					foreach (Corner item in cornerIsland[num])
					{
						float num2 = value.x - item.Point.x;
						float num3 = value.y - item.Point.y;
						if (num2 * num2 + num3 * num3 < 1E-06f)
						{
							return item;
						}
					}
				}
			}
			num = (int)value.x;
			while (cornerIsland.Count <= num)
			{
				cornerIsland.Add(new List<Corner>());
			}
			Corner corner2 = new Corner
			{
				Index = Corners.Count
			};
			Corners.Add(corner2);
			corner2.Point = value;
			corner2.Border = value.x == 0f || value.x == Size || value.y == 0f || value.y == Size;
			corner2.Touches = new List<Center>();
			corner2.Protrudes = new List<Edge>();
			corner2.Adjacent = new List<Corner>();
			cornerIsland[num].Add(corner2);
			return corner2;
		};
		Action<List<Corner>, Corner> action = delegate(List<Corner> cornerList, Corner corner)
		{
			if (corner != null && cornerList.IndexOf(corner) < 0)
			{
				cornerList.Add(corner);
			}
		};
		Action<List<Center>, Center> action2 = delegate(List<Center> centerList, Center center)
		{
			if (center != null && centerList.IndexOf(center) < 0)
			{
				centerList.Add(center);
			}
		};
		foreach (Delaunay.Edge item2 in voronoi.Edges())
		{
			LineSegment lineSegment = item2.DelaunayLine();
			LineSegment lineSegment2 = item2.VoronoiEdge();
			Edge edge = new Edge();
			edge.Index = Edges.Count;
			edge.River = 0;
			Edges.Add(edge);
			if (lineSegment2.p0.HasValue && lineSegment2.p1.HasValue)
			{
				edge.Midpoint = Vector2.Lerp(lineSegment2.p0.Value, lineSegment2.p1.Value, 0.5f);
			}
			edge.V0 = func(lineSegment2.p0);
			edge.V1 = func(lineSegment2.p1);
			edge.D0 = dictionary[lineSegment.p0.Value];
			edge.D1 = dictionary[lineSegment.p1.Value];
			if (edge.D0 != null)
			{
				edge.D0.Borders.Add(edge);
			}
			if (edge.D1 != null)
			{
				edge.D1.Borders.Add(edge);
			}
			if (edge.V0 != null)
			{
				edge.V0.Protrudes.Add(edge);
			}
			if (edge.V1 != null)
			{
				edge.V1.Protrudes.Add(edge);
			}
			if (edge.D0 != null && edge.D1 != null)
			{
				action2(edge.D0.Neighbors, edge.D1);
				action2(edge.D1.Neighbors, edge.D0);
			}
			if (edge.V0 != null && edge.V1 != null)
			{
				action(edge.V0.Adjacent, edge.V1);
				action(edge.V1.Adjacent, edge.V0);
			}
			if (edge.D0 != null)
			{
				action(edge.D0.Corners, edge.V0);
				action(edge.D0.Corners, edge.V1);
			}
			if (edge.D1 != null)
			{
				action(edge.D1.Corners, edge.V0);
				action(edge.D1.Corners, edge.V1);
			}
			if (edge.V0 != null)
			{
				action2(edge.V0.Touches, edge.D0);
				action2(edge.V0.Touches, edge.D1);
			}
			if (edge.V1 != null)
			{
				action2(edge.V1.Touches, edge.D0);
				action2(edge.V1.Touches, edge.D1);
			}
		}
	}

	private List<Corner> LandCorners(List<Corner> corners)
	{
		List<Corner> list = new List<Corner>();
		foreach (Corner corner in Corners)
		{
			if (!corner.Ocean && !corner.Coast)
			{
				list.Add(corner);
			}
		}
		return list;
	}

	private void AssignCornerElevations()
	{
		Queue<Corner> queue = new Queue<Corner>();
		foreach (Corner corner2 in Corners)
		{
			corner2.Water = !Inside(corner2.Point);
		}
		foreach (Corner corner3 in Corners)
		{
			if (corner3.Border)
			{
				corner3.Elevation = 0f;
				queue.Enqueue(corner3);
			}
			else
			{
				corner3.Elevation = float.PositiveInfinity;
			}
		}
		while (queue.Count > 0)
		{
			Corner corner = queue.Dequeue();
			foreach (Corner item in corner.Adjacent)
			{
				float num = 0.01f + corner.Elevation;
				if (!corner.Water && !item.Water)
				{
					num += 1f;
				}
				if (num < item.Elevation)
				{
					item.Elevation = num;
					queue.Enqueue(item);
				}
			}
		}
	}

	private void RedistributeElevations(List<Corner> locations)
	{
		float num = 1.1f;
		locations = locations.OrderBy((Corner o) => o.Elevation).ToList();
		for (int i = 0; i < locations.Count; i++)
		{
			float num2 = (float)i / (float)(locations.Count - 1);
			float num3 = Mathf.Sqrt(num) - Mathf.Sqrt(num * (1f - num2));
			if (num3 > 1f)
			{
				num3 = 1f;
			}
			locations[i].Elevation = num3;
		}
	}

	private void AssignOceanCoastAndLand()
	{
		Queue<Center> queue = new Queue<Center>();
		foreach (Center center in Centers)
		{
			int num = 0;
			foreach (Corner corner in center.Corners)
			{
				if (corner.Border)
				{
					center.Border = true;
					center.Ocean = true;
					corner.Water = true;
					queue.Enqueue(center);
				}
				if (corner.Water)
				{
					num++;
				}
			}
			center.Water = center.Ocean || (float)num >= (float)center.Corners.Count * LakeThreshold;
		}
		while (queue.Count > 0)
		{
			foreach (Center neighbor in queue.Dequeue().Neighbors)
			{
				if (neighbor.Water && !neighbor.Ocean)
				{
					neighbor.Ocean = true;
					queue.Enqueue(neighbor);
				}
			}
		}
		foreach (Center center2 in Centers)
		{
			int num2 = 0;
			int num3 = 0;
			foreach (Center neighbor2 in center2.Neighbors)
			{
				if (neighbor2.Ocean)
				{
					num2++;
				}
				if (!neighbor2.Water)
				{
					num3++;
				}
			}
			center2.Coast = num2 > 0 && num3 > 0;
		}
		foreach (Corner corner2 in Corners)
		{
			int num4 = 0;
			int num5 = 0;
			foreach (Center touch in corner2.Touches)
			{
				if (touch.Ocean)
				{
					num4++;
				}
				if (!touch.Water)
				{
					num5++;
				}
			}
			corner2.Ocean = num4 == corner2.Touches.Count;
			corner2.Coast = num4 > 0 && num5 > 0;
			corner2.Water = corner2.Border || (num5 != corner2.Touches.Count && !corner2.Coast);
		}
	}

	private void AssignPolygonElevations()
	{
		foreach (Center center in Centers)
		{
			float num = 0f;
			foreach (Corner corner in center.Corners)
			{
				num += corner.Elevation;
			}
			center.Elevation = num / (float)center.Corners.Count;
		}
	}

	private void CalculateDownslopes()
	{
		foreach (Corner corner2 in Corners)
		{
			Corner corner = corner2;
			foreach (Corner item in corner2.Adjacent)
			{
				if (item.Elevation <= corner.Elevation)
				{
					corner = item;
				}
			}
			corner2.Downslope = corner;
		}
	}

	private void CalculateWatersheds()
	{
		foreach (Corner corner2 in Corners)
		{
			corner2.Watershed = corner2;
			if (!corner2.Ocean && !corner2.Coast)
			{
				corner2.Watershed = corner2.Downslope;
			}
		}
		Corner corner = null;
		for (int i = 0; i < 100; i++)
		{
			bool flag = false;
			foreach (Corner corner3 in Corners)
			{
				if (!corner3.Ocean && !corner3.Coast && !corner3.Watershed.Coast)
				{
					corner = corner3.Downslope.Watershed;
					if (!corner.Ocean)
					{
						corner3.Watershed = corner;
					}
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		foreach (Corner corner4 in Corners)
		{
			corner = corner4.Watershed;
			corner.WatershedSize = ((corner.WatershedSize == 0) ? 1 : 2);
		}
	}

	private void CreateRivers()
	{
		for (int i = 0; (float)i < Size / 2f; i++)
		{
			Corner corner = Corners[UnityEngine.Random.Range(0, Corners.Count)];
			if (!corner.Ocean && !(corner.Elevation < 0.3f) && !(corner.Elevation > 0.9f))
			{
				while (!corner.Coast && corner != corner.Downslope)
				{
					LookUpEdgeFromCorner(corner, corner.Downslope).River++;
					corner.River = ((corner.River == 0) ? 1 : 2);
					corner = corner.Downslope;
				}
			}
		}
	}

	private static Biome GetBiome(Center center)
	{
		if (center.Ocean)
		{
			return Biome.Ocean;
		}
		if (center.Water)
		{
			return Biome.Lake;
		}
		if (center.Coast)
		{
			return Biome.Beach;
		}
		return Biome.Land;
	}

	private void AssignBiomes()
	{
		foreach (Center center in Centers)
		{
			center.Biome = GetBiome(center);
		}
	}

	public Edge LookUpEdgeFromCorner(Corner main, Corner secondary)
	{
		foreach (Edge protrude in main.Protrudes)
		{
			if (protrude.V0 == secondary || protrude.V1 == secondary)
			{
				return protrude;
			}
		}
		return null;
	}

	public Edge LookUpEdgeFromCenter(Center main, Center secondary)
	{
		foreach (Edge border in main.Borders)
		{
			if (border.D0 == secondary || border.D1 == secondary)
			{
				return border;
			}
		}
		return null;
	}

	private bool Inside(Vector2 position)
	{
		return InsideIsland(new Vector2(2f * (position.x / Size - 0.5f), 2f * (position.y / Size - 0.5f)));
	}

	private bool InsideIsland(Vector2 point)
	{
		bool flag = new Vector2(point.x - 0.2f, point.y / 2f + 0.2f).magnitude < 0.05f;
		bool flag2 = new Vector2(point.x + 0.2f, point.y / 2f + 0.2f).magnitude < 0.05f;
		if (point.magnitude < 0.8f - 0.18f * Mathf.Sin(5f * Mathf.Atan2(point.y, point.x)) && !flag)
		{
			return !flag2;
		}
		return false;
	}
}
