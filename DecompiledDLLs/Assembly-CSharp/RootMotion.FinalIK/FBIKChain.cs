using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class FBIKChain
{
	[Serializable]
	public class ChildConstraint
	{
		public float pushElasticity;

		public float pullElasticity;

		[SerializeField]
		private Transform bone1;

		[SerializeField]
		private Transform bone2;

		private float crossFade;

		private float inverseCrossFade;

		private int chain1Index;

		private int chain2Index;

		public float nominalDistance { get; private set; }

		public bool isRigid { get; private set; }

		public ChildConstraint(Transform bone1, Transform bone2, float pushElasticity = 0f, float pullElasticity = 0f)
		{
			this.bone1 = bone1;
			this.bone2 = bone2;
			this.pushElasticity = pushElasticity;
			this.pullElasticity = pullElasticity;
		}

		public void Initiate(IKSolverFullBody solver)
		{
			chain1Index = solver.GetChainIndex(bone1);
			chain2Index = solver.GetChainIndex(bone2);
			OnPreSolve(solver);
		}

		public void OnPreSolve(IKSolverFullBody solver)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			nominalDistance = Vector3.Distance(solver.chain[chain1Index].nodes[0].transform.position, solver.chain[chain2Index].nodes[0].transform.position);
			isRigid = pushElasticity <= 0f && pullElasticity <= 0f;
			if (isRigid)
			{
				float num = solver.chain[chain1Index].pull - solver.chain[chain2Index].pull;
				crossFade = 1f - (0.5f + num * 0.5f);
			}
			else
			{
				crossFade = 0.5f;
			}
			inverseCrossFade = 1f - crossFade;
		}

		public void Solve(IKSolverFullBody solver)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			if (pushElasticity >= 1f && pullElasticity >= 1f)
			{
				return;
			}
			Vector3 val = solver.chain[chain2Index].nodes[0].solverPosition - solver.chain[chain1Index].nodes[0].solverPosition;
			float magnitude = val.magnitude;
			if (magnitude != nominalDistance && magnitude != 0f)
			{
				float num = 1f;
				if (!isRigid)
				{
					float num2 = ((magnitude > nominalDistance) ? pullElasticity : pushElasticity);
					num = 1f - num2;
				}
				num *= 1f - nominalDistance / magnitude;
				Vector3 val2 = val * num;
				IKSolver.Node obj = solver.chain[chain1Index].nodes[0];
				obj.solverPosition += val2 * crossFade;
				IKSolver.Node obj2 = solver.chain[chain2Index].nodes[0];
				obj2.solverPosition -= val2 * inverseCrossFade;
			}
		}
	}

	[Serializable]
	public enum Smoothing
	{
		None,
		Exponential,
		Cubic
	}

	[Range(0f, 1f)]
	public float pin;

	[Range(0f, 1f)]
	public float pull = 1f;

	[Range(0f, 1f)]
	public float push;

	[Range(-1f, 1f)]
	public float pushParent;

	[Range(0f, 1f)]
	public float reach = 0.1f;

	public Smoothing reachSmoothing = Smoothing.Exponential;

	public Smoothing pushSmoothing = Smoothing.Exponential;

	public IKSolver.Node[] nodes = new IKSolver.Node[0];

	public int[] children = new int[0];

	public ChildConstraint[] childConstraints = new ChildConstraint[0];

	public IKConstraintBend bendConstraint = new IKConstraintBend();

	private float rootLength;

	private bool initiated;

	private float length;

	private float distance;

	private IKSolver.Point p;

	private float reachForce;

	private float pullParentSum;

	private float[] crossFades;

	private float sqrMag1;

	private float sqrMag2;

	private float sqrMagDif;

	private const float maxLimbLength = 0.99999f;

	public FBIKChain()
	{
	}

	public FBIKChain(float pin, float pull, params Transform[] nodeTransforms)
	{
		this.pin = pin;
		this.pull = pull;
		SetNodes(nodeTransforms);
		children = new int[0];
	}

	public void SetNodes(params Transform[] boneTransforms)
	{
		nodes = new IKSolver.Node[boneTransforms.Length];
		for (int i = 0; i < boneTransforms.Length; i++)
		{
			nodes[i] = new IKSolver.Node(boneTransforms[i]);
		}
	}

	public int GetNodeIndex(Transform boneTransform)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			if ((Object)(object)nodes[i].transform == (Object)(object)boneTransform)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsValid(ref string message)
	{
		if (nodes.Length == 0)
		{
			message = "FBIK chain contains no nodes.";
			return false;
		}
		IKSolver.Node[] array = nodes;
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)array[i].transform == (Object)null)
			{
				message = "Node transform is null in FBIK chain.";
				return false;
			}
		}
		return true;
	}

	public void Initiate(IKSolverFullBody solver)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		initiated = false;
		IKSolver.Node[] array = nodes;
		foreach (IKSolver.Node obj in array)
		{
			obj.solverPosition = obj.transform.position;
		}
		CalculateBoneLengths(solver);
		ChildConstraint[] array2 = childConstraints;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Initiate(solver);
		}
		if (nodes.Length == 3)
		{
			bendConstraint.SetBones(nodes[0].transform, nodes[1].transform, nodes[2].transform);
			bendConstraint.Initiate(solver);
		}
		crossFades = new float[children.Length];
		initiated = true;
	}

	public void ReadPose(IKSolverFullBody solver, bool fullBody)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return;
		}
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i].solverPosition = nodes[i].transform.position + nodes[i].offset;
		}
		CalculateBoneLengths(solver);
		if (!fullBody)
		{
			return;
		}
		for (int j = 0; j < childConstraints.Length; j++)
		{
			childConstraints[j].OnPreSolve(solver);
		}
		if (children.Length != 0)
		{
			float num = nodes[nodes.Length - 1].effectorPositionWeight;
			for (int k = 0; k < children.Length; k++)
			{
				num += solver.chain[children[k]].nodes[0].effectorPositionWeight * solver.chain[children[k]].pull;
			}
			num = Mathf.Clamp(num, 1f, float.PositiveInfinity);
			for (int l = 0; l < children.Length; l++)
			{
				crossFades[l] = solver.chain[children[l]].nodes[0].effectorPositionWeight * solver.chain[children[l]].pull / num;
			}
		}
		pullParentSum = 0f;
		for (int m = 0; m < children.Length; m++)
		{
			pullParentSum += solver.chain[children[m]].pull;
		}
		pullParentSum = Mathf.Clamp(pullParentSum, 1f, float.PositiveInfinity);
		if (nodes.Length == 3)
		{
			reachForce = reach * Mathf.Clamp(nodes[2].effectorPositionWeight, 0f, 1f);
		}
		else
		{
			reachForce = 0f;
		}
		if (push > 0f && nodes.Length > 1)
		{
			distance = Vector3.Distance(nodes[0].transform.position, nodes[nodes.Length - 1].transform.position);
		}
	}

	private void CalculateBoneLengths(IKSolverFullBody solver)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		length = 0f;
		for (int i = 0; i < nodes.Length - 1; i++)
		{
			nodes[i].length = Vector3.Distance(nodes[i].transform.position, nodes[i + 1].transform.position);
			length += nodes[i].length;
			if (nodes[i].length == 0f)
			{
				Warning.Log("Bone " + ((Object)nodes[i].transform).name + " - " + ((Object)nodes[i + 1].transform).name + " length is zero, can not solve.", nodes[i].transform);
				return;
			}
		}
		for (int j = 0; j < children.Length; j++)
		{
			FBIKChain obj = solver.chain[children[j]];
			Vector3 val = solver.chain[children[j]].nodes[0].transform.position - nodes[nodes.Length - 1].transform.position;
			obj.rootLength = val.magnitude;
			if (solver.chain[children[j]].rootLength == 0f)
			{
				return;
			}
		}
		if (nodes.Length == 3)
		{
			sqrMag1 = nodes[0].length * nodes[0].length;
			sqrMag2 = nodes[1].length * nodes[1].length;
			sqrMagDif = sqrMag1 - sqrMag2;
		}
	}

	public void Reach(IKSolverFullBody solver)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return;
		}
		for (int i = 0; i < children.Length; i++)
		{
			solver.chain[children[i]].Reach(solver);
		}
		if (reachForce <= 0f)
		{
			return;
		}
		Vector3 val = nodes[2].solverPosition - nodes[0].solverPosition;
		if (!(val == Vector3.zero))
		{
			float magnitude = val.magnitude;
			Vector3 val2 = val / magnitude * length;
			float num = Mathf.Clamp(magnitude / length, 1f - reachForce, 1f + reachForce) - 1f;
			num = Mathf.Clamp(num + reachForce, -1f, 1f);
			switch (reachSmoothing)
			{
			case Smoothing.Exponential:
				num *= num;
				break;
			case Smoothing.Cubic:
				num *= num * num;
				break;
			}
			Vector3 val3 = val2 * Mathf.Clamp(num, 0f, magnitude);
			IKSolver.Node obj = nodes[0];
			obj.solverPosition += val3 * (1f - nodes[0].effectorPositionWeight);
			IKSolver.Node obj2 = nodes[2];
			obj2.solverPosition += val3;
		}
	}

	public Vector3 Push(IKSolverFullBody solver)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		for (int i = 0; i < children.Length; i++)
		{
			val += solver.chain[children[i]].Push(solver) * solver.chain[children[i]].pushParent;
		}
		IKSolver.Node obj = nodes[nodes.Length - 1];
		obj.solverPosition += val;
		if (nodes.Length < 2)
		{
			return Vector3.zero;
		}
		if (push <= 0f)
		{
			return Vector3.zero;
		}
		Vector3 val2 = nodes[2].solverPosition - nodes[0].solverPosition;
		float magnitude = val2.magnitude;
		if (magnitude == 0f)
		{
			return Vector3.zero;
		}
		float num = 1f - magnitude / distance;
		if (num <= 0f)
		{
			return Vector3.zero;
		}
		switch (pushSmoothing)
		{
		case Smoothing.Exponential:
			num *= num;
			break;
		case Smoothing.Cubic:
			num *= num * num;
			break;
		}
		Vector3 val3 = -val2 * num * push;
		IKSolver.Node obj2 = nodes[0];
		obj2.solverPosition += val3;
		return val3;
	}

	public void SolveTrigonometric(IKSolverFullBody solver, bool calculateBendDirection = false)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return;
		}
		for (int i = 0; i < children.Length; i++)
		{
			solver.chain[children[i]].SolveTrigonometric(solver, calculateBendDirection);
		}
		if (nodes.Length == 3)
		{
			Vector3 val = nodes[2].solverPosition - nodes[0].solverPosition;
			float magnitude = val.magnitude;
			if (magnitude != 0f)
			{
				float num = Mathf.Clamp(magnitude, 0f, length * 0.99999f);
				Vector3 direction = val / magnitude * num;
				Vector3 bendDirection = ((calculateBendDirection && bendConstraint.initiated) ? bendConstraint.GetDir(solver) : (nodes[1].solverPosition - nodes[0].solverPosition));
				Vector3 dirToBendPoint = GetDirToBendPoint(direction, bendDirection, num);
				nodes[1].solverPosition = nodes[0].solverPosition + dirToBendPoint;
			}
		}
	}

	public void Stage1(IKSolverFullBody solver)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < children.Length; i++)
		{
			solver.chain[children[i]].Stage1(solver);
		}
		if (children.Length == 0)
		{
			ForwardReach(nodes[nodes.Length - 1].solverPosition);
			return;
		}
		Vector3 val = nodes[nodes.Length - 1].solverPosition;
		SolveChildConstraints(solver);
		for (int j = 0; j < children.Length; j++)
		{
			Vector3 val2 = solver.chain[children[j]].nodes[0].solverPosition;
			if (solver.chain[children[j]].rootLength > 0f)
			{
				val2 = SolveFABRIKJoint(nodes[nodes.Length - 1].solverPosition, solver.chain[children[j]].nodes[0].solverPosition, solver.chain[children[j]].rootLength);
			}
			if (pullParentSum > 0f)
			{
				val += (val2 - nodes[nodes.Length - 1].solverPosition) * (solver.chain[children[j]].pull / pullParentSum);
			}
		}
		ForwardReach(Vector3.Lerp(val, nodes[nodes.Length - 1].solverPosition, pin));
	}

	public void Stage2(IKSolverFullBody solver, Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		BackwardReach(position);
		int num = Mathf.Clamp(solver.iterations, 2, 4);
		if (childConstraints.Length != 0)
		{
			for (int i = 0; i < num; i++)
			{
				SolveConstraintSystems(solver);
			}
		}
		for (int j = 0; j < children.Length; j++)
		{
			solver.chain[children[j]].Stage2(solver, nodes[nodes.Length - 1].solverPosition);
		}
	}

	public void SolveConstraintSystems(IKSolverFullBody solver)
	{
		SolveChildConstraints(solver);
		for (int i = 0; i < children.Length; i++)
		{
			SolveLinearConstraint(nodes[nodes.Length - 1], solver.chain[children[i]].nodes[0], crossFades[i], solver.chain[children[i]].rootLength);
		}
	}

	private Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = pos1 - pos2;
		return pos2 + val.normalized * length;
	}

	protected Vector3 GetDirToBendPoint(Vector3 direction, Vector3 bendDirection, float directionMagnitude)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		float num = (directionMagnitude * directionMagnitude + sqrMagDif) / 2f / directionMagnitude;
		float num2 = (float)Math.Sqrt(Mathf.Clamp(sqrMag1 - num * num, 0f, float.PositiveInfinity));
		if (direction == Vector3.zero)
		{
			return Vector3.zero;
		}
		return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, num2, num);
	}

	private void SolveChildConstraints(IKSolverFullBody solver)
	{
		for (int i = 0; i < childConstraints.Length; i++)
		{
			childConstraints[i].Solve(solver);
		}
	}

	private void SolveLinearConstraint(IKSolver.Node node1, IKSolver.Node node2, float crossFade, float distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = node2.solverPosition - node1.solverPosition;
		float magnitude = val.magnitude;
		if (distance != magnitude && magnitude != 0f)
		{
			Vector3 val2 = val * (1f - distance / magnitude);
			node1.solverPosition += val2 * crossFade;
			node2.solverPosition -= val2 * (1f - crossFade);
		}
	}

	public void ForwardReach(Vector3 position)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		nodes[nodes.Length - 1].solverPosition = position;
		for (int num = nodes.Length - 2; num > -1; num--)
		{
			nodes[num].solverPosition = SolveFABRIKJoint(nodes[num].solverPosition, nodes[num + 1].solverPosition, nodes[num].length);
		}
	}

	private void BackwardReach(Vector3 position)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (rootLength > 0f)
		{
			position = SolveFABRIKJoint(nodes[0].solverPosition, position, rootLength);
		}
		nodes[0].solverPosition = position;
		for (int i = 1; i < nodes.Length; i++)
		{
			nodes[i].solverPosition = SolveFABRIKJoint(nodes[i].solverPosition, nodes[i - 1].solverPosition, nodes[i - 1].length);
		}
	}
}
