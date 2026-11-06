using UnityEngine;

public struct Sample(RaycastHit hit)
{
	public static Sample Empty = new Sample
	{
		Point = Vector3.zero,
		Normal = Vector3.up
	};

	public Vector3 Point = hit.point;

	public Vector3 Normal = hit.normal;

	static Sample()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
	}
}
