using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKEffector
{
	public Transform bone;

	public Transform target;

	[Range(0f, 1f)]
	public float positionWeight;

	[Range(0f, 1f)]
	public float rotationWeight;

	public Vector3 position = Vector3.zero;

	public Quaternion rotation = Quaternion.identity;

	public Vector3 positionOffset;

	public bool effectChildNodes = true;

	[Range(0f, 1f)]
	public float maintainRelativePositionWeight;

	public Transform[] childBones = new Transform[0];

	public Transform planeBone1;

	public Transform planeBone2;

	public Transform planeBone3;

	public Quaternion planeRotationOffset = Quaternion.identity;

	private float posW;

	private float rotW;

	private Vector3[] localPositions = new Vector3[0];

	private bool usePlaneNodes;

	private Quaternion animatedPlaneRotation = Quaternion.identity;

	private Vector3 animatedPosition;

	private bool firstUpdate;

	private int chainIndex = -1;

	private int nodeIndex = -1;

	private int plane1ChainIndex;

	private int plane1NodeIndex = -1;

	private int plane2ChainIndex = -1;

	private int plane2NodeIndex = -1;

	private int plane3ChainIndex = -1;

	private int plane3NodeIndex = -1;

	private int[] childChainIndexes = new int[0];

	private int[] childNodeIndexes = new int[0];

	public bool isEndEffector { get; private set; }

	public IKSolver.Node GetNode(IKSolverFullBody solver)
	{
		return solver.chain[chainIndex].nodes[nodeIndex];
	}

	public void PinToBone(float positionWeight, float rotationWeight)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		position = bone.position;
		this.positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
		rotation = bone.rotation;
		this.rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
	}

	public IKEffector()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
	}

	public IKEffector(Transform bone, Transform[] childBones)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		this.bone = bone;
		this.childBones = childBones;
	}

	public bool IsValid(IKSolver solver, ref string message)
	{
		if ((Object)(object)bone == (Object)null)
		{
			message = "IK Effector bone is null.";
			return false;
		}
		if (solver.GetPoint(bone) == null)
		{
			message = "IK Effector is referencing to a bone '" + ((Object)bone).name + "' that does not excist in the Node Chain.";
			return false;
		}
		Transform[] array = childBones;
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)array[i] == (Object)null)
			{
				message = "IK Effector contains a null reference.";
				return false;
			}
		}
		array = childBones;
		foreach (Transform val in array)
		{
			if (solver.GetPoint(val) == null)
			{
				message = "IK Effector is referencing to a bone '" + ((Object)val).name + "' that does not excist in the Node Chain.";
				return false;
			}
		}
		if ((Object)(object)planeBone1 != (Object)null && solver.GetPoint(planeBone1) == null)
		{
			message = "IK Effector is referencing to a bone '" + ((Object)planeBone1).name + "' that does not excist in the Node Chain.";
			return false;
		}
		if ((Object)(object)planeBone2 != (Object)null && solver.GetPoint(planeBone2) == null)
		{
			message = "IK Effector is referencing to a bone '" + ((Object)planeBone2).name + "' that does not excist in the Node Chain.";
			return false;
		}
		if ((Object)(object)planeBone3 != (Object)null && solver.GetPoint(planeBone3) == null)
		{
			message = "IK Effector is referencing to a bone '" + ((Object)planeBone3).name + "' that does not excist in the Node Chain.";
			return false;
		}
		return true;
	}

	public void Initiate(IKSolverFullBody solver)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		position = bone.position;
		rotation = bone.rotation;
		animatedPlaneRotation = Quaternion.identity;
		solver.GetChainAndNodeIndexes(bone, out chainIndex, out nodeIndex);
		childChainIndexes = new int[childBones.Length];
		childNodeIndexes = new int[childBones.Length];
		for (int i = 0; i < childBones.Length; i++)
		{
			solver.GetChainAndNodeIndexes(childBones[i], out childChainIndexes[i], out childNodeIndexes[i]);
		}
		localPositions = new Vector3[childBones.Length];
		usePlaneNodes = false;
		if ((Object)(object)planeBone1 != (Object)null)
		{
			solver.GetChainAndNodeIndexes(planeBone1, out plane1ChainIndex, out plane1NodeIndex);
			if ((Object)(object)planeBone2 != (Object)null)
			{
				solver.GetChainAndNodeIndexes(planeBone2, out plane2ChainIndex, out plane2NodeIndex);
				if ((Object)(object)planeBone3 != (Object)null)
				{
					solver.GetChainAndNodeIndexes(planeBone3, out plane3ChainIndex, out plane3NodeIndex);
					usePlaneNodes = true;
				}
			}
			isEndEffector = true;
		}
		else
		{
			isEndEffector = false;
		}
	}

	public void ResetOffset(IKSolverFullBody solver)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		solver.GetNode(chainIndex, nodeIndex).offset = Vector3.zero;
		for (int i = 0; i < childChainIndexes.Length; i++)
		{
			solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).offset = Vector3.zero;
		}
	}

	public void SetToTarget()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			position = target.position;
			rotation = target.rotation;
		}
	}

	public void OnPreSolve(IKSolverFullBody solver)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
		rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		maintainRelativePositionWeight = Mathf.Clamp(maintainRelativePositionWeight, 0f, 1f);
		posW = positionWeight * solver.IKPositionWeight;
		rotW = rotationWeight * solver.IKPositionWeight;
		solver.GetNode(chainIndex, nodeIndex).effectorPositionWeight = posW;
		solver.GetNode(chainIndex, nodeIndex).effectorRotationWeight = rotW;
		solver.GetNode(chainIndex, nodeIndex).solverRotation = rotation;
		if (float.IsInfinity(positionOffset.x) || float.IsInfinity(positionOffset.y) || float.IsInfinity(positionOffset.z))
		{
			Debug.LogError((object)"Invalid IKEffector.positionOffset (contains Infinity)! Please make sure not to set IKEffector.positionOffset to infinite values.", (Object)(object)bone);
		}
		if (float.IsNaN(positionOffset.x) || float.IsNaN(positionOffset.y) || float.IsNaN(positionOffset.z))
		{
			Debug.LogError((object)"Invalid IKEffector.positionOffset (contains NaN)! Please make sure not to set IKEffector.positionOffset to NaN values.", (Object)(object)bone);
		}
		if (positionOffset.sqrMagnitude > 1E+10f)
		{
			Debug.LogError((object)"Additive effector positionOffset detected in Full Body IK (extremely large value). Make sure you are not circularily adding to effector positionOffset each frame.", (Object)(object)bone);
		}
		if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
		{
			Debug.LogError((object)"Invalid IKEffector.position (contains Infinity)!");
		}
		IKSolver.Node node = solver.GetNode(chainIndex, nodeIndex);
		node.offset += positionOffset * solver.IKPositionWeight;
		if (effectChildNodes && solver.iterations > 0)
		{
			for (int i = 0; i < childBones.Length; i++)
			{
				localPositions[i] = ((Component)childBones[i]).transform.position - ((Component)bone).transform.position;
				IKSolver.Node node2 = solver.GetNode(childChainIndexes[i], childNodeIndexes[i]);
				node2.offset += positionOffset * solver.IKPositionWeight;
			}
		}
		if (usePlaneNodes && maintainRelativePositionWeight > 0f)
		{
			animatedPlaneRotation = Quaternion.LookRotation(planeBone2.position - planeBone1.position, planeBone3.position - planeBone1.position);
		}
		firstUpdate = true;
	}

	public void OnPostWrite()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		positionOffset = Vector3.zero;
	}

	private Quaternion GetPlaneRotation(IKSolverFullBody solver)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Vector3 solverPosition = solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition;
		Vector3 solverPosition2 = solver.GetNode(plane2ChainIndex, plane2NodeIndex).solverPosition;
		Vector3 solverPosition3 = solver.GetNode(plane3ChainIndex, plane3NodeIndex).solverPosition;
		Vector3 val = solverPosition2 - solverPosition;
		Vector3 val2 = solverPosition3 - solverPosition;
		if (val == Vector3.zero)
		{
			Warning.Log("Make sure you are not placing 2 or more FBBIK effectors of the same chain to exactly the same position.", bone);
			return Quaternion.identity;
		}
		return Quaternion.LookRotation(val, val2);
	}

	public void Update(IKSolverFullBody solver)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (firstUpdate)
		{
			animatedPosition = bone.position + solver.GetNode(chainIndex, nodeIndex).offset;
			firstUpdate = false;
		}
		solver.GetNode(chainIndex, nodeIndex).solverPosition = Vector3.Lerp(GetPosition(solver, out planeRotationOffset), position, posW);
		if (effectChildNodes)
		{
			for (int i = 0; i < childBones.Length; i++)
			{
				solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).solverPosition = Vector3.Lerp(solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).solverPosition, solver.GetNode(chainIndex, nodeIndex).solverPosition + localPositions[i], posW);
			}
		}
	}

	private Vector3 GetPosition(IKSolverFullBody solver, out Quaternion planeRotationOffset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		planeRotationOffset = Quaternion.identity;
		if (!isEndEffector)
		{
			return solver.GetNode(chainIndex, nodeIndex).solverPosition;
		}
		if (maintainRelativePositionWeight <= 0f)
		{
			return animatedPosition;
		}
		Vector3 val = bone.position;
		Vector3 val2 = val - planeBone1.position;
		planeRotationOffset = GetPlaneRotation(solver) * Quaternion.Inverse(animatedPlaneRotation);
		val = solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition + planeRotationOffset * val2;
		planeRotationOffset = Quaternion.Lerp(Quaternion.identity, planeRotationOffset, maintainRelativePositionWeight);
		return Vector3.Lerp(animatedPosition, val + solver.GetNode(chainIndex, nodeIndex).offset, maintainRelativePositionWeight);
	}
}
