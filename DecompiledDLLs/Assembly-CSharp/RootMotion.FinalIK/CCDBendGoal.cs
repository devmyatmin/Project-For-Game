using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class CCDBendGoal : MonoBehaviour
{
	public CCDIK ik;

	[Range(0f, 1f)]
	public float weight = 1f;

	private void Start()
	{
		IKSolverCCD solver = ik.solver;
		solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(BeforeIK));
	}

	private void BeforeIK()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		float num = ik.solver.IKPositionWeight * weight;
		if (!(num <= 0f))
		{
			Vector3 position = ik.solver.bones[0].transform.position;
			Quaternion val = Quaternion.FromToRotation(ik.solver.bones[ik.solver.bones.Length - 1].transform.position - position, ((Component)this).transform.position - position);
			if (num < 1f)
			{
				val = Quaternion.Slerp(Quaternion.identity, val, num);
			}
			ik.solver.bones[0].transform.rotation = val * ik.solver.bones[0].transform.rotation;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolverCCD solver = ik.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(BeforeIK));
		}
	}
}
