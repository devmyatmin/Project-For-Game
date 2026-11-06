using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class ConstraintRotation : Constraint
{
	public Quaternion rotation;

	public override void UpdateConstraint()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!(weight <= 0f) && isValid)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, weight);
		}
	}

	public ConstraintRotation()
	{
	}

	public ConstraintRotation(Transform transform)
	{
		base.transform = transform;
	}
}
