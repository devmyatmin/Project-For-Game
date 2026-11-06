using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverFABRIK : IKSolverHeuristic
{
	public IterationDelegate OnPreIteration;

	private bool[] limitedBones = new bool[0];

	private Vector3[] solverLocalPositions = new Vector3[0];

	protected override bool boneLengthCanBeZero => false;

	public void SolveForward(Vector3 position)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			if (!Warning.logged)
			{
				LogWarning("Trying to solve uninitiated FABRIK chain.");
			}
		}
		else
		{
			OnPreSolve();
			ForwardReach(position);
		}
	}

	public void SolveBackward(Vector3 position)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			if (!Warning.logged)
			{
				LogWarning("Trying to solve uninitiated FABRIK chain.");
			}
		}
		else
		{
			BackwardReach(position);
			OnPostSolve();
		}
	}

	public override Vector3 GetIKPosition()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target != (Object)null)
		{
			return target.position;
		}
		return IKPosition;
	}

	protected override void OnInitiate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (firstInitiation || !Application.isPlaying)
		{
			IKPosition = bones[bones.Length - 1].transform.position;
		}
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].solverPosition = bones[i].transform.position;
			bones[i].solverRotation = bones[i].transform.rotation;
		}
		limitedBones = new bool[bones.Length];
		solverLocalPositions = new Vector3[bones.Length];
		InitiateBones();
		for (int j = 0; j < bones.Length; j++)
		{
			solverLocalPositions[j] = Quaternion.Inverse(GetParentSolverRotation(j)) * (bones[j].transform.position - GetParentSolverPosition(j));
		}
	}

	protected override void OnUpdate()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (IKPositionWeight <= 0f)
		{
			return;
		}
		IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
		OnPreSolve();
		if ((Object)(object)target != (Object)null)
		{
			IKPosition = target.position;
		}
		if (XY)
		{
			IKPosition.z = bones[0].transform.position.z;
		}
		Vector3 val = ((maxIterations > 1) ? GetSingularityOffset() : Vector3.zero);
		for (int i = 0; i < maxIterations && (!(val == Vector3.zero) || i < 1 || !(tolerance > 0f) || !(positionOffset < tolerance * tolerance)); i++)
		{
			lastLocalDirection = localDirection;
			if (OnPreIteration != null)
			{
				OnPreIteration(i);
			}
			Solve(IKPosition + ((i == 0) ? val : Vector3.zero));
		}
		OnPostSolve();
	}

	private Vector3 SolveJoint(Vector3 pos1, Vector3 pos2, float length)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (XY)
		{
			pos1.z = pos2.z;
		}
		Vector3 val = pos1 - pos2;
		return pos2 + val.normalized * length;
	}

	private void OnPreSolve()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		chainLength = 0f;
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].solverPosition = bones[i].transform.position;
			bones[i].solverRotation = bones[i].transform.rotation;
			if (i < bones.Length - 1)
			{
				Bone obj = bones[i];
				Vector3 val = bones[i].transform.position - bones[i + 1].transform.position;
				obj.length = val.magnitude;
				bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (bones[i + 1].transform.position - bones[i].transform.position);
				chainLength += bones[i].length;
			}
			if (useRotationLimits)
			{
				solverLocalPositions[i] = Quaternion.Inverse(GetParentSolverRotation(i)) * (bones[i].transform.position - GetParentSolverPosition(i));
			}
		}
	}

	private void OnPostSolve()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!useRotationLimits)
		{
			MapToSolverPositions();
		}
		else
		{
			MapToSolverPositionsLimited();
		}
		lastLocalDirection = localDirection;
	}

	private void Solve(Vector3 targetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		ForwardReach(targetPosition);
		BackwardReach(bones[0].transform.position);
	}

	private void ForwardReach(Vector3 position)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		bones[bones.Length - 1].solverPosition = Vector3.Lerp(bones[bones.Length - 1].solverPosition, position, IKPositionWeight);
		for (int i = 0; i < limitedBones.Length; i++)
		{
			limitedBones[i] = false;
		}
		for (int num = bones.Length - 2; num > -1; num--)
		{
			bones[num].solverPosition = SolveJoint(bones[num].solverPosition, bones[num + 1].solverPosition, bones[num].length);
			LimitForward(num, num + 1);
		}
		LimitForward(0, 0);
	}

	private void SolverMove(int index, Vector3 offset)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		for (int i = index; i < bones.Length; i++)
		{
			Bone obj = bones[i];
			obj.solverPosition += offset;
		}
	}

	private void SolverRotate(int index, Quaternion rotation, bool recursive)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = index; i < bones.Length; i++)
		{
			bones[i].solverRotation = rotation * bones[i].solverRotation;
			if (!recursive)
			{
				break;
			}
		}
	}

	private void SolverRotateChildren(int index, Quaternion rotation)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		for (int i = index + 1; i < bones.Length; i++)
		{
			bones[i].solverRotation = rotation * bones[i].solverRotation;
		}
	}

	private void SolverMoveChildrenAroundPoint(int index, Quaternion rotation)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		for (int i = index + 1; i < bones.Length; i++)
		{
			Vector3 val = bones[i].solverPosition - bones[index].solverPosition;
			bones[i].solverPosition = bones[index].solverPosition + rotation * val;
		}
	}

	private Quaternion GetParentSolverRotation(int index)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (index > 0)
		{
			return bones[index - 1].solverRotation;
		}
		if ((Object)(object)bones[0].transform.parent == (Object)null)
		{
			return Quaternion.identity;
		}
		return bones[0].transform.parent.rotation;
	}

	private Vector3 GetParentSolverPosition(int index)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (index > 0)
		{
			return bones[index - 1].solverPosition;
		}
		if ((Object)(object)bones[0].transform.parent == (Object)null)
		{
			return Vector3.zero;
		}
		return bones[0].transform.parent.position;
	}

	private Quaternion GetLimitedRotation(int index, Quaternion q, out bool changed)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		changed = false;
		Quaternion parentSolverRotation = GetParentSolverRotation(index);
		Quaternion localRotation = Quaternion.Inverse(parentSolverRotation) * q;
		Quaternion limitedLocalRotation = bones[index].rotationLimit.GetLimitedLocalRotation(localRotation, out changed);
		if (!changed)
		{
			return q;
		}
		return parentSolverRotation * limitedLocalRotation;
	}

	private void LimitForward(int rotateBone, int limitBone)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		if (!useRotationLimits || (Object)(object)bones[limitBone].rotationLimit == (Object)null)
		{
			return;
		}
		Vector3 solverPosition = bones[bones.Length - 1].solverPosition;
		for (int i = rotateBone; i < bones.Length - 1 && !limitedBones[i]; i++)
		{
			Quaternion rotation = Quaternion.FromToRotation(bones[i].solverRotation * bones[i].axis, bones[i + 1].solverPosition - bones[i].solverPosition);
			SolverRotate(i, rotation, recursive: false);
		}
		bool changed = false;
		Quaternion limitedRotation = GetLimitedRotation(limitBone, bones[limitBone].solverRotation, out changed);
		if (changed)
		{
			if (limitBone < bones.Length - 1)
			{
				Quaternion rotation2 = QuaTools.FromToRotation(bones[limitBone].solverRotation, limitedRotation);
				bones[limitBone].solverRotation = limitedRotation;
				SolverRotateChildren(limitBone, rotation2);
				SolverMoveChildrenAroundPoint(limitBone, rotation2);
				Quaternion rotation3 = Quaternion.FromToRotation(bones[bones.Length - 1].solverPosition - bones[rotateBone].solverPosition, solverPosition - bones[rotateBone].solverPosition);
				SolverRotate(rotateBone, rotation3, recursive: true);
				SolverMoveChildrenAroundPoint(rotateBone, rotation3);
				SolverMove(rotateBone, solverPosition - bones[bones.Length - 1].solverPosition);
			}
			else
			{
				bones[limitBone].solverRotation = limitedRotation;
			}
		}
		limitedBones[limitBone] = true;
	}

	private void BackwardReach(Vector3 position)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (useRotationLimits)
		{
			BackwardReachLimited(position);
		}
		else
		{
			BackwardReachUnlimited(position);
		}
	}

	private void BackwardReachUnlimited(Vector3 position)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		bones[0].solverPosition = position;
		for (int i = 1; i < bones.Length; i++)
		{
			bones[i].solverPosition = SolveJoint(bones[i].solverPosition, bones[i - 1].solverPosition, bones[i - 1].length);
		}
	}

	private void BackwardReachLimited(Vector3 position)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		bones[0].solverPosition = position;
		for (int i = 0; i < bones.Length - 1; i++)
		{
			Vector3 val = SolveJoint(bones[i + 1].solverPosition, bones[i].solverPosition, bones[i].length);
			Quaternion val2 = Quaternion.FromToRotation(bones[i].solverRotation * bones[i].axis, val - bones[i].solverPosition) * bones[i].solverRotation;
			if ((Object)(object)bones[i].rotationLimit != (Object)null)
			{
				bool changed = false;
				val2 = GetLimitedRotation(i, val2, out changed);
			}
			Quaternion rotation = QuaTools.FromToRotation(bones[i].solverRotation, val2);
			bones[i].solverRotation = val2;
			SolverRotateChildren(i, rotation);
			bones[i + 1].solverPosition = bones[i].solverPosition + bones[i].solverRotation * solverLocalPositions[i + 1];
		}
		for (int j = 0; j < bones.Length; j++)
		{
			bones[j].solverRotation = Quaternion.LookRotation(bones[j].solverRotation * Vector3.forward, bones[j].solverRotation * Vector3.up);
		}
	}

	private void MapToSolverPositions()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		bones[0].transform.position = bones[0].solverPosition;
		for (int i = 0; i < bones.Length - 1; i++)
		{
			if (XY)
			{
				bones[i].Swing2D(bones[i + 1].solverPosition);
			}
			else
			{
				bones[i].Swing(bones[i + 1].solverPosition);
			}
		}
	}

	private void MapToSolverPositionsLimited()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		bones[0].transform.position = bones[0].solverPosition;
		for (int i = 0; i < bones.Length; i++)
		{
			if (i < bones.Length - 1)
			{
				bones[i].transform.rotation = bones[i].solverRotation;
			}
		}
	}
}
