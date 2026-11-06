using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class BipedIKvsAnimatorIK : MonoBehaviour
{
	[LargeHeader("References")]
	public Animator animator;

	public BipedIK bipedIK;

	[LargeHeader("Look At")]
	public Transform lookAtTargetBiped;

	public Transform lookAtTargetAnimator;

	[Range(0f, 1f)]
	public float lookAtWeight = 1f;

	[Range(0f, 1f)]
	public float lookAtBodyWeight = 1f;

	[Range(0f, 1f)]
	public float lookAtHeadWeight = 1f;

	[Range(0f, 1f)]
	public float lookAtEyesWeight = 1f;

	[Range(0f, 1f)]
	public float lookAtClampWeight = 0.5f;

	[Range(0f, 1f)]
	public float lookAtClampWeightHead = 0.5f;

	[Range(0f, 1f)]
	public float lookAtClampWeightEyes = 0.5f;

	[LargeHeader("Foot")]
	public Transform footTargetBiped;

	public Transform footTargetAnimator;

	[Range(0f, 1f)]
	public float footPositionWeight;

	[Range(0f, 1f)]
	public float footRotationWeight;

	[LargeHeader("Hand")]
	public Transform handTargetBiped;

	public Transform handTargetAnimator;

	[Range(0f, 1f)]
	public float handPositionWeight;

	[Range(0f, 1f)]
	public float handRotationWeight;

	private void OnAnimatorIK(int layer)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		((Component)animator).transform.rotation = ((Component)bipedIK).transform.rotation;
		Vector3 val = ((Component)animator).transform.position - ((Component)bipedIK).transform.position;
		lookAtTargetAnimator.position = lookAtTargetBiped.position + val;
		bipedIK.SetLookAtPosition(lookAtTargetBiped.position);
		bipedIK.SetLookAtWeight(lookAtWeight, lookAtBodyWeight, lookAtHeadWeight, lookAtEyesWeight, lookAtClampWeight, lookAtClampWeightHead, lookAtClampWeightEyes);
		animator.SetLookAtPosition(lookAtTargetAnimator.position);
		animator.SetLookAtWeight(lookAtWeight, lookAtBodyWeight, lookAtHeadWeight, lookAtEyesWeight, lookAtClampWeight);
		footTargetAnimator.position = footTargetBiped.position + val;
		footTargetAnimator.rotation = footTargetBiped.rotation;
		bipedIK.SetIKPosition((AvatarIKGoal)0, footTargetBiped.position);
		bipedIK.SetIKRotation((AvatarIKGoal)0, footTargetBiped.rotation);
		bipedIK.SetIKPositionWeight((AvatarIKGoal)0, footPositionWeight);
		bipedIK.SetIKRotationWeight((AvatarIKGoal)0, footRotationWeight);
		animator.SetIKPosition((AvatarIKGoal)0, footTargetAnimator.position);
		animator.SetIKRotation((AvatarIKGoal)0, footTargetAnimator.rotation);
		animator.SetIKPositionWeight((AvatarIKGoal)0, footPositionWeight);
		animator.SetIKRotationWeight((AvatarIKGoal)0, footRotationWeight);
		handTargetAnimator.position = handTargetBiped.position + val;
		handTargetAnimator.rotation = handTargetBiped.rotation;
		bipedIK.SetIKPosition((AvatarIKGoal)2, handTargetBiped.position);
		bipedIK.SetIKRotation((AvatarIKGoal)2, handTargetBiped.rotation);
		bipedIK.SetIKPositionWeight((AvatarIKGoal)2, handPositionWeight);
		bipedIK.SetIKRotationWeight((AvatarIKGoal)2, handRotationWeight);
		animator.SetIKPosition((AvatarIKGoal)2, handTargetAnimator.position);
		animator.SetIKRotation((AvatarIKGoal)2, handTargetAnimator.rotation);
		animator.SetIKPositionWeight((AvatarIKGoal)2, handPositionWeight);
		animator.SetIKRotationWeight((AvatarIKGoal)2, handRotationWeight);
	}
}
