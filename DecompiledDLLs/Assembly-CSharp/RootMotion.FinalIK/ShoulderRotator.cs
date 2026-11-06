using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class ShoulderRotator : MonoBehaviour
{
	[Tooltip("Weight of shoulder rotation")]
	public float weight = 1.5f;

	[Tooltip("The greater the offset, the sooner the shoulder will start rotating")]
	public float offset = 0.2f;

	private FullBodyBipedIK ik;

	private bool skip;

	private void Start()
	{
		ik = ((Component)this).GetComponent<FullBodyBipedIK>();
		IKSolverFullBodyBiped solver = ik.solver;
		solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(RotateShoulders));
	}

	private void RotateShoulders()
	{
		if (!((Object)(object)ik == (Object)null) && !(ik.solver.IKPositionWeight <= 0f))
		{
			if (skip)
			{
				skip = false;
				return;
			}
			RotateShoulder(FullBodyBipedChain.LeftArm, weight, offset);
			RotateShoulder(FullBodyBipedChain.RightArm, weight, offset);
			skip = true;
			ik.solver.Update();
		}
	}

	private void RotateShoulder(FullBodyBipedChain chain, float weight, float offset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.FromToRotation(GetParentBoneMap(chain).swingDirection, ik.solver.GetEndEffector(chain).position - GetParentBoneMap(chain).transform.position);
		Vector3 val2 = ik.solver.GetEndEffector(chain).position - ik.solver.GetLimbMapping(chain).bone1.position;
		float num = ik.solver.GetChain(chain).nodes[0].length + ik.solver.GetChain(chain).nodes[1].length;
		float num2 = val2.magnitude / num - 1f + offset;
		num2 = Mathf.Clamp(num2 * weight, 0f, 1f);
		Quaternion val3 = Quaternion.Lerp(Quaternion.identity, val, num2 * ik.solver.GetEndEffector(chain).positionWeight * ik.solver.IKPositionWeight);
		ik.solver.GetLimbMapping(chain).parentBone.rotation = val3 * ik.solver.GetLimbMapping(chain).parentBone.rotation;
	}

	private IKMapping.BoneMap GetParentBoneMap(FullBodyBipedChain chain)
	{
		return ik.solver.GetLimbMapping(chain).GetBoneMap(IKMappingLimb.BoneMapType.Parent);
	}

	private void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(RotateShoulders));
		}
	}
}
