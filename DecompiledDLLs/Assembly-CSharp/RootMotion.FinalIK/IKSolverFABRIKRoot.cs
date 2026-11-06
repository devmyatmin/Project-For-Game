using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverFABRIKRoot : IKSolver
{
	public int iterations = 4;

	[Range(0f, 1f)]
	public float rootPin;

	public FABRIKChain[] chains = new FABRIKChain[0];

	private bool zeroWeightApplied;

	private bool[] isRoot;

	private Vector3 rootDefaultPosition;

	public override bool IsValid(ref string message)
	{
		if (chains.Length == 0)
		{
			message = "IKSolverFABRIKRoot contains no chains.";
			return false;
		}
		FABRIKChain[] array = chains;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].IsValid(ref message))
			{
				return false;
			}
		}
		for (int j = 0; j < chains.Length; j++)
		{
			for (int k = 0; k < chains.Length; k++)
			{
				if (j != k && (Object)(object)chains[j].ik == (Object)(object)chains[k].ik)
				{
					message = ((Object)chains[j].ik).name + " is represented more than once in IKSolverFABRIKRoot chain.";
					return false;
				}
			}
		}
		for (int l = 0; l < chains.Length; l++)
		{
			for (int m = 0; m < chains[l].children.Length; m++)
			{
				int num = chains[l].children[m];
				if (num < 0)
				{
					message = ((Object)chains[l].ik).name + "IKSolverFABRIKRoot chain at index " + l + " has invalid children array. Child index is < 0.";
					return false;
				}
				if (num == l)
				{
					message = ((Object)chains[l].ik).name + "IKSolverFABRIKRoot chain at index " + l + " has invalid children array. Child index is referencing to itself.";
					return false;
				}
				if (num >= chains.Length)
				{
					message = ((Object)chains[l].ik).name + "IKSolverFABRIKRoot chain at index " + l + " has invalid children array. Child index > number of chains";
					return false;
				}
				for (int n = 0; n < chains.Length; n++)
				{
					if (num != n)
					{
						continue;
					}
					for (int num2 = 0; num2 < chains[n].children.Length; num2++)
					{
						if (chains[n].children[num2] == l)
						{
							message = "Circular parenting. " + ((Object)chains[n].ik).name + " already has " + ((Object)chains[l].ik).name + " listed as its child.";
							return false;
						}
					}
				}
				for (int num3 = 0; num3 < chains[l].children.Length; num3++)
				{
					if (m != num3 && chains[l].children[num3] == num)
					{
						message = "Chain number " + num + " is represented more than once in the children of " + ((Object)chains[l].ik).name;
						return false;
					}
				}
			}
		}
		return true;
	}

	public override void StoreDefaultLocalState()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		rootDefaultPosition = root.localPosition;
		for (int i = 0; i < chains.Length; i++)
		{
			chains[i].ik.solver.StoreDefaultLocalState();
		}
	}

	public override void FixTransforms()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			root.localPosition = rootDefaultPosition;
			for (int i = 0; i < chains.Length; i++)
			{
				chains[i].ik.solver.FixTransforms();
			}
		}
	}

	protected override void OnInitiate()
	{
		for (int i = 0; i < chains.Length; i++)
		{
			chains[i].Initiate();
		}
		isRoot = new bool[chains.Length];
		for (int j = 0; j < chains.Length; j++)
		{
			isRoot[j] = IsRoot(j);
		}
	}

	private bool IsRoot(int index)
	{
		for (int i = 0; i < chains.Length; i++)
		{
			for (int j = 0; j < chains[i].children.Length; j++)
			{
				if (chains[i].children[j] == index)
				{
					return false;
				}
			}
		}
		return true;
	}

	protected override void OnUpdate()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if (IKPositionWeight <= 0f && zeroWeightApplied)
		{
			return;
		}
		IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
		for (int i = 0; i < chains.Length; i++)
		{
			chains[i].ik.solver.IKPositionWeight = IKPositionWeight;
		}
		if (IKPositionWeight <= 0f)
		{
			zeroWeightApplied = true;
			return;
		}
		zeroWeightApplied = false;
		for (int j = 0; j < iterations; j++)
		{
			for (int k = 0; k < chains.Length; k++)
			{
				if (isRoot[k])
				{
					chains[k].Stage1(chains);
				}
			}
			Vector3 centroid = GetCentroid();
			root.position = centroid;
			for (int l = 0; l < chains.Length; l++)
			{
				if (isRoot[l])
				{
					chains[l].Stage2(centroid, chains);
				}
			}
		}
	}

	public override Point[] GetPoints()
	{
		Point[] array = new Point[0];
		for (int i = 0; i < chains.Length; i++)
		{
			AddPointsToArray(ref array, chains[i]);
		}
		return array;
	}

	public override Point GetPoint(Transform transform)
	{
		Point point = null;
		for (int i = 0; i < chains.Length; i++)
		{
			point = chains[i].ik.solver.GetPoint(transform);
			if (point != null)
			{
				return point;
			}
		}
		return null;
	}

	private void AddPointsToArray(ref Point[] array, FABRIKChain chain)
	{
		Point[] points = chain.ik.solver.GetPoints();
		Array.Resize(ref array, array.Length + points.Length);
		int num = 0;
		for (int i = array.Length - points.Length; i < array.Length; i++)
		{
			array[i] = points[num];
			num++;
		}
	}

	private Vector3 GetCentroid()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = root.position;
		if (rootPin >= 1f)
		{
			return val;
		}
		float num = 0f;
		for (int i = 0; i < chains.Length; i++)
		{
			if (isRoot[i])
			{
				num += chains[i].pull;
			}
		}
		for (int j = 0; j < chains.Length; j++)
		{
			if (isRoot[j] && num > 0f)
			{
				val += (chains[j].ik.solver.bones[0].solverPosition - root.position) * (chains[j].pull / Mathf.Clamp(num, 1f, num));
			}
		}
		return Vector3.Lerp(val, root.position, rootPin);
	}
}
