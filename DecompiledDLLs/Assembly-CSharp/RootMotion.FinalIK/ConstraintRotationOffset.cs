using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class ConstraintRotationOffset : Constraint
{
	public Quaternion offset;

	private Quaternion defaultRotation;

	private Quaternion defaultLocalRotation;

	private Quaternion lastLocalRotation;

	private Quaternion defaultTargetLocalRotation;

	private bool initiated;

	private bool rotationChanged
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return transform.localRotation != lastLocalRotation;
		}
	}

	public override void UpdateConstraint()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!(weight <= 0f) && isValid)
		{
			if (!initiated)
			{
				defaultLocalRotation = transform.localRotation;
				lastLocalRotation = transform.localRotation;
				initiated = true;
			}
			if (rotationChanged)
			{
				defaultLocalRotation = transform.localRotation;
			}
			transform.localRotation = defaultLocalRotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, offset, weight);
			lastLocalRotation = transform.localRotation;
		}
	}

	public ConstraintRotationOffset()
	{
	}

	public ConstraintRotationOffset(Transform transform)
	{
		base.transform = transform;
	}
}
