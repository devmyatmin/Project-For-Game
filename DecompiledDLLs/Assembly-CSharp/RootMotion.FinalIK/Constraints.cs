using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class Constraints
{
	public Transform transform;

	public Transform target;

	public Vector3 positionOffset;

	public Vector3 position;

	[Range(0f, 1f)]
	public float positionWeight;

	public Vector3 rotationOffset;

	public Vector3 rotation;

	[Range(0f, 1f)]
	public float rotationWeight;

	public bool IsValid()
	{
		return (Object)(object)transform != (Object)null;
	}

	public void Initiate(Transform transform)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		this.transform = transform;
		position = transform.position;
		rotation = transform.eulerAngles;
	}

	public void Update()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		if (IsValid())
		{
			if ((Object)(object)target != (Object)null)
			{
				position = target.position;
			}
			Transform val = transform;
			val.position += positionOffset;
			if (positionWeight > 0f)
			{
				transform.position = Vector3.Lerp(transform.position, position, positionWeight);
			}
			if ((Object)(object)target != (Object)null)
			{
				rotation = target.eulerAngles;
			}
			transform.rotation = Quaternion.Euler(rotationOffset) * transform.rotation;
			if (rotationWeight > 0f)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotation), rotationWeight);
			}
		}
	}
}
