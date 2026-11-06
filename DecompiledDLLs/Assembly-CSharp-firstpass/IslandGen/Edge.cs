using UnityEngine;

namespace IslandGen;

public class Edge
{
	public int Index;

	public Center D0;

	public Center D1;

	public Corner V0;

	public Corner V1;

	public Vector2 Midpoint;

	public int River;
}
