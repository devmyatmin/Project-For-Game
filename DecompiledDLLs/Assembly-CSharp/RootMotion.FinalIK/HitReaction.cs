using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RootMotion.FinalIK;

public class HitReaction : OffsetModifier
{
	[Serializable]
	public abstract class HitPoint
	{
		[Tooltip("Just for visual clarity, not used at all")]
		public string name;

		[Tooltip("Linking this hit point to a collider")]
		public Collider collider;

		[Tooltip("Only used if this hit point gets hit when already processing another hit")]
		[SerializeField]
		private float crossFadeTime = 0.1f;

		private float length;

		private float crossFadeSpeed;

		private float lastTime;

		public bool inProgress => timer < length;

		protected float crossFader { get; private set; }

		protected float timer { get; private set; }

		protected Vector3 force
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		public virtual void Hit(Vector3 force, Vector3 point)
		{
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			if (length == 0f)
			{
				length = GetLength();
			}
			if (length <= 0f)
			{
				Debug.LogError((object)"Hit Point WeightCurve length is zero.");
				return;
			}
			if (timer < 1f)
			{
				crossFader = 0f;
			}
			crossFadeSpeed = ((crossFadeTime > 0f) ? (1f / crossFadeTime) : 0f);
			CrossFadeStart();
			timer = 0f;
			this.force = force;
		}

		public void Apply(IKSolverFullBodyBiped solver, float weight)
		{
			float num = Time.time - lastTime;
			lastTime = Time.time;
			if (!(timer >= length))
			{
				timer = Mathf.Clamp(timer + num, 0f, length);
				if (crossFadeSpeed > 0f)
				{
					crossFader = Mathf.Clamp(crossFader + num * crossFadeSpeed, 0f, 1f);
				}
				else
				{
					crossFader = 1f;
				}
				OnApply(solver, weight);
			}
		}

		protected abstract float GetLength();

		protected abstract void CrossFadeStart();

		protected abstract void OnApply(IKSolverFullBodyBiped solver, float weight);
	}

	[Serializable]
	public class HitPointEffector : HitPoint
	{
		[Serializable]
		public class EffectorLink
		{
			[Tooltip("The FBBIK effector type")]
			public FullBodyBipedEffector effector;

			[Tooltip("The weight of this effector (could also be negative)")]
			public float weight;

			private Vector3 lastValue;

			private Vector3 current;

			public void Apply(IKSolverFullBodyBiped solver, Vector3 offset, float crossFader)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				current = Vector3.Lerp(lastValue, offset * weight, crossFader);
				IKEffector iKEffector = solver.GetEffector(effector);
				iKEffector.positionOffset += current;
			}

			public void CrossFadeStart()
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				lastValue = current;
			}
		}

		[Tooltip("Offset magnitude in the direction of the hit force")]
		public AnimationCurve offsetInForceDirection;

		[Tooltip("Offset magnitude in the direction of character.up")]
		public AnimationCurve offsetInUpDirection;

		[Tooltip("Linking this offset to the FBBIK effectors")]
		public EffectorLink[] effectorLinks;

		protected override float GetLength()
		{
			float num = ((offsetInForceDirection.keys.Length != 0) ? offsetInForceDirection.keys[offsetInForceDirection.length - 1].time : 0f);
			float num2 = ((offsetInUpDirection.keys.Length != 0) ? offsetInUpDirection.keys[offsetInUpDirection.length - 1].time : 0f);
			return Mathf.Clamp(num, num2, num);
		}

		protected override void CrossFadeStart()
		{
			EffectorLink[] array = effectorLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CrossFadeStart();
			}
		}

		protected override void OnApply(IKSolverFullBodyBiped solver, float weight)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			Vector3 up = solver.GetRoot().up;
			Vector3 val = force;
			Vector3 val2 = up * val.magnitude;
			Vector3 val3 = offsetInForceDirection.Evaluate(timer) * force + offsetInUpDirection.Evaluate(timer) * val2;
			val3 *= weight;
			EffectorLink[] array = effectorLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Apply(solver, val3, crossFader);
			}
		}
	}

	[Serializable]
	public class HitPointBone : HitPoint
	{
		[Serializable]
		public class BoneLink
		{
			[Tooltip("Reference to the bone that this hit point rotates")]
			public Transform bone;

			[Tooltip("Weight of rotating the bone")]
			[Range(0f, 1f)]
			public float weight;

			private Quaternion lastValue = Quaternion.identity;

			private Quaternion current = Quaternion.identity;

			public void Apply(IKSolverFullBodyBiped solver, Quaternion offset, float crossFader)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				current = Quaternion.Lerp(lastValue, Quaternion.Lerp(Quaternion.identity, offset, weight), crossFader);
				bone.rotation = current * bone.rotation;
			}

			public void CrossFadeStart()
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				lastValue = current;
			}

			public BoneLink()
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			}
		}

		[Tooltip("The angle to rotate the bone around its rigidbody's world center of mass")]
		public AnimationCurve aroundCenterOfMass;

		[Tooltip("Linking this hit point to bone(s)")]
		public BoneLink[] boneLinks;

		private Rigidbody rigidbody;

		private Vector3 comAxis;

		public override void Hit(Vector3 force, Vector3 point)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			base.Hit(force, point);
			if ((Object)(object)rigidbody == (Object)null)
			{
				rigidbody = ((Component)collider).GetComponent<Rigidbody>();
			}
			Vector3 val = (((Object)(object)rigidbody != (Object)null) ? rigidbody.worldCenterOfMass : ((Component)collider).transform.position);
			comAxis = Vector3.Cross(force, point - val);
		}

		protected override float GetLength()
		{
			if (aroundCenterOfMass.keys.Length == 0)
			{
				return 0f;
			}
			return aroundCenterOfMass.keys[aroundCenterOfMass.length - 1].time;
		}

		protected override void CrossFadeStart()
		{
			BoneLink[] array = boneLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CrossFadeStart();
			}
		}

		protected override void OnApply(IKSolverFullBodyBiped solver, float weight)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			Quaternion offset = Quaternion.AngleAxis(aroundCenterOfMass.Evaluate(timer) * weight, comAxis);
			BoneLink[] array = boneLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Apply(solver, offset, crossFader);
			}
		}
	}

	[Tooltip("Hit points for the FBBIK effectors")]
	public HitPointEffector[] effectorHitPoints;

	[Tooltip(" Hit points for bones without an effector, such as the head")]
	public HitPointBone[] boneHitPoints;

	public bool inProgress
	{
		get
		{
			HitPointEffector[] array = effectorHitPoints;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].inProgress)
				{
					return true;
				}
			}
			HitPointBone[] array2 = boneHitPoints;
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i].inProgress)
				{
					return true;
				}
			}
			return false;
		}
	}

	protected override void OnModifyOffset()
	{
		HitPointEffector[] array = effectorHitPoints;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply(ik.solver, weight);
		}
		HitPointBone[] array2 = boneHitPoints;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Apply(ik.solver, weight);
		}
	}

	public void Hit(Collider collider, Vector3 force, Vector3 point)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ik == (Object)null)
		{
			Debug.LogError((object)"No IK assigned in HitReaction");
			return;
		}
		HitPointEffector[] array = effectorHitPoints;
		foreach (HitPointEffector hitPointEffector in array)
		{
			if ((Object)(object)hitPointEffector.collider == (Object)(object)collider)
			{
				hitPointEffector.Hit(force, point);
			}
		}
		HitPointBone[] array2 = boneHitPoints;
		foreach (HitPointBone hitPointBone in array2)
		{
			if ((Object)(object)hitPointBone.collider == (Object)(object)collider)
			{
				hitPointBone.Hit(force, point);
			}
		}
	}
}
