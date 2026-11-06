using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class Recoil : OffsetModifier
{
	[Serializable]
	public class RecoilOffset
	{
		[Serializable]
		public class EffectorLink
		{
			[Tooltip("Type of the FBBIK effector to use")]
			public FullBodyBipedEffector effector;

			[Tooltip("Weight of using this effector")]
			public float weight;
		}

		[Tooltip("Offset vector for the associated effector when doing recoil.")]
		public Vector3 offset;

		[Tooltip("When firing before the last recoil has faded, how much of the current recoil offset will be maintained?")]
		[Range(0f, 1f)]
		public float additivity = 1f;

		[Tooltip("Max additive recoil for automatic fire.")]
		public float maxAdditiveOffsetMag = 0.2f;

		[Tooltip("Linking this recoil offset to FBBIK effectors.")]
		public EffectorLink[] effectorLinks;

		private Vector3 additiveOffset;

		private Vector3 lastOffset;

		public void Start()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (!(additivity <= 0f))
			{
				additiveOffset = Vector3.ClampMagnitude(lastOffset * additivity, maxAdditiveOffsetMag);
			}
		}

		public void Apply(IKSolverFullBodyBiped solver, Quaternion rotation, float masterWeight, float length, float timeLeft)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			additiveOffset = Vector3.Lerp(Vector3.zero, additiveOffset, timeLeft / length);
			lastOffset = rotation * (offset * masterWeight) + rotation * additiveOffset;
			EffectorLink[] array = effectorLinks;
			foreach (EffectorLink effectorLink in array)
			{
				IKEffector effector = solver.GetEffector(effectorLink.effector);
				effector.positionOffset += lastOffset * effectorLink.weight;
			}
		}
	}

	[Serializable]
	public enum Handedness
	{
		Right,
		Left
	}

	[Tooltip("Reference to the AimIK component. Optional, only used to getting the aiming direction.")]
	public AimIK aimIK;

	[Tooltip("Optional head AimIK solver. This solver should only use neck and head bones and have the head as Aim Transform")]
	public AimIK headIK;

	[Tooltip("Set this true if you are using IKExecutionOrder.cs or a custom script to force AimIK solve after FBBIK.")]
	public bool aimIKSolvedLast;

	[Tooltip("Which hand is holding the weapon?")]
	public Handedness handedness;

	[Tooltip("Check for 2-handed weapons.")]
	public bool twoHanded = true;

	[Tooltip("Weight curve for the recoil offsets. Recoil procedure is as long as this curve.")]
	public AnimationCurve recoilWeight;

	[Tooltip("How much is the magnitude randomized each time Recoil is called?")]
	public float magnitudeRandom = 0.1f;

	[Tooltip("How much is the rotation randomized each time Recoil is called?")]
	public Vector3 rotationRandom;

	[Tooltip("Rotating the primary hand bone for the recoil (in local space).")]
	public Vector3 handRotationOffset;

	[Tooltip("Time of blending in another recoil when doing automatic fire.")]
	public float blendTime;

	[Space(10f)]
	[Tooltip("FBBIK effector position offsets for the recoil (in aiming direction space).")]
	public RecoilOffset[] offsets;

	[HideInInspector]
	public Quaternion rotationOffset = Quaternion.identity;

	private float magnitudeMlp = 1f;

	private float endTime = -1f;

	private Quaternion handRotation;

	private Quaternion secondaryHandRelativeRotation;

	private Quaternion randomRotation;

	private float length = 1f;

	private bool initiated;

	private float blendWeight;

	private float w;

	private Quaternion primaryHandRotation = Quaternion.identity;

	private bool handRotationsSet;

	private Vector3 aimIKAxis;

	public bool isFinished => Time.time > endTime;

	private IKEffector primaryHandEffector
	{
		get
		{
			if (handedness == Handedness.Right)
			{
				return ik.solver.rightHandEffector;
			}
			return ik.solver.leftHandEffector;
		}
	}

	private IKEffector secondaryHandEffector
	{
		get
		{
			if (handedness == Handedness.Right)
			{
				return ik.solver.leftHandEffector;
			}
			return ik.solver.rightHandEffector;
		}
	}

	private Transform primaryHand => primaryHandEffector.bone;

	private Transform secondaryHand => secondaryHandEffector.bone;

	public void SetHandRotations(Quaternion leftHandRotation, Quaternion rightHandRotation)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (handedness == Handedness.Left)
		{
			primaryHandRotation = leftHandRotation;
		}
		else
		{
			primaryHandRotation = rightHandRotation;
		}
		handRotationsSet = true;
	}

	public void Fire(float magnitude)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		float num = magnitude * Random.value * magnitudeRandom;
		magnitudeMlp = magnitude + num;
		randomRotation = Quaternion.Euler(rotationRandom * Random.value);
		RecoilOffset[] array = offsets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Start();
		}
		if (Time.time < endTime)
		{
			blendWeight = 0f;
		}
		else
		{
			blendWeight = 1f;
		}
		length = recoilWeight.keys[^1].time;
		endTime = Time.time + length;
	}

	protected override void OnModifyOffset()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)aimIK != (Object)null)
		{
			aimIKAxis = aimIK.solver.axis;
		}
		if (!initiated && (Object)(object)ik != (Object)null)
		{
			initiated = true;
			if ((Object)(object)headIK != (Object)null)
			{
				((Behaviour)headIK).enabled = false;
			}
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
			if ((Object)(object)aimIK != (Object)null)
			{
				IKSolverAim solver2 = aimIK.solver;
				solver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver2.OnPostUpdate, new IKSolver.UpdateDelegate(AfterAimIK));
			}
		}
		if (Time.time >= endTime)
		{
			rotationOffset = Quaternion.identity;
			return;
		}
		blendTime = Mathf.Max(blendTime, 0f);
		if (blendTime > 0f)
		{
			blendWeight = Mathf.Min(blendWeight + Time.deltaTime * (1f / blendTime), 1f);
		}
		else
		{
			blendWeight = 1f;
		}
		float num = recoilWeight.Evaluate(length - (endTime - Time.time)) * magnitudeMlp;
		w = Mathf.Lerp(w, num, blendWeight);
		Quaternion val = (((Object)(object)aimIK != (Object)null && (Object)(object)aimIK.solver.transform != (Object)null && !aimIKSolvedLast) ? Quaternion.LookRotation(aimIK.solver.IKPosition - aimIK.solver.transform.position, ik.references.root.up) : ik.references.root.rotation);
		val = randomRotation * val;
		RecoilOffset[] array = offsets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply(ik.solver, val, w, length, endTime - Time.time);
		}
		if (!handRotationsSet)
		{
			primaryHandRotation = primaryHand.rotation;
		}
		handRotationsSet = false;
		rotationOffset = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(randomRotation * primaryHandRotation * handRotationOffset), w);
		handRotation = rotationOffset * primaryHandRotation;
		if (twoHanded)
		{
			Vector3 val2 = Quaternion.Inverse(primaryHand.rotation) * (secondaryHand.position - primaryHand.position);
			secondaryHandRelativeRotation = Quaternion.Inverse(primaryHand.rotation) * secondaryHand.rotation;
			Vector3 val3 = primaryHand.position + primaryHandEffector.positionOffset + handRotation * val2;
			IKEffector iKEffector = secondaryHandEffector;
			iKEffector.positionOffset += val3 - (secondaryHand.position + secondaryHandEffector.positionOffset);
		}
		if ((Object)(object)aimIK != (Object)null && aimIKSolvedLast)
		{
			aimIK.solver.axis = Quaternion.Inverse(ik.references.root.rotation) * Quaternion.Inverse(rotationOffset) * aimIKAxis;
		}
	}

	private void AfterFBBIK()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time < endTime)
		{
			primaryHand.rotation = handRotation;
			if (twoHanded)
			{
				secondaryHand.rotation = primaryHand.rotation * secondaryHandRelativeRotation;
			}
		}
		if (!aimIKSolvedLast && (Object)(object)headIK != (Object)null)
		{
			headIK.solver.Update();
		}
	}

	private void AfterAimIK()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (aimIKSolvedLast)
		{
			aimIK.solver.axis = aimIKAxis;
		}
		if (aimIKSolvedLast && (Object)(object)headIK != (Object)null)
		{
			headIK.solver.Update();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((Object)(object)ik != (Object)null && initiated)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
			if ((Object)(object)aimIK != (Object)null)
			{
				IKSolverAim solver2 = aimIK.solver;
				solver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver2.OnPostUpdate, new IKSolver.UpdateDelegate(AfterAimIK));
			}
		}
	}

	public Recoil()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
	}
}
