using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverArm : IKSolver
{
	[Range(0f, 1f)]
	public float IKRotationWeight = 1f;

	public Quaternion IKRotation = Quaternion.identity;

	public Point chest = new Point();

	public Point shoulder = new Point();

	public Point upperArm = new Point();

	public Point forearm = new Point();

	public Point hand = new Point();

	public bool isLeft;

	public IKSolverVR.Arm arm = new IKSolverVR.Arm();

	private Vector3[] positions = new Vector3[6];

	private Quaternion[] rotations = new Quaternion[6];

	public override bool IsValid(ref string message)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected Obj, but got Unknown
		if ((Object)(object)chest.transform == (Object)null || (Object)(object)shoulder.transform == (Object)null || (Object)(object)upperArm.transform == (Object)null || (Object)(object)forearm.transform == (Object)null || (Object)(object)hand.transform == (Object)null)
		{
			message = "Please assign all bone slots of the Arm IK solver.";
			return false;
		}
		Object[] objects = (Object[])(object)new Transform[5] { chest.transform, shoulder.transform, upperArm.transform, forearm.transform, hand.transform };
		Transform val = (Transform)Hierarchy.ContainsDuplicate(objects);
		if ((Object)(object)val != (Object)null)
		{
			message = ((Object)val).name + " is represented multiple times in the ArmIK.";
			return false;
		}
		return true;
	}

	public bool SetChain(Transform chest, Transform shoulder, Transform upperArm, Transform forearm, Transform hand, Transform root)
	{
		this.chest.transform = chest;
		this.shoulder.transform = shoulder;
		this.upperArm.transform = upperArm;
		this.forearm.transform = forearm;
		this.hand.transform = hand;
		Initiate(root);
		return initiated;
	}

	public override Point[] GetPoints()
	{
		return new Point[5] { chest, shoulder, upperArm, forearm, hand };
	}

	public override Point GetPoint(Transform transform)
	{
		if ((Object)(object)chest.transform == (Object)(object)transform)
		{
			return chest;
		}
		if ((Object)(object)shoulder.transform == (Object)(object)transform)
		{
			return shoulder;
		}
		if ((Object)(object)upperArm.transform == (Object)(object)transform)
		{
			return upperArm;
		}
		if ((Object)(object)forearm.transform == (Object)(object)transform)
		{
			return forearm;
		}
		if ((Object)(object)hand.transform == (Object)(object)transform)
		{
			return hand;
		}
		return null;
	}

	public override void StoreDefaultLocalState()
	{
		shoulder.StoreDefaultLocalState();
		upperArm.StoreDefaultLocalState();
		forearm.StoreDefaultLocalState();
		hand.StoreDefaultLocalState();
	}

	public override void FixTransforms()
	{
		if (initiated)
		{
			shoulder.FixTransform();
			upperArm.FixTransform();
			forearm.FixTransform();
			hand.FixTransform();
		}
	}

	protected override void OnInitiate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		IKPosition = hand.transform.position;
		IKRotation = hand.transform.rotation;
		Read();
	}

	protected override void OnUpdate()
	{
		Read();
		Solve();
		Write();
	}

	private void Solve()
	{
		arm.PreSolve(1f);
		arm.ApplyOffsets(1f);
		arm.Solve(isLeft);
		arm.ResetOffsets();
	}

	private void Read()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		arm.IKPosition = IKPosition;
		arm.positionWeight = IKPositionWeight;
		arm.IKRotation = IKRotation;
		arm.rotationWeight = IKRotationWeight;
		positions[0] = root.position;
		positions[1] = chest.transform.position;
		positions[2] = shoulder.transform.position;
		positions[3] = upperArm.transform.position;
		positions[4] = forearm.transform.position;
		positions[5] = hand.transform.position;
		rotations[0] = root.rotation;
		rotations[1] = chest.transform.rotation;
		rotations[2] = shoulder.transform.rotation;
		rotations[3] = upperArm.transform.rotation;
		rotations[4] = forearm.transform.rotation;
		rotations[5] = hand.transform.rotation;
		arm.Read(positions, rotations, hasChest: false, hasNeck: false, hasShoulders: true, hasToes: false, hasLegs: false, 1, 2);
	}

	private void Write()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		arm.Write(ref positions, ref rotations);
		shoulder.transform.rotation = rotations[2];
		upperArm.transform.rotation = rotations[3];
		forearm.transform.rotation = rotations[4];
		hand.transform.rotation = rotations[5];
		forearm.transform.position = positions[4];
		hand.transform.position = positions[5];
	}

	public IKSolverArm()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
