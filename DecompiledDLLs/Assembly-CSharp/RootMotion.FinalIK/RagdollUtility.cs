using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK;

public class RagdollUtility : MonoBehaviour
{
	public class Rigidbone
	{
		public Rigidbody r;

		public Transform t;

		public Collider collider;

		public Joint joint;

		public Rigidbody c;

		public bool updateAnchor;

		public Vector3 deltaPosition;

		public Quaternion deltaRotation;

		public float deltaTime;

		public Vector3 lastPosition;

		public Quaternion lastRotation;

		public Rigidbone(Rigidbody r)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			this.r = r;
			t = ((Component)r).transform;
			joint = ((Component)t).GetComponent<Joint>();
			collider = ((Component)t).GetComponent<Collider>();
			if ((Object)(object)joint != (Object)null)
			{
				c = joint.connectedBody;
				updateAnchor = (Object)(object)c != (Object)null;
			}
			lastPosition = t.position;
			lastRotation = t.rotation;
		}

		public void RecordVelocity()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			deltaPosition = t.position - lastPosition;
			lastPosition = t.position;
			deltaRotation = QuaTools.FromToRotation(lastRotation, t.rotation);
			lastRotation = t.rotation;
			deltaTime = Time.deltaTime;
		}

		public void WakeUp(float velocityWeight, float angularVelocityWeight)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			if (updateAnchor)
			{
				joint.connectedAnchor = t.InverseTransformPoint(c.position);
			}
			r.isKinematic = false;
			if (velocityWeight != 0f)
			{
				r.velocity = deltaPosition / deltaTime * velocityWeight;
			}
			if (angularVelocityWeight != 0f)
			{
				float num = 0f;
				Vector3 zero = Vector3.zero;
				deltaRotation.ToAngleAxis(ref num, ref zero);
				num *= (float)Math.PI / 180f;
				num /= deltaTime;
				zero *= num * angularVelocityWeight;
				r.angularVelocity = Vector3.ClampMagnitude(zero, r.maxAngularVelocity);
			}
			r.WakeUp();
		}
	}

	public class Child
	{
		public Transform t;

		public Vector3 localPosition;

		public Quaternion localRotation;

		public Child(Transform transform)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			t = transform;
			localPosition = t.localPosition;
			localRotation = t.localRotation;
		}

		public void FixTransform(float weight)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (!(weight <= 0f))
			{
				if (weight >= 1f)
				{
					t.localPosition = localPosition;
					t.localRotation = localRotation;
				}
				else
				{
					t.localPosition = Vector3.Lerp(t.localPosition, localPosition, weight);
					t.localRotation = Quaternion.Lerp(t.localRotation, localRotation, weight);
				}
			}
		}

		public void StoreLocalState()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			localPosition = t.localPosition;
			localRotation = t.localRotation;
		}
	}

	[Tooltip("If you have multiple IK components, then this should be the one that solves last each frame.")]
	public IK ik;

	[Tooltip("How long does it take to blend from ragdoll to animation?")]
	public float ragdollToAnimationTime = 0.2f;

	[Tooltip("If true, IK can be used on top of physical ragdoll simulation.")]
	public bool applyIkOnRagdoll;

	[Tooltip("How much velocity transfer from animation to ragdoll?")]
	public float applyVelocity = 1f;

	[Tooltip("How much angular velocity to transfer from animation to ragdoll?")]
	public float applyAngularVelocity = 1f;

	private Animator animator;

	private Rigidbone[] rigidbones = new Rigidbone[0];

	private Child[] children = new Child[0];

	private bool enableRagdollFlag;

	private AnimatorUpdateMode animatorUpdateMode;

	private IK[] allIKComponents = new IK[0];

	private bool[] fixTransforms = new bool[0];

	private float ragdollWeight;

	private float ragdollWeightV;

	private bool fixedFrame;

	private bool[] disabledIKComponents = new bool[0];

	private bool isRagdoll
	{
		get
		{
			if (!rigidbones[0].r.isKinematic)
			{
				return !((Behaviour)animator).enabled;
			}
			return false;
		}
	}

	private bool ikUsed
	{
		get
		{
			if ((Object)(object)ik == (Object)null)
			{
				return false;
			}
			if (((Behaviour)ik).enabled && ik.GetIKSolver().IKPositionWeight > 0f)
			{
				return true;
			}
			IK[] array = allIKComponents;
			foreach (IK iK in array)
			{
				if (((Behaviour)iK).enabled && iK.GetIKSolver().IKPositionWeight > 0f)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void EnableRagdoll()
	{
		if (!isRagdoll)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			enableRagdollFlag = true;
		}
	}

	public void DisableRagdoll()
	{
		if (isRagdoll)
		{
			StoreLocalState();
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(DisableRagdollSmooth());
		}
	}

	public void Start()
	{
		animator = ((Component)this).GetComponent<Animator>();
		allIKComponents = ((Component)this).GetComponentsInChildren<IK>();
		disabledIKComponents = new bool[allIKComponents.Length];
		fixTransforms = new bool[allIKComponents.Length];
		if ((Object)(object)ik != (Object)null)
		{
			IKSolver iKSolver = ik.GetIKSolver();
			iKSolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterLastIK));
		}
		Rigidbody[] componentsInChildren = ((Component)this).GetComponentsInChildren<Rigidbody>();
		int num = (((Object)(object)((Component)componentsInChildren[0]).gameObject == (Object)(object)((Component)this).gameObject) ? 1 : 0);
		rigidbones = new Rigidbone[(num == 0) ? componentsInChildren.Length : (componentsInChildren.Length - 1)];
		for (int i = 0; i < rigidbones.Length; i++)
		{
			rigidbones[i] = new Rigidbone(componentsInChildren[i + num]);
		}
		Transform[] componentsInChildren2 = ((Component)this).GetComponentsInChildren<Transform>();
		children = new Child[componentsInChildren2.Length - 1];
		for (int j = 0; j < children.Length; j++)
		{
			children[j] = new Child(componentsInChildren2[j + 1]);
		}
	}

	private IEnumerator DisableRagdollSmooth()
	{
		for (int i = 0; i < rigidbones.Length; i++)
		{
			rigidbones[i].r.isKinematic = true;
		}
		for (int j = 0; j < allIKComponents.Length; j++)
		{
			allIKComponents[j].fixTransforms = fixTransforms[j];
			if (disabledIKComponents[j])
			{
				((Behaviour)allIKComponents[j]).enabled = true;
			}
		}
		animator.updateMode = animatorUpdateMode;
		((Behaviour)animator).enabled = true;
		while (ragdollWeight > 0f)
		{
			ragdollWeight = Mathf.SmoothDamp(ragdollWeight, 0f, ref ragdollWeightV, ragdollToAnimationTime);
			if (ragdollWeight < 0.001f)
			{
				ragdollWeight = 0f;
			}
			yield return null;
		}
		yield return null;
	}

	private void Update()
	{
		if (!isRagdoll)
		{
			return;
		}
		if (!applyIkOnRagdoll)
		{
			bool flag = false;
			for (int i = 0; i < allIKComponents.Length; i++)
			{
				if (((Behaviour)allIKComponents[i]).enabled)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				for (int j = 0; j < allIKComponents.Length; j++)
				{
					disabledIKComponents[j] = false;
				}
			}
			for (int k = 0; k < allIKComponents.Length; k++)
			{
				if (((Behaviour)allIKComponents[k]).enabled)
				{
					((Behaviour)allIKComponents[k]).enabled = false;
					disabledIKComponents[k] = true;
				}
			}
			return;
		}
		bool flag2 = false;
		for (int l = 0; l < allIKComponents.Length; l++)
		{
			if (disabledIKComponents[l])
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			return;
		}
		for (int m = 0; m < allIKComponents.Length; m++)
		{
			if (disabledIKComponents[m])
			{
				((Behaviour)allIKComponents[m]).enabled = true;
			}
		}
		for (int n = 0; n < allIKComponents.Length; n++)
		{
			disabledIKComponents[n] = false;
		}
	}

	private void FixedUpdate()
	{
		if (isRagdoll && applyIkOnRagdoll)
		{
			FixTransforms(1f);
		}
		fixedFrame = true;
	}

	private void LateUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if ((int)animator.updateMode != 1 || ((int)animator.updateMode == 1 && fixedFrame))
		{
			AfterAnimation();
		}
		fixedFrame = false;
		if (!ikUsed)
		{
			OnFinalPose();
		}
	}

	private void AfterLastIK()
	{
		if (ikUsed)
		{
			OnFinalPose();
		}
	}

	private void AfterAnimation()
	{
		if (isRagdoll)
		{
			StoreLocalState();
		}
		else
		{
			FixTransforms(ragdollWeight);
		}
	}

	private void OnFinalPose()
	{
		if (!isRagdoll)
		{
			RecordVelocities();
		}
		if (enableRagdollFlag)
		{
			RagdollEnabler();
		}
	}

	private void RagdollEnabler()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		StoreLocalState();
		for (int i = 0; i < allIKComponents.Length; i++)
		{
			disabledIKComponents[i] = false;
		}
		if (!applyIkOnRagdoll)
		{
			for (int j = 0; j < allIKComponents.Length; j++)
			{
				if (((Behaviour)allIKComponents[j]).enabled)
				{
					((Behaviour)allIKComponents[j]).enabled = false;
					disabledIKComponents[j] = true;
				}
			}
		}
		animatorUpdateMode = animator.updateMode;
		animator.updateMode = (AnimatorUpdateMode)1;
		((Behaviour)animator).enabled = false;
		for (int k = 0; k < rigidbones.Length; k++)
		{
			rigidbones[k].WakeUp(applyVelocity, applyAngularVelocity);
		}
		for (int l = 0; l < fixTransforms.Length; l++)
		{
			fixTransforms[l] = allIKComponents[l].fixTransforms;
			allIKComponents[l].fixTransforms = false;
		}
		ragdollWeight = 1f;
		ragdollWeightV = 0f;
		enableRagdollFlag = false;
	}

	private void RecordVelocities()
	{
		Rigidbone[] array = rigidbones;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RecordVelocity();
		}
	}

	private void StoreLocalState()
	{
		Child[] array = children;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].StoreLocalState();
		}
	}

	private void FixTransforms(float weight)
	{
		Child[] array = children;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].FixTransform(weight);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolver iKSolver = ik.GetIKSolver();
			iKSolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterLastIK));
		}
	}
}
