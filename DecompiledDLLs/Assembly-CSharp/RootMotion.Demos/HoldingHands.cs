using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class HoldingHands : MonoBehaviour
{
	public FullBodyBipedIK rightHandChar;

	public FullBodyBipedIK leftHandChar;

	public Transform rightHandTarget;

	public Transform leftHandTarget;

	public float crossFade;

	public float speed = 10f;

	private Quaternion rightHandRotation;

	private Quaternion leftHandRotation;

	private void Start()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		rightHandRotation = Quaternion.Inverse(rightHandChar.solver.rightHandEffector.bone.rotation) * ((Component)this).transform.rotation;
		leftHandRotation = Quaternion.Inverse(leftHandChar.solver.leftHandEffector.bone.rotation) * ((Component)this).transform.rotation;
	}

	private void LateUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Lerp(rightHandChar.solver.rightHandEffector.bone.position, leftHandChar.solver.leftHandEffector.bone.position, crossFade);
		((Component)this).transform.position = Vector3.Lerp(((Component)this).transform.position, val, Time.deltaTime * speed);
		((Component)this).transform.rotation = Quaternion.Slerp(rightHandChar.solver.rightHandEffector.bone.rotation * rightHandRotation, leftHandChar.solver.leftHandEffector.bone.rotation * leftHandRotation, crossFade);
		rightHandChar.solver.rightHandEffector.position = rightHandTarget.position;
		rightHandChar.solver.rightHandEffector.rotation = rightHandTarget.rotation;
		leftHandChar.solver.leftHandEffector.position = leftHandTarget.position;
		leftHandChar.solver.leftHandEffector.rotation = leftHandTarget.rotation;
	}
}
