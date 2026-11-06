using System;
using UnityEngine;

namespace RootMotion;

[Serializable]
public class BipedLimbOrientations
{
	[Serializable]
	public class LimbOrientation
	{
		public Vector3 upperBoneForwardAxis;

		public Vector3 lowerBoneForwardAxis;

		public Vector3 lastBoneLeftAxis;

		public LimbOrientation(Vector3 upperBoneForwardAxis, Vector3 lowerBoneForwardAxis, Vector3 lastBoneLeftAxis)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			this.upperBoneForwardAxis = upperBoneForwardAxis;
			this.lowerBoneForwardAxis = lowerBoneForwardAxis;
			this.lastBoneLeftAxis = lastBoneLeftAxis;
		}
	}

	public LimbOrientation leftArm;

	public LimbOrientation rightArm;

	public LimbOrientation leftLeg;

	public LimbOrientation rightLeg;

	public static BipedLimbOrientations UMA
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			return new BipedLimbOrientations(new LimbOrientation(Vector3.forward, Vector3.forward, Vector3.forward), new LimbOrientation(Vector3.forward, Vector3.forward, Vector3.back), new LimbOrientation(Vector3.forward, Vector3.forward, Vector3.down), new LimbOrientation(Vector3.forward, Vector3.forward, Vector3.down));
		}
	}

	public static BipedLimbOrientations MaxBiped
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			return new BipedLimbOrientations(new LimbOrientation(Vector3.down, Vector3.down, Vector3.down), new LimbOrientation(Vector3.down, Vector3.down, Vector3.up), new LimbOrientation(Vector3.up, Vector3.up, Vector3.back), new LimbOrientation(Vector3.up, Vector3.up, Vector3.back));
		}
	}

	public BipedLimbOrientations(LimbOrientation leftArm, LimbOrientation rightArm, LimbOrientation leftLeg, LimbOrientation rightLeg)
	{
		this.leftArm = leftArm;
		this.rightArm = rightArm;
		this.leftLeg = leftLeg;
		this.rightLeg = rightLeg;
	}
}
