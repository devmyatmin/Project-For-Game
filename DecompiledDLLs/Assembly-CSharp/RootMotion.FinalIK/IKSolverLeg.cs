using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverLeg : IKSolver
{
	[Range(0f, 1f)]
	public float IKRotationWeight = 1f;

	public Quaternion IKRotation = Quaternion.identity;

	public Point pelvis = new Point();

	public Point thigh = new Point();

	public Point calf = new Point();

	public Point foot = new Point();

	public Point toe = new Point();

	public IKSolverVR.Leg leg = new IKSolverVR.Leg();

	public Vector3 heelOffset;

	private Vector3[] positions = new Vector3[6];

	private Quaternion[] rotations = new Quaternion[6];

	public override bool IsValid(ref string message)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected Obj, but got Unknown
		if ((Object)(object)pelvis.transform == (Object)null || (Object)(object)thigh.transform == (Object)null || (Object)(object)calf.transform == (Object)null || (Object)(object)foot.transform == (Object)null || (Object)(object)toe.transform == (Object)null)
		{
			message = "Please assign all bone slots of the Leg IK solver.";
			return false;
		}
		Object[] objects = (Object[])(object)new Transform[5] { pelvis.transform, thigh.transform, calf.transform, foot.transform, toe.transform };
		Transform val = (Transform)Hierarchy.ContainsDuplicate(objects);
		if ((Object)(object)val != (Object)null)
		{
			message = ((Object)val).name + " is represented multiple times in the LegIK.";
			return false;
		}
		return true;
	}

	public bool SetChain(Transform pelvis, Transform thigh, Transform calf, Transform foot, Transform toe, Transform root)
	{
		this.pelvis.transform = pelvis;
		this.thigh.transform = thigh;
		this.calf.transform = calf;
		this.foot.transform = foot;
		this.toe.transform = toe;
		Initiate(root);
		return initiated;
	}

	public override Point[] GetPoints()
	{
		return new Point[5] { pelvis, thigh, calf, foot, toe };
	}

	public override Point GetPoint(Transform transform)
	{
		if ((Object)(object)pelvis.transform == (Object)(object)transform)
		{
			return pelvis;
		}
		if ((Object)(object)thigh.transform == (Object)(object)transform)
		{
			return thigh;
		}
		if ((Object)(object)calf.transform == (Object)(object)transform)
		{
			return calf;
		}
		if ((Object)(object)foot.transform == (Object)(object)transform)
		{
			return foot;
		}
		if ((Object)(object)toe.transform == (Object)(object)transform)
		{
			return toe;
		}
		return null;
	}

	public override void StoreDefaultLocalState()
	{
		thigh.StoreDefaultLocalState();
		calf.StoreDefaultLocalState();
		foot.StoreDefaultLocalState();
		toe.StoreDefaultLocalState();
	}

	public override void FixTransforms()
	{
		if (initiated)
		{
			thigh.FixTransform();
			calf.FixTransform();
			foot.FixTransform();
			toe.FixTransform();
		}
	}

	protected override void OnInitiate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		IKPosition = toe.transform.position;
		IKRotation = toe.transform.rotation;
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
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		IKSolverVR.Leg leg = this.leg;
		leg.heelPositionOffset += heelOffset;
		this.leg.PreSolve(1f);
		this.leg.ApplyOffsets(1f);
		this.leg.Solve(stretch: true);
		this.leg.ResetOffsets();
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
		leg.IKPosition = IKPosition;
		leg.positionWeight = IKPositionWeight;
		leg.IKRotation = IKRotation;
		leg.rotationWeight = IKRotationWeight;
		positions[0] = root.position;
		positions[1] = pelvis.transform.position;
		positions[2] = thigh.transform.position;
		positions[3] = calf.transform.position;
		positions[4] = foot.transform.position;
		positions[5] = toe.transform.position;
		rotations[0] = root.rotation;
		rotations[1] = pelvis.transform.rotation;
		rotations[2] = thigh.transform.rotation;
		rotations[3] = calf.transform.rotation;
		rotations[4] = foot.transform.rotation;
		rotations[5] = toe.transform.rotation;
		leg.Read(positions, rotations, hasChest: false, hasNeck: false, hasShoulders: false, hasToes: true, hasLegs: true, 1, 2);
	}

	private void Write()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		leg.Write(ref positions, ref rotations);
		thigh.transform.rotation = rotations[2];
		calf.transform.rotation = rotations[3];
		foot.transform.rotation = rotations[4];
		toe.transform.rotation = rotations[5];
		calf.transform.position = positions[3];
		foot.transform.position = positions[4];
	}

	public IKSolverLeg()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
