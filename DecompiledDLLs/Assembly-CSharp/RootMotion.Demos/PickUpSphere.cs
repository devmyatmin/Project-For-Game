using UnityEngine;

namespace RootMotion.Demos;

public class PickUpSphere : PickUp2Handed
{
	protected override void RotatePivot()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Lerp(interactionSystem.ik.solver.leftHandEffector.bone.position, interactionSystem.ik.solver.rightHandEffector.bone.position, 0.5f);
		Vector3 val2 = ((Component)obj).transform.position - val;
		pivot.rotation = Quaternion.LookRotation(val2);
	}
}
