using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class FABRIKChain
{
	public FABRIK ik;

	[Range(0f, 1f)]
	public float pull = 1f;

	[Range(0f, 1f)]
	public float pin = 1f;

	public int[] children = new int[0];

	public bool IsValid(ref string message)
	{
		if ((Object)(object)ik == (Object)null)
		{
			message = "IK unassigned in FABRIKChain.";
			return false;
		}
		if (!ik.solver.IsValid(ref message))
		{
			return false;
		}
		return true;
	}

	public void Initiate()
	{
		((Behaviour)ik).enabled = false;
	}

	public void Stage1(FABRIKChain[] chain)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < children.Length; i++)
		{
			chain[children[i]].Stage1(chain);
		}
		if (children.Length == 0)
		{
			ik.solver.SolveForward(ik.solver.GetIKPosition());
		}
		else
		{
			ik.solver.SolveForward(GetCentroid(chain));
		}
	}

	public void Stage2(Vector3 rootPosition, FABRIKChain[] chain)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		ik.solver.SolveBackward(rootPosition);
		for (int i = 0; i < children.Length; i++)
		{
			chain[children[i]].Stage2(ik.solver.bones[ik.solver.bones.Length - 1].transform.position, chain);
		}
	}

	private Vector3 GetCentroid(FABRIKChain[] chain)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		Vector3 iKPosition = ik.solver.GetIKPosition();
		if (pin >= 1f)
		{
			return iKPosition;
		}
		float num = 0f;
		for (int i = 0; i < children.Length; i++)
		{
			num += chain[children[i]].pull;
		}
		if (num <= 0f)
		{
			return iKPosition;
		}
		if (num < 1f)
		{
			num = 1f;
		}
		Vector3 val = iKPosition;
		for (int j = 0; j < children.Length; j++)
		{
			Vector3 val2 = chain[children[j]].ik.solver.bones[0].solverPosition - iKPosition;
			float num2 = chain[children[j]].pull / num;
			val += val2 * num2;
		}
		if (pin <= 0f)
		{
			return val;
		}
		return val + (iKPosition - val) * pin;
	}
}
