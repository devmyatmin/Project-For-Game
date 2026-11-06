using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class AnimatorController3rdPersonIK : AnimatorController3rdPerson
{
	[Range(0f, 1f)]
	public float headLookWeight = 1f;

	public Vector3 gunHoldOffset;

	public Vector3 leftHandOffset;

	public Recoil recoil;

	private AimIK aim;

	private FullBodyBipedIK ik;

	private Vector3 headLookAxis;

	private Vector3 leftHandPosRelToRightHand;

	private Quaternion leftHandRotRelToRightHand;

	private Vector3 aimTarget;

	private Quaternion rightHandRotation;

	protected override void Start()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		aim = ((Component)this).GetComponent<AimIK>();
		ik = ((Component)this).GetComponent<FullBodyBipedIK>();
		IKSolverFullBodyBiped solver = ik.solver;
		solver.OnPreRead = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreRead, new IKSolver.UpdateDelegate(OnPreRead));
		((Behaviour)aim).enabled = false;
		((Behaviour)ik).enabled = false;
		headLookAxis = ik.references.head.InverseTransformVector(ik.references.root.forward);
		animator.SetLayerWeight(1, 1f);
	}

	public override void Move(Vector3 moveInput, bool isMoving, Vector3 faceDirection, Vector3 aimTarget)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.Move(moveInput, isMoving, faceDirection, aimTarget);
		this.aimTarget = aimTarget;
		Read();
		AimIK();
		FBBIK();
		AimIK();
		HeadLookAt(aimTarget);
	}

	private void Read()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		leftHandPosRelToRightHand = ik.references.rightHand.InverseTransformPoint(ik.references.leftHand.position);
		leftHandRotRelToRightHand = Quaternion.Inverse(ik.references.rightHand.rotation) * ik.references.leftHand.rotation;
	}

	private void AimIK()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		aim.solver.IKPosition = aimTarget;
		aim.solver.Update();
	}

	private void FBBIK()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		rightHandRotation = ik.references.rightHand.rotation;
		Vector3 val = ik.references.rightHand.rotation * gunHoldOffset;
		IKEffector rightHandEffector = ik.solver.rightHandEffector;
		rightHandEffector.positionOffset += val;
		if ((Object)(object)recoil != (Object)null)
		{
			recoil.SetHandRotations(rightHandRotation * leftHandRotRelToRightHand, rightHandRotation);
		}
		ik.solver.Update();
		if ((Object)(object)recoil != (Object)null)
		{
			ik.references.rightHand.rotation = recoil.rotationOffset * rightHandRotation;
			ik.references.leftHand.rotation = recoil.rotationOffset * rightHandRotation * leftHandRotRelToRightHand;
		}
		else
		{
			ik.references.rightHand.rotation = rightHandRotation;
			ik.references.leftHand.rotation = rightHandRotation * leftHandRotRelToRightHand;
		}
	}

	private void OnPreRead()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = (((Object)(object)recoil != (Object)null) ? (recoil.rotationOffset * rightHandRotation) : rightHandRotation);
		Vector3 val2 = ik.references.rightHand.position + ik.solver.rightHandEffector.positionOffset + val * leftHandPosRelToRightHand;
		IKEffector leftHandEffector = ik.solver.leftHandEffector;
		leftHandEffector.positionOffset += val2 - ik.references.leftHand.position - ik.solver.leftHandEffector.positionOffset + val * leftHandOffset;
	}

	private void HeadLookAt(Vector3 lookAtTarget)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.FromToRotation(ik.references.head.rotation * headLookAxis, lookAtTarget - ik.references.head.position);
		ik.references.head.rotation = Quaternion.Lerp(Quaternion.identity, val, headLookWeight) * ik.references.head.rotation;
	}

	private void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPreRead = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreRead, new IKSolver.UpdateDelegate(OnPreRead));
		}
	}
}
