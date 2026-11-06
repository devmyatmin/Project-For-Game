using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverLookAt : IKSolver
{
	[Serializable]
	public class LookAtBone : Bone
	{
		public Vector3 baseForwardOffsetEuler;

		public Vector3 forward
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				return transform.rotation * axis;
			}
		}

		public LookAtBone()
		{
		}

		public LookAtBone(Transform transform)
		{
			base.transform = transform;
		}

		public void Initiate(Transform root)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)transform == (Object)null))
			{
				axis = Quaternion.Inverse(transform.rotation) * root.forward;
			}
		}

		public void LookAt(Vector3 direction, float weight)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			Quaternion val = Quaternion.FromToRotation(forward, direction);
			Quaternion rotation = transform.rotation;
			transform.rotation = Quaternion.Lerp(rotation, val * rotation, weight);
		}
	}

	public Transform target;

	public LookAtBone[] spine = new LookAtBone[0];

	public LookAtBone head = new LookAtBone();

	public LookAtBone[] eyes = new LookAtBone[0];

	[Range(0f, 1f)]
	public float bodyWeight = 0.5f;

	[Range(0f, 1f)]
	public float headWeight = 0.5f;

	[Range(0f, 1f)]
	public float eyesWeight = 1f;

	[Range(0f, 1f)]
	public float clampWeight = 0.5f;

	[Range(0f, 1f)]
	public float clampWeightHead = 0.5f;

	[Range(0f, 1f)]
	public float clampWeightEyes = 0.5f;

	[Range(0f, 2f)]
	public int clampSmoothing = 2;

	public AnimationCurve spineWeightCurve = new AnimationCurve(new Keyframe[2]
	{
		new Keyframe(0f, 0.3f),
		new Keyframe(1f, 1f)
	});

	public Vector3 spineTargetOffset;

	protected Vector3[] spineForwards = new Vector3[0];

	protected Vector3[] headForwards = new Vector3[1];

	protected Vector3[] eyeForward = new Vector3[1];

	private bool isDirty;

	protected bool spineIsValid
	{
		get
		{
			if (spine == null)
			{
				return false;
			}
			if (spine.Length == 0)
			{
				return true;
			}
			for (int i = 0; i < spine.Length; i++)
			{
				if (spine[i] == null || (Object)(object)spine[i].transform == (Object)null)
				{
					return false;
				}
			}
			return true;
		}
	}

	protected bool spineIsEmpty => spine.Length == 0;

	protected bool headIsValid
	{
		get
		{
			if (head == null)
			{
				return false;
			}
			return true;
		}
	}

	protected bool headIsEmpty => (Object)(object)head.transform == (Object)null;

	protected bool eyesIsValid
	{
		get
		{
			if (eyes == null)
			{
				return false;
			}
			if (eyes.Length == 0)
			{
				return true;
			}
			for (int i = 0; i < eyes.Length; i++)
			{
				if (eyes[i] == null || (Object)(object)eyes[i].transform == (Object)null)
				{
					return false;
				}
			}
			return true;
		}
	}

	protected bool eyesIsEmpty => eyes.Length == 0;

	public void SetLookAtWeight(float weight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight, float headWeight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
		this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight, float clampWeight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
		this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
		this.clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
		clampWeightHead = this.clampWeight;
		clampWeightEyes = this.clampWeight;
	}

	public void SetLookAtWeight(float weight, float bodyWeight = 0f, float headWeight = 1f, float eyesWeight = 0.5f, float clampWeight = 0.5f, float clampWeightHead = 0.5f, float clampWeightEyes = 0.3f)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
		this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
		this.clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
		this.clampWeightHead = Mathf.Clamp(clampWeightHead, 0f, 1f);
		this.clampWeightEyes = Mathf.Clamp(clampWeightEyes, 0f, 1f);
	}

	public override void StoreDefaultLocalState()
	{
		for (int i = 0; i < spine.Length; i++)
		{
			spine[i].StoreDefaultLocalState();
		}
		for (int j = 0; j < eyes.Length; j++)
		{
			eyes[j].StoreDefaultLocalState();
		}
		if (head != null && (Object)(object)head.transform != (Object)null)
		{
			head.StoreDefaultLocalState();
		}
	}

	public void SetDirty()
	{
		isDirty = true;
	}

	public override void FixTransforms()
	{
		if (initiated && (!(IKPositionWeight <= 0f) || isDirty))
		{
			for (int i = 0; i < spine.Length; i++)
			{
				spine[i].FixTransform();
			}
			for (int j = 0; j < eyes.Length; j++)
			{
				eyes[j].FixTransform();
			}
			if (head != null && (Object)(object)head.transform != (Object)null)
			{
				head.FixTransform();
			}
			isDirty = false;
		}
	}

	public override bool IsValid(ref string message)
	{
		if (!spineIsValid)
		{
			message = "IKSolverLookAt spine setup is invalid. Can't initiate solver.";
			return false;
		}
		if (!headIsValid)
		{
			message = "IKSolverLookAt head transform is null. Can't initiate solver.";
			return false;
		}
		if (!eyesIsValid)
		{
			message = "IKSolverLookAt eyes setup is invalid. Can't initiate solver.";
			return false;
		}
		if (spineIsEmpty && headIsEmpty && eyesIsEmpty)
		{
			message = "IKSolverLookAt eyes setup is invalid. Can't initiate solver.";
			return false;
		}
		Bone[] bones = spine;
		Transform val = IKSolver.ContainsDuplicateBone(bones);
		if ((Object)(object)val != (Object)null)
		{
			message = ((Object)val).name + " is represented multiple times in a single IK chain. Can't initiate solver.";
			return false;
		}
		bones = eyes;
		Transform val2 = IKSolver.ContainsDuplicateBone(bones);
		if ((Object)(object)val2 != (Object)null)
		{
			message = ((Object)val2).name + " is represented multiple times in a single IK chain. Can't initiate solver.";
			return false;
		}
		return true;
	}

	public override Point[] GetPoints()
	{
		Point[] array = new Point[spine.Length + eyes.Length + (((Object)(object)head.transform != (Object)null) ? 1 : 0)];
		for (int i = 0; i < spine.Length; i++)
		{
			array[i] = spine[i];
		}
		int num = 0;
		for (int j = spine.Length; j < spine.Length + eyes.Length; j++)
		{
			array[j] = eyes[num];
			num++;
		}
		if ((Object)(object)head.transform != (Object)null)
		{
			array[^1] = head;
		}
		return array;
	}

	public override Point GetPoint(Transform transform)
	{
		LookAtBone[] array = spine;
		foreach (LookAtBone lookAtBone in array)
		{
			if ((Object)(object)lookAtBone.transform == (Object)(object)transform)
			{
				return lookAtBone;
			}
		}
		array = eyes;
		foreach (LookAtBone lookAtBone2 in array)
		{
			if ((Object)(object)lookAtBone2.transform == (Object)(object)transform)
			{
				return lookAtBone2;
			}
		}
		if ((Object)(object)head.transform == (Object)(object)transform)
		{
			return head;
		}
		return null;
	}

	public bool SetChain(Transform[] spine, Transform head, Transform[] eyes, Transform root)
	{
		SetBones(spine, ref this.spine);
		this.head = new LookAtBone(head);
		SetBones(eyes, ref this.eyes);
		Initiate(root);
		return initiated;
	}

	protected override void OnInitiate()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (firstInitiation || !Application.isPlaying)
		{
			if (spine.Length != 0)
			{
				IKPosition = spine[spine.Length - 1].transform.position + root.forward * 3f;
			}
			else if ((Object)(object)head.transform != (Object)null)
			{
				IKPosition = head.transform.position + root.forward * 3f;
			}
			else if (eyes.Length != 0 && (Object)(object)eyes[0].transform != (Object)null)
			{
				IKPosition = eyes[0].transform.position + root.forward * 3f;
			}
		}
		LookAtBone[] array = spine;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Initiate(root);
		}
		if (head != null)
		{
			head.Initiate(root);
		}
		array = eyes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Initiate(root);
		}
		if (spineForwards == null || spineForwards.Length != spine.Length)
		{
			spineForwards = new Vector3[spine.Length];
		}
		if (headForwards == null)
		{
			headForwards = new Vector3[1];
		}
		if (eyeForward == null)
		{
			eyeForward = new Vector3[1];
		}
	}

	protected override void OnUpdate()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!(IKPositionWeight <= 0f))
		{
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			if ((Object)(object)target != (Object)null)
			{
				IKPosition = target.position;
			}
			SolveSpine();
			SolveHead();
			SolveEyes();
		}
	}

	protected void SolveSpine()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (!(bodyWeight <= 0f) && !spineIsEmpty)
		{
			Vector3 val = IKPosition + spineTargetOffset - spine[spine.Length - 1].transform.position;
			Vector3 normalized = val.normalized;
			GetForwards(ref spineForwards, spine[0].forward, normalized, spine.Length, clampWeight);
			for (int i = 0; i < spine.Length; i++)
			{
				spine[i].LookAt(spineForwards[i], bodyWeight * IKPositionWeight);
			}
		}
	}

	protected void SolveHead()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!(headWeight <= 0f) && !headIsEmpty)
		{
			Vector3 val = ((spine.Length != 0 && (Object)(object)spine[spine.Length - 1].transform != (Object)null) ? spine[spine.Length - 1].forward : head.forward);
			Vector3 val2 = IKPosition - head.transform.position;
			val2 = Vector3.Lerp(val, val2.normalized, headWeight * IKPositionWeight);
			Vector3 normalized = val2.normalized;
			GetForwards(ref headForwards, val, normalized, 1, clampWeightHead);
			head.LookAt(headForwards[0], headWeight * IKPositionWeight);
		}
	}

	protected void SolveEyes()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		if (eyesWeight <= 0f || eyesIsEmpty)
		{
			return;
		}
		for (int i = 0; i < eyes.Length; i++)
		{
			Quaternion val;
			if ((Object)(object)head.transform != (Object)null)
			{
				val = head.transform.rotation;
			}
			else
			{
				val = ((spine.Length != 0) ? spine[spine.Length - 1].transform.rotation : root.rotation);
			}
			Vector3 val2;
			if ((Object)(object)head.transform != (Object)null)
			{
				val2 = head.axis;
			}
			else
			{
				val2 = ((spine.Length != 0) ? spine[spine.Length - 1].axis : root.forward);
			}
			if (eyes[i].baseForwardOffsetEuler != Vector3.zero)
			{
				val *= Quaternion.Euler(eyes[i].baseForwardOffsetEuler);
			}
			Vector3 baseForward = val * val2;
			ref Vector3[] forwards = ref eyeForward;
			Vector3 val3 = IKPosition - eyes[i].transform.position;
			GetForwards(ref forwards, baseForward, val3.normalized, 1, clampWeightEyes);
			eyes[i].LookAt(eyeForward[0], eyesWeight * IKPositionWeight);
		}
	}

	protected Vector3[] GetForwards(ref Vector3[] forwards, Vector3 baseForward, Vector3 targetForward, int bones, float clamp)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		if (clamp >= 1f || IKPositionWeight <= 0f)
		{
			for (int i = 0; i < forwards.Length; i++)
			{
				forwards[i] = baseForward;
			}
			return forwards;
		}
		float num = Vector3.Angle(baseForward, targetForward);
		float num2 = 1f - num / 180f;
		float num3 = ((clamp > 0f) ? Mathf.Clamp(1f - (clamp - num2) / (1f - num2), 0f, 1f) : 1f);
		float num4 = ((clamp > 0f) ? Mathf.Clamp(num2 / clamp, 0f, 1f) : 1f);
		for (int j = 0; j < clampSmoothing; j++)
		{
			num4 = Mathf.Sin(num4 * (float)Math.PI * 0.5f);
		}
		if (forwards.Length == 1)
		{
			forwards[0] = Vector3.Slerp(baseForward, targetForward, num4 * num3);
		}
		else
		{
			float num5 = 1f / (float)(forwards.Length - 1);
			for (int k = 0; k < forwards.Length; k++)
			{
				forwards[k] = Vector3.Slerp(baseForward, targetForward, spineWeightCurve.Evaluate(num5 * (float)k) * num4 * num3);
			}
		}
		return forwards;
	}

	protected void SetBones(Transform[] array, ref LookAtBone[] bones)
	{
		if (array == null)
		{
			bones = new LookAtBone[0];
			return;
		}
		if (bones.Length != array.Length)
		{
			bones = new LookAtBone[array.Length];
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (bones[i] == null)
			{
				bones[i] = new LookAtBone(array[i]);
			}
			else
			{
				bones[i].transform = array[i];
			}
		}
	}

	public IKSolverLookAt()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected Obj, but got Unknown
	}
}
