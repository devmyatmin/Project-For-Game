using System.Collections.Generic;
using UnityEngine;

namespace IslandGen;

public class Corner
{
	public int Index;

	public Vector2 Point;

	public bool Ocean;

	public bool Water;

	public bool Coast;

	public bool Border;

	public float Elevation;

	public float Moisture;

	public List<Center> Touches;

	public List<Edge> Protrudes;

	public List<Corner> Adjacent;

	public int River;

	public Corner Downslope;

	public Corner Watershed;

	public int WatershedSize;
}
