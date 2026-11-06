using System.Collections.Generic;
using UnityEngine;

namespace IslandGen;

public class Center
{
	public int Index;

	public Vector2 Point;

	public bool Water;

	public bool Ocean;

	public bool Coast;

	public bool Border;

	public Island.Biome Biome;

	public float Elevation;

	public float Moisture;

	public List<Center> Neighbors;

	public List<Edge> Borders;

	public List<Corner> Corners;
}
