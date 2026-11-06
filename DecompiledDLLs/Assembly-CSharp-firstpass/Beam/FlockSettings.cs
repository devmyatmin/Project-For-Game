using System;
using UnityEngine;

namespace Beam;

[Serializable]
public class FlockSettings
{
	public const int MAX_FLOCK_COUNT = 100;

	[Range(1f, 100f)]
	public int FlockCount = 1;

	public float BoundsRadius = 1f;

	public BoundsType BoundsType;

	public int SimulationCells = 1;

	public int SimulationTimestep = 10;

	public float MaxSpeed = 2f;

	public float MaxSteer = 0.03f;

	public float AvoidanceDistance = 0.2f;

	public float AllignmentDistance = 0.35f;

	public float CohesionDistance = 1f;

	[Range(0f, 1f)]
	public float AvoidanceWeight = 1f;

	[Range(0f, 1f)]
	public float AllignmentWeight = 0.5f;

	[Range(0f, 1f)]
	public float CohesionWeight = 0.1f;

	[Range(0f, 1f)]
	public float BoundsWeight = 1f;
}
