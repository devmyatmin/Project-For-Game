using UnityEngine;

namespace Beam.Terrain;

public class GenerationPoints
{
	public Vector3 Position;

	public Quaternion Rotation;

	public bool Occupied;

	public float steepNess;

	public float Soilmap;

	public GenerationPoints(Vector3 pos, Quaternion rot, bool occupied, float steepness, float soilmap)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Position = pos;
		Rotation = rot;
		Occupied = occupied;
		steepNess = steepness;
		Soilmap = soilmap;
	}
}
