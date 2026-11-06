using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class FBBIKArmBending : MonoBehaviour
{
	public FullBodyBipedIK ik;

	public Vector3 bendDirectionOffsetLeft;

	public Vector3 bendDirectionOffsetRight;

	public Vector3 characterSpaceBendOffsetLeft;

	public Vector3 characterSpaceBendOffsetRight;

	private Quaternion leftHandTargetRotation;

	private Quaternion rightHandTargetRotation;

	private bool initiated;

	private void LateUpdate()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ik == (Object)null))
		{
			if (!initiated)
			{
				IKSolverFullBodyBiped solver = ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostFBBIK));
				initiated = true;
			}
			if ((Object)(object)ik.solver.leftHandEffector.target != (Object)null)
			{
				Vector3 left = Vector3.left;
				ik.solver.leftArmChain.bendConstraint.direction = ik.solver.leftHandEffector.target.rotation * left + ik.solver.leftHandEffector.target.rotation * bendDirectionOffsetLeft + ((Component)ik).transform.rotation * characterSpaceBendOffsetLeft;
				ik.solver.leftArmChain.bendConstraint.weight = 1f;
			}
			if ((Object)(object)ik.solver.rightHandEffector.target != (Object)null)
			{
				Vector3 right = Vector3.right;
				ik.solver.rightArmChain.bendConstraint.direction = ik.solver.rightHandEffector.target.rotation * right + ik.solver.rightHandEffector.target.rotation * bendDirectionOffsetRight + ((Component)ik).transform.rotation * characterSpaceBendOffsetRight;
				ik.solver.rightArmChain.bendConstraint.weight = 1f;
			}
		}
	}

	private void OnPostFBBIK()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ik == (Object)null))
		{
			if ((Object)(object)ik.solver.leftHandEffector.target != (Object)null)
			{
				ik.references.leftHand.rotation = ik.solver.leftHandEffector.target.rotation;
			}
			if ((Object)(object)ik.solver.rightHandEffector.target != (Object)null)
			{
				ik.references.rightHand.rotation = ik.solver.rightHandEffector.target.rotation;
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostFBBIK));
		}
	}
}
