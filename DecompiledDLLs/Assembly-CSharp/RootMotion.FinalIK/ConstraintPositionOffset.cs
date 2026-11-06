using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class ConstraintPositionOffset : Constraint
{
	public Vector3 offset;

	private Vector3 defaultLocalPosition;

	private Vector3 lastLocalPosition;

	private bool initiated;

	private bool positionChanged
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return transform.localPosition != lastLocalPosition;
		}
	}

	public override void UpdateConstraint()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!(weight <= 0f) && isValid)
		{
			if (!initiated)
			{
				defaultLocalPosition = transform.localPosition;
				lastLocalPosition = transform.localPosition;
				initiated = true;
			}
			if (positionChanged)
			{
				defaultLocalPosition = transform.localPosition;
			}
			transform.localPosition = defaultLocalPosition;
			Transform val = transform;
			val.position += offset * weight;
			lastLocalPosition = transform.localPosition;
		}
	}

	public ConstraintPositionOffset()
	{
	}

	public ConstraintPositionOffset(Transform transform)
	{
		base.transform = transform;
	}
}
