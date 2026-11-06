using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class FBBIKHeadEffector : MonoBehaviour
{
	[Serializable]
	public class BendBone
	{
		[Tooltip("Assign spine and/or neck bones.")]
		public Transform transform;

		[Tooltip("The weight of rotating this bone.")]
		[Range(0f, 1f)]
		public float weight = 0.5f;

		private Quaternion defaultLocalRotation = Quaternion.identity;

		public BendBone()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		}

		public BendBone(Transform transform, float weight)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			this.transform = transform;
			this.weight = weight;
		}

		public void StoreDefaultLocalState()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			defaultLocalRotation = transform.localRotation;
		}

		public void FixTransforms()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			transform.localRotation = defaultLocalRotation;
		}
	}

	[Tooltip("Reference to the FBBIK component.")]
	public FullBodyBipedIK ik;

	[LargeHeader("Position")]
	[Tooltip("Master weight for positioning the head.")]
	[Range(0f, 1f)]
	public float positionWeight = 1f;

	[Tooltip("The weight of moving the body along with the head")]
	[Range(0f, 1f)]
	public float bodyWeight = 0.8f;

	[Tooltip("The weight of moving the thighs along with the head")]
	[Range(0f, 1f)]
	public float thighWeight = 0.8f;

	[Tooltip("If false, hands will not pull the head away if they are too far. Disabling this will improve performance significantly.")]
	public bool handsPullBody = true;

	[LargeHeader("Rotation")]
	[Tooltip("The weight of rotating the head bone after solving")]
	[Range(0f, 1f)]
	public float rotationWeight;

	[Tooltip("Clamping the rotation of the body")]
	[Range(0f, 1f)]
	public float bodyClampWeight = 0.5f;

	[Tooltip("Clamping the rotation of the head")]
	[Range(0f, 1f)]
	public float headClampWeight = 0.5f;

	[Tooltip("The master weight of bending/twisting the spine to the rotation of the head effector. This is similar to CCD, but uses the rotation of the head effector not the position.")]
	[Range(0f, 1f)]
	public float bendWeight = 1f;

	[Tooltip("The bones to use for bending.")]
	public BendBone[] bendBones = new BendBone[0];

	[LargeHeader("CCD")]
	[Tooltip("Optional. The master weight of the CCD (Cyclic Coordinate Descent) IK effect that bends the spine towards the head effector before FBBIK solves.")]
	[Range(0f, 1f)]
	public float CCDWeight = 1f;

	[Tooltip("The weight of rolling the bones in towards the target")]
	[Range(0f, 1f)]
	public float roll;

	[Tooltip("Smoothing the CCD effect.")]
	[Range(0f, 1000f)]
	public float damper = 500f;

	[Tooltip("Bones to use for the CCD pass. Assign spine and/or neck bones.")]
	public Transform[] CCDBones = new Transform[0];

	[LargeHeader("Stretching")]
	[Tooltip("Stretching the spine/neck to help reach the target. This is useful for making sure the head stays locked relative to the VR headset. NB! Stretching is done after FBBIK has solved so if you have the hand effectors pinned and spine bones included in the 'Stretch Bones', the hands might become offset from their target positions.")]
	[Range(0f, 1f)]
	public float postStretchWeight = 1f;

	[Tooltip("Stretch magnitude limit.")]
	public float maxStretch = 0.1f;

	[Tooltip("If > 0, dampers the stretching effect.")]
	public float stretchDamper;

	[Tooltip("If true, will fix head position to this Transform no matter what. Good for making sure the head will not budge away from the VR headset")]
	public bool fixHead;

	[Tooltip("Bones to use for stretching. The more bones you add, the less noticable the effect.")]
	public Transform[] stretchBones = new Transform[0];

	[LargeHeader("Chest Direction")]
	public Vector3 chestDirection = Vector3.forward;

	[Range(0f, 1f)]
	public float chestDirectionWeight = 1f;

	public Transform[] chestBones = new Transform[0];

	public IKSolver.UpdateDelegate OnPostHeadEffectorFK;

	private Vector3 offset;

	private Vector3 headToBody;

	private Vector3 shoulderCenterToHead;

	private Vector3 headToLeftThigh;

	private Vector3 headToRightThigh;

	private Vector3 leftShoulderPos;

	private Vector3 rightShoulderPos;

	private float shoulderDist;

	private float leftShoulderDist;

	private float rightShoulderDist;

	private Quaternion chestRotation;

	private Quaternion headRotationRelativeToRoot;

	private Quaternion[] ccdDefaultLocalRotations = new Quaternion[0];

	private Vector3 headLocalPosition;

	private Quaternion headLocalRotation;

	private Vector3[] stretchLocalPositions = new Vector3[0];

	private Quaternion[] stretchLocalRotations = new Quaternion[0];

	private Vector3[] chestLocalPositions = new Vector3[0];

	private Quaternion[] chestLocalRotations = new Quaternion[0];

	private int bendBonesCount;

	private int ccdBonesCount;

	private int stretchBonesCount;

	private int chestBonesCount;

	private void Start()
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		IKSolverFullBodyBiped solver = ik.solver;
		solver.OnPreRead = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreRead, new IKSolver.UpdateDelegate(OnPreRead));
		IKSolverFullBodyBiped solver2 = ik.solver;
		solver2.OnPreIteration = (IKSolver.IterationDelegate)Delegate.Combine(solver2.OnPreIteration, new IKSolver.IterationDelegate(Iterate));
		IKSolverFullBodyBiped solver3 = ik.solver;
		solver3.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver3.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostUpdate));
		IKSolverFullBodyBiped solver4 = ik.solver;
		solver4.OnStoreDefaultLocalState = (IKSolver.UpdateDelegate)Delegate.Combine(solver4.OnStoreDefaultLocalState, new IKSolver.UpdateDelegate(OnStoreDefaultLocalState));
		IKSolverFullBodyBiped solver5 = ik.solver;
		solver5.OnFixTransforms = (IKSolver.UpdateDelegate)Delegate.Combine(solver5.OnFixTransforms, new IKSolver.UpdateDelegate(OnFixTransforms));
		OnStoreDefaultLocalState();
		headRotationRelativeToRoot = Quaternion.Inverse(ik.references.root.rotation) * ik.references.head.rotation;
	}

	private void OnStoreDefaultLocalState()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		BendBone[] array = bendBones;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.StoreDefaultLocalState();
		}
		ccdDefaultLocalRotations = new Quaternion[CCDBones.Length];
		for (int j = 0; j < CCDBones.Length; j++)
		{
			if ((Object)(object)CCDBones[j] != (Object)null)
			{
				ccdDefaultLocalRotations[j] = CCDBones[j].localRotation;
			}
		}
		headLocalPosition = ik.references.head.localPosition;
		headLocalRotation = ik.references.head.localRotation;
		stretchLocalPositions = new Vector3[stretchBones.Length];
		stretchLocalRotations = new Quaternion[stretchBones.Length];
		for (int k = 0; k < stretchBones.Length; k++)
		{
			if ((Object)(object)stretchBones[k] != (Object)null)
			{
				stretchLocalPositions[k] = stretchBones[k].localPosition;
				stretchLocalRotations[k] = stretchBones[k].localRotation;
			}
		}
		chestLocalPositions = new Vector3[chestBones.Length];
		chestLocalRotations = new Quaternion[chestBones.Length];
		for (int l = 0; l < chestBones.Length; l++)
		{
			if ((Object)(object)chestBones[l] != (Object)null)
			{
				chestLocalPositions[l] = chestBones[l].localPosition;
				chestLocalRotations[l] = chestBones[l].localRotation;
			}
		}
		bendBonesCount = bendBones.Length;
		ccdBonesCount = CCDBones.Length;
		stretchBonesCount = stretchBones.Length;
		chestBonesCount = chestBones.Length;
	}

	private void OnFixTransforms()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		BendBone[] array = bendBones;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.FixTransforms();
		}
		for (int j = 0; j < CCDBones.Length; j++)
		{
			if ((Object)(object)CCDBones[j] != (Object)null)
			{
				CCDBones[j].localRotation = ccdDefaultLocalRotations[j];
			}
		}
		ik.references.head.localPosition = headLocalPosition;
		ik.references.head.localRotation = headLocalRotation;
		for (int k = 0; k < stretchBones.Length; k++)
		{
			if ((Object)(object)stretchBones[k] != (Object)null)
			{
				stretchBones[k].localPosition = stretchLocalPositions[k];
				stretchBones[k].localRotation = stretchLocalRotations[k];
			}
		}
		for (int l = 0; l < chestBones.Length; l++)
		{
			if ((Object)(object)chestBones[l] != (Object)null)
			{
				chestBones[l].localPosition = chestLocalPositions[l];
				chestBones[l].localRotation = chestLocalRotations[l];
			}
		}
	}

	private void OnPreRead()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && ((Component)this).gameObject.activeInHierarchy && ik.solver.iterations != 0)
		{
			ik.solver.FABRIKPass = handsPullBody;
			if (bendBonesCount != bendBones.Length || ccdBonesCount != CCDBones.Length || stretchBonesCount != stretchBones.Length || chestBonesCount != chestBones.Length)
			{
				OnStoreDefaultLocalState();
			}
			ChestDirection();
			SpineBend();
			CCDPass();
			offset = ((Component)this).transform.position - ik.references.head.position;
			shoulderDist = Vector3.Distance(ik.references.leftUpperArm.position, ik.references.rightUpperArm.position);
			leftShoulderDist = Vector3.Distance(ik.references.head.position, ik.references.leftUpperArm.position);
			rightShoulderDist = Vector3.Distance(ik.references.head.position, ik.references.rightUpperArm.position);
			headToBody = ik.solver.rootNode.position - ik.references.head.position;
			headToLeftThigh = ik.references.leftThigh.position - ik.references.head.position;
			headToRightThigh = ik.references.rightThigh.position - ik.references.head.position;
			leftShoulderPos = ik.references.leftUpperArm.position + offset * bodyWeight;
			rightShoulderPos = ik.references.rightUpperArm.position + offset * bodyWeight;
			chestRotation = Quaternion.LookRotation(ik.references.head.position - ik.references.leftUpperArm.position, ik.references.rightUpperArm.position - ik.references.leftUpperArm.position);
			if (OnPostHeadEffectorFK != null)
			{
				OnPostHeadEffectorFK();
			}
		}
	}

	private void SpineBend()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		float num = bendWeight * ik.solver.IKPositionWeight;
		if (num <= 0f || bendBones.Length == 0)
		{
			return;
		}
		Quaternion rotation = ((Component)this).transform.rotation * Quaternion.Inverse(ik.references.root.rotation * headRotationRelativeToRoot);
		rotation = QuaTools.ClampRotation(rotation, bodyClampWeight, 2);
		float num2 = 1f / (float)bendBones.Length;
		for (int i = 0; i < bendBones.Length; i++)
		{
			if ((Object)(object)bendBones[i].transform != (Object)null)
			{
				bendBones[i].transform.rotation = Quaternion.Lerp(Quaternion.identity, rotation, num2 * bendBones[i].weight * num) * bendBones[i].transform.rotation;
			}
		}
	}

	private void CCDPass()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		float num = CCDWeight * ik.solver.IKPositionWeight;
		if (!(num <= 0f))
		{
			for (int num2 = CCDBones.Length - 1; num2 > -1; num2--)
			{
				Quaternion val = Quaternion.FromToRotation(ik.references.head.position - CCDBones[num2].position, ((Component)this).transform.position - CCDBones[num2].position) * CCDBones[num2].rotation;
				float num3 = Mathf.Lerp((float)((CCDBones.Length - num2) / CCDBones.Length), 1f, roll);
				float num4 = Quaternion.Angle(Quaternion.identity, val);
				num4 = Mathf.Lerp(0f, num4, (damper - num4) / damper);
				CCDBones[num2].rotation = Quaternion.RotateTowards(CCDBones[num2].rotation, val, num4 * num * num3);
			}
		}
	}

	private void Iterate(int iteration)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && ((Component)this).gameObject.activeInHierarchy && ik.solver.iterations != 0)
		{
			Vector3 position = ((Component)this).transform.position;
			Vector3 val = leftShoulderPos - ((Component)this).transform.position;
			leftShoulderPos = position + val.normalized * leftShoulderDist;
			Vector3 position2 = ((Component)this).transform.position;
			val = rightShoulderPos - ((Component)this).transform.position;
			rightShoulderPos = position2 + val.normalized * rightShoulderDist;
			Solve(ref leftShoulderPos, ref rightShoulderPos, shoulderDist);
			LerpSolverPosition(ik.solver.leftShoulderEffector, leftShoulderPos, positionWeight * ik.solver.IKPositionWeight, ik.solver.leftShoulderEffector.positionOffset);
			LerpSolverPosition(ik.solver.rightShoulderEffector, rightShoulderPos, positionWeight * ik.solver.IKPositionWeight, ik.solver.rightShoulderEffector.positionOffset);
			Quaternion to = Quaternion.LookRotation(((Component)this).transform.position - leftShoulderPos, rightShoulderPos - leftShoulderPos);
			Quaternion val2 = QuaTools.FromToRotation(chestRotation, to);
			Vector3 val3 = val2 * headToBody;
			LerpSolverPosition(ik.solver.bodyEffector, ((Component)this).transform.position + val3, positionWeight * ik.solver.IKPositionWeight, ik.solver.bodyEffector.positionOffset - ik.solver.pullBodyOffset);
			Quaternion val4 = Quaternion.Lerp(Quaternion.identity, val2, thighWeight);
			Vector3 val5 = val4 * headToLeftThigh;
			Vector3 val6 = val4 * headToRightThigh;
			LerpSolverPosition(ik.solver.leftThighEffector, ((Component)this).transform.position + val5, positionWeight * ik.solver.IKPositionWeight, ik.solver.bodyEffector.positionOffset - ik.solver.pullBodyOffset + ik.solver.leftThighEffector.positionOffset);
			LerpSolverPosition(ik.solver.rightThighEffector, ((Component)this).transform.position + val6, positionWeight * ik.solver.IKPositionWeight, ik.solver.bodyEffector.positionOffset - ik.solver.pullBodyOffset + ik.solver.rightThighEffector.positionOffset);
		}
	}

	private void OnPostUpdate()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && ((Component)this).gameObject.activeInHierarchy)
		{
			PostStretching();
			Quaternion rotation = QuaTools.FromToRotation(ik.references.head.rotation, ((Component)this).transform.rotation);
			rotation = QuaTools.ClampRotation(rotation, headClampWeight, 2);
			ik.references.head.rotation = Quaternion.Lerp(Quaternion.identity, rotation, rotationWeight * ik.solver.IKPositionWeight) * ik.references.head.rotation;
		}
	}

	private void ChestDirection()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		float num = chestDirectionWeight * ik.solver.IKPositionWeight;
		if (num <= 0f)
		{
			return;
		}
		bool changed = false;
		chestDirection = V3Tools.ClampDirection(chestDirection, ik.references.root.forward, 0.45f, 2, out changed);
		if (!(chestDirection == Vector3.zero))
		{
			Quaternion val = Quaternion.FromToRotation(ik.references.root.forward, chestDirection);
			val = Quaternion.Lerp(Quaternion.identity, val, num * (1f / (float)chestBones.Length));
			Transform[] array = chestBones;
			foreach (Transform val2 in array)
			{
				val2.rotation = val * val2.rotation;
			}
		}
	}

	private void PostStretching()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		float num = postStretchWeight * ik.solver.IKPositionWeight;
		if (num > 0f)
		{
			Vector3 val = Vector3.ClampMagnitude(((Component)this).transform.position - ik.references.head.position, maxStretch);
			val *= num;
			stretchDamper = Mathf.Max(stretchDamper, 0f);
			if (stretchDamper > 0f)
			{
				val /= (1f + val.magnitude) * (1f + stretchDamper);
			}
			for (int i = 0; i < stretchBones.Length; i++)
			{
				if ((Object)(object)stretchBones[i] != (Object)null)
				{
					Transform obj = stretchBones[i];
					obj.position += val / (float)stretchBones.Length;
				}
			}
		}
		if (fixHead && ik.solver.IKPositionWeight > 0f)
		{
			ik.references.head.position = ((Component)this).transform.position;
		}
	}

	private void LerpSolverPosition(IKEffector effector, Vector3 position, float weight, Vector3 offset)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		effector.GetNode(ik.solver).solverPosition = Vector3.Lerp(effector.GetNode(ik.solver).solverPosition, position + offset, weight);
	}

	private void Solve(ref Vector3 pos1, ref Vector3 pos2, float nominalDistance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = pos2 - pos1;
		float magnitude = val.magnitude;
		if (magnitude != nominalDistance && magnitude != 0f)
		{
			float num = 1f;
			num *= 1f - nominalDistance / magnitude;
			Vector3 val2 = val * num * 0.5f;
			pos1 += val2;
			pos2 -= val2;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPreRead = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreRead, new IKSolver.UpdateDelegate(OnPreRead));
			IKSolverFullBodyBiped solver2 = ik.solver;
			solver2.OnPreIteration = (IKSolver.IterationDelegate)Delegate.Remove(solver2.OnPreIteration, new IKSolver.IterationDelegate(Iterate));
			IKSolverFullBodyBiped solver3 = ik.solver;
			solver3.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver3.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostUpdate));
			IKSolverFullBodyBiped solver4 = ik.solver;
			solver4.OnStoreDefaultLocalState = (IKSolver.UpdateDelegate)Delegate.Remove(solver4.OnStoreDefaultLocalState, new IKSolver.UpdateDelegate(OnStoreDefaultLocalState));
			IKSolverFullBodyBiped solver5 = ik.solver;
			solver5.OnFixTransforms = (IKSolver.UpdateDelegate)Delegate.Remove(solver5.OnFixTransforms, new IKSolver.UpdateDelegate(OnFixTransforms));
		}
	}

	public FBBIKHeadEffector()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
	}
}
