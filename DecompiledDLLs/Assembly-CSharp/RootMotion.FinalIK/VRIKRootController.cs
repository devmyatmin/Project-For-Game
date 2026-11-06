using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RootMotion.FinalIK;

public class VRIKRootController : MonoBehaviour
{
	private Transform pelvisTarget;

	private Transform leftFootTarget;

	private Transform rightFootTarget;

	private VRIK ik;

	public Vector3 pelvisTargetRight
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	private void Awake()
	{
		ik = ((Component)this).GetComponent<VRIK>();
		IKSolverVR solver = ik.solver;
		solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(OnPreUpdate));
		Calibrate();
	}

	public void Calibrate()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ik == (Object)null)
		{
			Debug.LogError((object)"No VRIK found on VRIKRootController's GameObject.", (Object)(object)((Component)this).transform);
			return;
		}
		pelvisTarget = ik.solver.spine.pelvisTarget;
		leftFootTarget = ik.solver.leftLeg.target;
		rightFootTarget = ik.solver.rightLeg.target;
		if ((Object)(object)pelvisTarget != (Object)null)
		{
			pelvisTargetRight = Quaternion.Inverse(pelvisTarget.rotation) * ik.references.root.right;
		}
	}

	public void Calibrate(VRIKCalibrator.CalibrationData data)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ik == (Object)null)
		{
			Debug.LogError((object)"No VRIK found on VRIKRootController's GameObject.", (Object)(object)((Component)this).transform);
			return;
		}
		pelvisTarget = ik.solver.spine.pelvisTarget;
		leftFootTarget = ik.solver.leftLeg.target;
		rightFootTarget = ik.solver.rightLeg.target;
		if ((Object)(object)pelvisTarget != (Object)null)
		{
			pelvisTargetRight = data.pelvisTargetRight;
		}
	}

	private void OnPreUpdate()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			if ((Object)(object)pelvisTarget != (Object)null)
			{
				ik.references.root.position = new Vector3(pelvisTarget.position.x, ik.references.root.position.y, pelvisTarget.position.z);
				Vector3 val = Vector3.Cross(pelvisTarget.rotation * pelvisTargetRight, ik.references.root.up);
				val.y = 0f;
				ik.references.root.rotation = Quaternion.LookRotation(val);
				ik.references.pelvis.position = Vector3.Lerp(ik.references.pelvis.position, pelvisTarget.position, ik.solver.spine.pelvisPositionWeight);
				ik.references.pelvis.rotation = Quaternion.Slerp(ik.references.pelvis.rotation, pelvisTarget.rotation, ik.solver.spine.pelvisRotationWeight);
			}
			else if ((Object)(object)leftFootTarget != (Object)null && (Object)(object)rightFootTarget != (Object)null)
			{
				ik.references.root.position = Vector3.Lerp(leftFootTarget.position, rightFootTarget.position, 0.5f);
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolverVR solver = ik.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(OnPreUpdate));
		}
	}
}
