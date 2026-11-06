using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class SecondHandOnGun : MonoBehaviour
{
	public AimIK aim;

	public LimbIK leftArmIK;

	public Transform leftHand;

	public Transform rightHand;

	public GrounderFBBIK grounder;

	public Vector3 leftHandPositionOffset;

	public Vector3 leftHandRotationOffset;

	private Vector3 leftHandPosRelToRight;

	private Quaternion leftHandRotRelToRight;

	private void Start()
	{
		((Behaviour)aim).enabled = false;
		((Behaviour)leftArmIK).enabled = false;
		if ((Object)(object)grounder != (Object)null)
		{
			((Behaviour)grounder.ik).enabled = false;
		}
	}

	private void LateUpdate()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		leftHandPosRelToRight = rightHand.InverseTransformPoint(leftHand.position);
		leftHandRotRelToRight = Quaternion.Inverse(rightHand.rotation) * leftHand.rotation;
		if ((Object)(object)grounder != (Object)null)
		{
			grounder.ik.solver.Update();
		}
		aim.solver.Update();
		leftArmIK.solver.IKPosition = rightHand.TransformPoint(leftHandPosRelToRight + leftHandPositionOffset);
		leftArmIK.solver.IKRotation = rightHand.rotation * Quaternion.Euler(leftHandRotationOffset) * leftHandRotRelToRight;
		leftArmIK.solver.Update();
	}
}
