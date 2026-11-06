using UnityEngine;

namespace RootMotion.Demos;

public class PickUpBox : PickUp2Handed
{
	protected override void RotatePivot()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = pivot.position - ((Component)interactionSystem).transform.position;
		Vector3 normalized = val.normalized;
		normalized.y = 0f;
		Vector3 axis = QuaTools.GetAxis(((Component)obj).transform.InverseTransformDirection(normalized));
		Vector3 axis2 = QuaTools.GetAxis(((Component)obj).transform.InverseTransformDirection(((Component)interactionSystem).transform.up));
		pivot.localRotation = Quaternion.LookRotation(axis, axis2);
	}
}
