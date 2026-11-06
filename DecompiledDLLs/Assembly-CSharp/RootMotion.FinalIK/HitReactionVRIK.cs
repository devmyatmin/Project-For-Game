using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RootMotion.FinalIK;

public class HitReactionVRIK : OffsetModifierVRIK
{
	[Serializable]
	public abstract class Offset
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

		public virtual void Hit(Vector3 force, AnimationCurve[] curves, Vector3 point)
		{
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			if (length == 0f)
			{
				length = GetLength(curves);
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

		public void Apply(VRIK ik, AnimationCurve[] curves, float weight)
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
				OnApply(ik, curves, weight);
			}
		}

		protected abstract float GetLength(AnimationCurve[] curves);

		protected abstract void CrossFadeStart();

		protected abstract void OnApply(VRIK ik, AnimationCurve[] curves, float weight);
	}

	[Serializable]
	public class PositionOffset : Offset
	{
		[Serializable]
		public class PositionOffsetLink
		{
			[Tooltip("The FBBIK effector type")]
			public IKSolverVR.PositionOffset positionOffset;

			[Tooltip("The weight of this effector (could also be negative)")]
			public float weight;

			private Vector3 lastValue;

			private Vector3 current;

			public void Apply(VRIK ik, Vector3 offset, float crossFader)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				current = Vector3.Lerp(lastValue, offset * weight, crossFader);
				ik.solver.AddPositionOffset(positionOffset, current);
			}

			public void CrossFadeStart()
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				lastValue = current;
			}
		}

		[Tooltip("Offset magnitude in the direction of the hit force")]
		public int forceDirCurveIndex;

		[Tooltip("Offset magnitude in the direction of character.up")]
		public int upDirCurveIndex = 1;

		[Tooltip("Linking this offset to the VRIK position offsets")]
		public PositionOffsetLink[] offsetLinks;

		protected override float GetLength(AnimationCurve[] curves)
		{
			float num = ((curves[forceDirCurveIndex].keys.Length != 0) ? curves[forceDirCurveIndex].keys[curves[forceDirCurveIndex].length - 1].time : 0f);
			float num2 = ((curves[upDirCurveIndex].keys.Length != 0) ? curves[upDirCurveIndex].keys[curves[upDirCurveIndex].length - 1].time : 0f);
			return Mathf.Clamp(num, num2, num);
		}

		protected override void CrossFadeStart()
		{
			PositionOffsetLink[] array = offsetLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CrossFadeStart();
			}
		}

		protected override void OnApply(VRIK ik, AnimationCurve[] curves, float weight)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			Vector3 up = ((Component)ik).transform.up;
			Vector3 val = force;
			Vector3 val2 = up * val.magnitude;
			Vector3 val3 = curves[forceDirCurveIndex].Evaluate(timer) * force + curves[upDirCurveIndex].Evaluate(timer) * val2;
			val3 *= weight;
			PositionOffsetLink[] array = offsetLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Apply(ik, val3, crossFader);
			}
		}
	}

	[Serializable]
	public class RotationOffset : Offset
	{
		[Serializable]
		public class RotationOffsetLink
		{
			[Tooltip("Reference to the bone that this hit point rotates")]
			public IKSolverVR.RotationOffset rotationOffset;

			[Tooltip("Weight of rotating the bone")]
			[Range(0f, 1f)]
			public float weight;

			private Quaternion lastValue = Quaternion.identity;

			private Quaternion current = Quaternion.identity;

			public void Apply(VRIK ik, Quaternion offset, float crossFader)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				current = Quaternion.Lerp(lastValue, Quaternion.Lerp(Quaternion.identity, offset, weight), crossFader);
				ik.solver.AddRotationOffset(rotationOffset, current);
			}

			public void CrossFadeStart()
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				lastValue = current;
			}

			public RotationOffsetLink()
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			}
		}

		[Tooltip("The angle to rotate the bone around its rigidbody's world center of mass")]
		public int curveIndex;

		[Tooltip("Linking this hit point to bone(s)")]
		public RotationOffsetLink[] offsetLinks;

		private Rigidbody rigidbody;

		private Vector3 comAxis;

		public override void Hit(Vector3 force, AnimationCurve[] curves, Vector3 point)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			base.Hit(force, curves, point);
			if ((Object)(object)rigidbody == (Object)null)
			{
				rigidbody = ((Component)collider).GetComponent<Rigidbody>();
			}
			Vector3 val = (((Object)(object)rigidbody != (Object)null) ? rigidbody.worldCenterOfMass : ((Component)collider).transform.position);
			comAxis = Vector3.Cross(force, point - val);
		}

		protected override float GetLength(AnimationCurve[] curves)
		{
			if (curves[curveIndex].keys.Length == 0)
			{
				return 0f;
			}
			return curves[curveIndex].keys[curves[curveIndex].length - 1].time;
		}

		protected override void CrossFadeStart()
		{
			RotationOffsetLink[] array = offsetLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CrossFadeStart();
			}
		}

		protected override void OnApply(VRIK ik, AnimationCurve[] curves, float weight)
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)collider == (Object)null)
			{
				Debug.LogError((object)"No collider assigned for a HitPointBone in the HitReaction component.");
				return;
			}
			if ((Object)(object)rigidbody == (Object)null)
			{
				rigidbody = ((Component)collider).GetComponent<Rigidbody>();
			}
			if ((Object)(object)rigidbody != (Object)null)
			{
				Quaternion offset = Quaternion.AngleAxis(curves[curveIndex].Evaluate(timer) * weight, comAxis);
				RotationOffsetLink[] array = offsetLinks;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Apply(ik, offset, crossFader);
				}
			}
		}
	}

	public AnimationCurve[] offsetCurves;

	[Tooltip("Hit points for the FBBIK effectors")]
	public PositionOffset[] positionOffsets;

	[Tooltip(" Hit points for bones without an effector, such as the head")]
	public RotationOffset[] rotationOffsets;

	protected override void OnModifyOffset()
	{
		PositionOffset[] array = positionOffsets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply(ik, offsetCurves, weight);
		}
		RotationOffset[] array2 = rotationOffsets;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Apply(ik, offsetCurves, weight);
		}
	}

	public void Hit(Collider collider, Vector3 force, Vector3 point)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ik == (Object)null)
		{
			Debug.LogError((object)"No IK assigned in HitReaction");
			return;
		}
		PositionOffset[] array = positionOffsets;
		foreach (PositionOffset positionOffset in array)
		{
			if ((Object)(object)positionOffset.collider == (Object)(object)collider)
			{
				positionOffset.Hit(force, offsetCurves, point);
			}
		}
		RotationOffset[] array2 = rotationOffsets;
		foreach (RotationOffset rotationOffset in array2)
		{
			if ((Object)(object)rotationOffset.collider == (Object)(object)collider)
			{
				rotationOffset.Hit(force, offsetCurves, point);
			}
		}
	}
}
