using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace RootMotion.Demos;

[Serializable]
public class Navigator
{
	public enum State
	{
		Idle,
		Seeking,
		OnPath
	}

	[Tooltip("Should this Navigator be actively seeking a path.")]
	public bool activeTargetSeeking;

	[Tooltip("Increase this value if the character starts running in a circle, not able to reach the corner because of a too large turning radius.")]
	public float cornerRadius = 0.5f;

	[Tooltip("Recalculate path if target position has moved by this distance from the position it was at when the path was originally calculated")]
	public float recalculateOnPathDistance = 1f;

	[Tooltip("Sample within this distance from sourcePosition.")]
	public float maxSampleDistance = 5f;

	[Tooltip("Interval of updating the path")]
	public float nextPathInterval = 3f;

	private Transform transform;

	private int cornerIndex;

	private Vector3[] corners = new Vector3[0];

	private NavMeshPath path;

	private Vector3 lastTargetPosition;

	private bool initiated;

	private float nextPathTime;

	public Vector3 normalizedDeltaPosition
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public State state { get; private set; }

	public void Initiate(Transform transform)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		this.transform = transform;
		path = new NavMeshPath();
		initiated = true;
		cornerIndex = 0;
		corners = new Vector3[0];
		state = State.Idle;
		lastTargetPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
	}

	public void Update(Vector3 targetPosition)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Invalid comparison between Unknown and I4
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Invalid comparison between Unknown and I4
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			Debug.LogError((object)"Trying to update an uninitiated Navigator.");
			return;
		}
		switch (state)
		{
		case State.Seeking:
			normalizedDeltaPosition = Vector3.zero;
			if ((int)path.status == 0)
			{
				corners = path.corners;
				cornerIndex = 0;
				if (corners.Length == 0)
				{
					Debug.LogWarning((object)"Zero Corner Path", (Object)(object)transform);
					Stop();
				}
				else
				{
					state = State.OnPath;
				}
			}
			if ((int)path.status == 1)
			{
				Debug.LogWarning((object)"Path Partial", (Object)(object)transform);
			}
			if ((int)path.status == 2)
			{
				Debug.LogWarning((object)"Path Invalid", (Object)(object)transform);
			}
			break;
		case State.OnPath:
			if (activeTargetSeeking && Time.time > nextPathTime && HorDistance(targetPosition, lastTargetPosition) > recalculateOnPathDistance)
			{
				CalculatePath(targetPosition);
			}
			else
			{
				if (cornerIndex >= corners.Length)
				{
					break;
				}
				Vector3 val = corners[cornerIndex] - transform.position;
				val.y = 0f;
				float magnitude = val.magnitude;
				if (magnitude > 0f)
				{
					normalizedDeltaPosition = val / val.magnitude;
				}
				else
				{
					normalizedDeltaPosition = Vector3.zero;
				}
				if (magnitude < cornerRadius)
				{
					cornerIndex++;
					if (cornerIndex >= corners.Length)
					{
						Stop();
					}
				}
			}
			break;
		case State.Idle:
			if (activeTargetSeeking && Time.time > nextPathTime)
			{
				CalculatePath(targetPosition);
			}
			break;
		}
	}

	private void CalculatePath(Vector3 targetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (Find(targetPosition))
		{
			lastTargetPosition = targetPosition;
			state = State.Seeking;
		}
		else
		{
			Stop();
		}
		nextPathTime = Time.time + nextPathInterval;
	}

	private bool Find(Vector3 targetPosition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (HorDistance(transform.position, targetPosition) < cornerRadius * 2f)
		{
			return false;
		}
		if (NavMesh.CalculatePath(transform.position, targetPosition, -1, path))
		{
			return true;
		}
		NavMeshHit val = default;
		if (NavMesh.SamplePosition(targetPosition, ref val, maxSampleDistance, -1) && NavMesh.CalculatePath(transform.position, val.position, -1, path))
		{
			return true;
		}
		return false;
	}

	private void Stop()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		state = State.Idle;
		normalizedDeltaPosition = Vector3.zero;
	}

	private float HorDistance(Vector3 p1, Vector3 p2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Distance(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z));
	}

	public void Visualize()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (state == State.Idle)
		{
			Gizmos.color = Color.gray;
		}
		if (state == State.Seeking)
		{
			Gizmos.color = Color.red;
		}
		if (state == State.OnPath)
		{
			Gizmos.color = Color.green;
		}
		if (corners.Length != 0 && state == State.OnPath && cornerIndex == 0)
		{
			Gizmos.DrawLine(transform.position, corners[0]);
		}
		for (int i = 0; i < corners.Length; i++)
		{
			Gizmos.DrawSphere(corners[i], 0.1f);
		}
		if (corners.Length > 1)
		{
			for (int j = 0; j < corners.Length - 1; j++)
			{
				Gizmos.DrawLine(corners[j], corners[j + 1]);
			}
		}
		Gizmos.color = Color.white;
	}
}
