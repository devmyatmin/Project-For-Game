using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class OffsetPose : MonoBehaviour
{
	[Serializable]
	public class EffectorLink
	{
		public FullBodyBipedEffector effector;

		public Vector3 offset;

		public Vector3 pin;

		public Vector3 pinWeight;

		public void Apply(IKSolverFullBodyBiped solver, float weight, Quaternion rotation)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			IKEffector iKEffector = solver.GetEffector(effector);
			iKEffector.positionOffset += rotation * offset * weight;
			Vector3 val = solver.GetRoot().position + rotation * pin - solver.GetEffector(effector).bone.position;
			Vector3 val2 = pinWeight * Mathf.Abs(weight);
			solver.GetEffector(effector).positionOffset = new Vector3(Mathf.Lerp(solver.GetEffector(effector).positionOffset.x, val.x, val2.x), Mathf.Lerp(solver.GetEffector(effector).positionOffset.y, val.y, val2.y), Mathf.Lerp(solver.GetEffector(effector).positionOffset.z, val.z, val2.z));
		}
	}

	public EffectorLink[] effectorLinks = new EffectorLink[0];

	public void Apply(IKSolverFullBodyBiped solver, float weight)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < effectorLinks.Length; i++)
		{
			effectorLinks[i].Apply(solver, weight, solver.GetRoot().rotation);
		}
	}

	public void Apply(IKSolverFullBodyBiped solver, float weight, Quaternion rotation)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < effectorLinks.Length; i++)
		{
			effectorLinks[i].Apply(solver, weight, rotation);
		}
	}
}
