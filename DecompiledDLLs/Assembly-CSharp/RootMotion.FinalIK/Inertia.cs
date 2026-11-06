using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class Inertia : OffsetModifier
{
	[Serializable]
	public class Body
	{
		[Serializable]
		public class EffectorLink
		{
			[Tooltip("Type of the FBBIK effector to use")]
			public FullBodyBipedEffector effector;

			[Tooltip("Weight of using this effector")]
			public float weight;
		}

		[Tooltip("The Transform to follow, can be any bone of the character")]
		public Transform transform;

		[Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector")]
		public EffectorLink[] effectorLinks;

		[Tooltip("The speed to follow the Transform")]
		public float speed = 10f;

		[Tooltip("The acceleration, smaller values means lazyer following")]
		public float acceleration = 3f;

		[Tooltip("Matching target velocity")]
		[Range(0f, 1f)]
		public float matchVelocity;

		[Tooltip("gravity applied to the Body")]
		public float gravity;

		private Vector3 delta;

		private Vector3 lazyPoint;

		private Vector3 direction;

		private Vector3 lastPosition;

		private bool firstUpdate = true;

		public void Reset()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)transform == (Object)null))
			{
				lazyPoint = transform.position;
				lastPosition = transform.position;
				direction = Vector3.zero;
			}
		}

		public void Update(IKSolverFullBodyBiped solver, float weight, float deltaTime)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)transform == (Object)null))
			{
				if (firstUpdate)
				{
					Reset();
					firstUpdate = false;
				}
				direction = Vector3.Lerp(direction, (transform.position - lazyPoint) / deltaTime * 0.01f, deltaTime * acceleration);
				lazyPoint += direction * deltaTime * speed;
				delta = transform.position - lastPosition;
				lazyPoint += delta * matchVelocity;
				lazyPoint.y += gravity * deltaTime;
				EffectorLink[] array = effectorLinks;
				foreach (EffectorLink effectorLink in array)
				{
					IKEffector effector = solver.GetEffector(effectorLink.effector);
					effector.positionOffset += (lazyPoint - transform.position) * effectorLink.weight * weight;
				}
				lastPosition = transform.position;
			}
		}
	}

	[Tooltip("The array of Bodies")]
	public Body[] bodies;

	[Tooltip("The array of OffsetLimits")]
	public OffsetLimits[] limits;

	public void ResetBodies()
	{
		lastTime = Time.time;
		Body[] array = bodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset();
		}
	}

	protected override void OnModifyOffset()
	{
		Body[] array = bodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Update(ik.solver, weight, deltaTime);
		}
		ApplyLimits(limits);
	}
}
