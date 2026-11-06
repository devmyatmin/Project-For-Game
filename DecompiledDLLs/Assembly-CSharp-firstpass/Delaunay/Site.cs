using System;
using System.Collections.Generic;
using Delaunay.Geo;
using Delaunay.LR;
using UnityEngine;

namespace Delaunay;

public sealed class Site : ICoord, IComparable
{
	private static Stack<Site> _pool = new Stack<Site>();

	private static readonly float EPSILON = 0.005f;

	private Vector2 _coord;

	public uint color;

	public float weight;

	private uint _siteIndex;

	private List<Edge> _edges;

	private List<Side> _edgeOrientations;

	private List<Vector2> _region;

	public Vector2 Coord
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _coord;
		}
	}

	internal List<Edge> edges => _edges;

	public float x => _coord.x;

	internal float y => _coord.y;

	public static Site Create(Vector2 p, uint index, float weight, uint color)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (_pool.Count > 0)
		{
			return _pool.Pop().Init(p, index, weight, color);
		}
		return new Site(p, index, weight, color);
	}

	internal static void SortSites(List<Site> sites)
	{
		sites.Sort();
	}

	public int CompareTo(object obj)
	{
		Site site = (Site)obj;
		int num = Voronoi.CompareByYThenX(this, site);
		switch (num)
		{
		case -1:
			if (_siteIndex > site._siteIndex)
			{
				uint siteIndex = _siteIndex;
				_siteIndex = site._siteIndex;
				site._siteIndex = siteIndex;
			}
			break;
		case 1:
			if (site._siteIndex > _siteIndex)
			{
				uint siteIndex = site._siteIndex;
				site._siteIndex = _siteIndex;
				_siteIndex = siteIndex;
			}
			break;
		}
		return num;
	}

	private static bool CloseEnough(Vector2 p0, Vector2 p1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Distance(p0, p1) < EPSILON;
	}

	private Site(Vector2 p, uint index, float weight, uint color)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Init(p, index, weight, color);
	}

	private Site Init(Vector2 p, uint index, float weight, uint color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_coord = p;
		_siteIndex = index;
		this.weight = weight;
		this.color = color;
		_edges = new List<Edge>();
		_region = null;
		return this;
	}

	public override string ToString()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return "Site " + _siteIndex + ": " + ((object)Coord/*cast due to constrained. prefix*/).ToString();
	}

	private void Move(Vector2 p)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Clear();
		_coord = p;
	}

	public void Dispose()
	{
		Clear();
		_pool.Push(this);
	}

	private void Clear()
	{
		if (_edges != null)
		{
			_edges.Clear();
			_edges = null;
		}
		if (_edgeOrientations != null)
		{
			_edgeOrientations.Clear();
			_edgeOrientations = null;
		}
		if (_region != null)
		{
			_region.Clear();
			_region = null;
		}
	}

	public void AddEdge(Edge edge)
	{
		_edges.Add(edge);
	}

	public Edge NearestEdge()
	{
		_edges.Sort((Edge a, Edge b) => Edge.CompareSitesDistances(a, b));
		return _edges[0];
	}

	public List<Site> NeighborSites()
	{
		if (_edges == null || _edges.Count == 0)
		{
			return new List<Site>();
		}
		if (_edgeOrientations == null)
		{
			ReorderEdges();
		}
		List<Site> list = new List<Site>();
		for (int i = 0; i < _edges.Count; i++)
		{
			Edge edge = _edges[i];
			list.Add(NeighborSite(edge));
		}
		return list;
	}

	private Site NeighborSite(Edge edge)
	{
		if (this == edge.leftSite)
		{
			return edge.rightSite;
		}
		if (this == edge.rightSite)
		{
			return edge.leftSite;
		}
		return null;
	}

	internal List<Vector2> Region(Rect clippingBounds)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (_edges == null || _edges.Count == 0)
		{
			return new List<Vector2>();
		}
		if (_edgeOrientations == null)
		{
			ReorderEdges();
			_region = ClipToBounds(clippingBounds);
			if (new Polygon(_region).Winding() == Winding.Clockwise)
			{
				_region.Reverse();
			}
		}
		return _region;
	}

	private void ReorderEdges()
	{
		EdgeReorderer edgeReorderer = new EdgeReorderer(_edges, VertexOrSite.Vertex);
		_edges = edgeReorderer.edges;
		_edgeOrientations = edgeReorderer.edgeOrientations;
		edgeReorderer.Dispose();
	}

	private List<Vector2> ClipToBounds(Rect bounds)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		List<Vector2> list = new List<Vector2>();
		int count = _edges.Count;
		int i;
		for (i = 0; i < count && !_edges[i].visible; i++)
		{
		}
		if (i == count)
		{
			return new List<Vector2>();
		}
		Edge edge = _edges[i];
		Side side = _edgeOrientations[i];
		if (!edge.clippedEnds[side].HasValue)
		{
			Debug.LogError((object)"XXX: Null detected when there should be a Vector2!");
		}
		if (!edge.clippedEnds[SideHelper.Other(side)].HasValue)
		{
			Debug.LogError((object)"XXX: Null detected when there should be a Vector2!");
		}
		list.Add(edge.clippedEnds[side].Value);
		list.Add(edge.clippedEnds[SideHelper.Other(side)].Value);
		for (int j = i + 1; j < count; j++)
		{
			edge = _edges[j];
			if (edge.visible)
			{
				Connect(list, j, bounds);
			}
		}
		Connect(list, i, bounds, closingUp: true);
		return list;
	}

	private void Connect(List<Vector2> points, int j, Rect bounds, bool closingUp = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = points[points.Count - 1];
		Edge edge = _edges[j];
		Side side = _edgeOrientations[j];
		if (!edge.clippedEnds[side].HasValue)
		{
			Debug.LogError((object)"XXX: Null detected when there should be a Vector2!");
		}
		Vector2 value = edge.clippedEnds[side].Value;
		if (!CloseEnough(val, value))
		{
			if (val.x != value.x && val.y != value.y)
			{
				int num = BoundsCheck.Check(val, bounds);
				int num2 = BoundsCheck.Check(value, bounds);
				if ((num & BoundsCheck.RIGHT) != 0)
				{
					float xMax = bounds.xMax;
					if ((num2 & BoundsCheck.BOTTOM) != 0)
					{
						float yMax = bounds.yMax;
						points.Add(new Vector2(xMax, yMax));
					}
					else if ((num2 & BoundsCheck.TOP) != 0)
					{
						float yMax = bounds.yMin;
						points.Add(new Vector2(xMax, yMax));
					}
					else if ((num2 & BoundsCheck.LEFT) != 0)
					{
						float yMax = ((!(val.y - bounds.y + value.y - bounds.y < bounds.height)) ? bounds.yMax : bounds.yMin);
						points.Add(new Vector2(xMax, yMax));
						points.Add(new Vector2(bounds.xMin, yMax));
					}
				}
				else if ((num & BoundsCheck.LEFT) != 0)
				{
					float xMax = bounds.xMin;
					if ((num2 & BoundsCheck.BOTTOM) != 0)
					{
						float yMax = bounds.yMax;
						points.Add(new Vector2(xMax, yMax));
					}
					else if ((num2 & BoundsCheck.TOP) != 0)
					{
						float yMax = bounds.yMin;
						points.Add(new Vector2(xMax, yMax));
					}
					else if ((num2 & BoundsCheck.RIGHT) != 0)
					{
						float yMax = ((!(val.y - bounds.y + value.y - bounds.y < bounds.height)) ? bounds.yMax : bounds.yMin);
						points.Add(new Vector2(xMax, yMax));
						points.Add(new Vector2(bounds.xMax, yMax));
					}
				}
				else if ((num & BoundsCheck.TOP) != 0)
				{
					float yMax = bounds.yMin;
					if ((num2 & BoundsCheck.RIGHT) != 0)
					{
						float xMax = bounds.xMax;
						points.Add(new Vector2(xMax, yMax));
					}
					else if ((num2 & BoundsCheck.LEFT) != 0)
					{
						float xMax = bounds.xMin;
						points.Add(new Vector2(xMax, yMax));
					}
					else if ((num2 & BoundsCheck.BOTTOM) != 0)
					{
						float xMax = ((!(val.x - bounds.x + value.x - bounds.x < bounds.width)) ? bounds.xMax : bounds.xMin);
						points.Add(new Vector2(xMax, yMax));
						points.Add(new Vector2(xMax, bounds.yMax));
					}
				}
				else if ((num & BoundsCheck.BOTTOM) != 0)
				{
					float yMax = bounds.yMax;
					if ((num2 & BoundsCheck.RIGHT) != 0)
					{
						float xMax = bounds.xMax;
						points.Add(new Vector2(xMax, yMax));
					}
					else if ((num2 & BoundsCheck.LEFT) != 0)
					{
						float xMax = bounds.xMin;
						points.Add(new Vector2(xMax, yMax));
					}
					else if ((num2 & BoundsCheck.TOP) != 0)
					{
						float xMax = ((!(val.x - bounds.x + value.x - bounds.x < bounds.width)) ? bounds.xMax : bounds.xMin);
						points.Add(new Vector2(xMax, yMax));
						points.Add(new Vector2(xMax, bounds.yMin));
					}
				}
			}
			if (closingUp)
			{
				return;
			}
			points.Add(value);
		}
		if (!edge.clippedEnds[SideHelper.Other(side)].HasValue)
		{
			Debug.LogError((object)"XXX: Null detected when there should be a Vector2!");
		}
		Vector2 value2 = edge.clippedEnds[SideHelper.Other(side)].Value;
		if (!CloseEnough(points[0], value2))
		{
			points.Add(value2);
		}
	}

	public float Dist(ICoord p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Distance(p.Coord, _coord);
	}
}
