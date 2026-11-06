using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class TwoHandedProp : MonoBehaviour
{
	[Range(0f, 1f)]
	public float weight = 1f;

	[Tooltip("The left hand target parented to the right hand.")]
	public Transform leftHandTarget;

	[Tooltip("Left hand poser (poses fingers to match the left hand target).")]
	public Poser leftHandPoser;

	[Tooltip("The weight of pinning the left hand to the prop.")]
	[Range(0f, 1f)]
	public float leftHandWeight = 1f;

	private FullBodyBipedIK ik;

	private Vector3 targetPosRelativeToRight;

	private Quaternion targetRotRelativeToRight;

	private void Start()
	{
		ik = ((Component)this).GetComponent<FullBodyBipedIK>();
		IKSolverFullBodyBiped solver = ik.solver;
		solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
		if ((Object)(object)ik.solver.rightHandEffector.target == (Object)null)
		{
			Debug.LogError((object)"Right Hand Effector needs a Target in this demo.");
		}
	}

	private void LateUpdate()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		targetPosRelativeToRight = ik.references.rightHand.InverseTransformPoint(leftHandTarget.position);
		targetRotRelativeToRight = Quaternion.Inverse(ik.references.rightHand.rotation) * leftHandTarget.rotation;
		ik.solver.leftHandEffector.position = ik.solver.rightHandEffector.target.position + ik.solver.rightHandEffector.target.rotation * targetPosRelativeToRight;
		ik.solver.leftHandEffector.rotation = ik.solver.rightHandEffector.target.rotation * targetRotRelativeToRight;
		ik.solver.rightHandEffector.positionWeight = weight;
		float positionWeight = leftHandWeight * weight;
		ik.solver.leftHandEffector.positionWeight = positionWeight;
		leftHandPoser.weight = positionWeight;
	}

	private void AfterFBBIK()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		ik.solver.leftHandEffector.bone.rotation = Quaternion.Slerp(ik.solver.leftHandEffector.bone.rotation, ik.solver.leftHandEffector.rotation, leftHandWeight * weight);
		ik.solver.rightHandEffector.bone.rotation = Quaternion.Slerp(ik.solver.rightHandEffector.bone.rotation, ik.solver.rightHandEffector.rotation, weight);
	}

	private void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
		}
	}
}
