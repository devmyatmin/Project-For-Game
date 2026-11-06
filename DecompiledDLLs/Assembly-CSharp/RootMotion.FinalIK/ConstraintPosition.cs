using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class ConstraintPosition : Constraint
{
	public Vector3 position;

	public override void UpdateConstraint()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!(weight <= 0f) && isValid)
		{
			transform.position = Vector3.Lerp(transform.position, position, weight);
		}
	}

	public ConstraintPosition()
	{
	}

	public ConstraintPosition(Transform transform)
	{
		base.transform = transform;
	}
}
