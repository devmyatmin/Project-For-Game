using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class Amplifier : OffsetModifier
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

		[Tooltip("The Transform that's motion we are reading.")]
		public Transform transform;

		[Tooltip("Amplify the 'transform's' position relative to this Transform.")]
		public Transform relativeTo;

		[Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector.")]
		public EffectorLink[] effectorLinks;

		[Tooltip("Amplification magnitude along the up axis of the character.")]
		public float verticalWeight = 1f;

		[Tooltip("Amplification magnitude along the horizontal axes of the character.")]
		public float horizontalWeight = 1f;

		[Tooltip("Speed of the amplifier. 0 means instant.")]
		public float speed = 3f;

		private Vector3 lastRelativePos;

		private Vector3 smoothDelta;

		private bool firstUpdate;

		public void Update(IKSolverFullBodyBiped solver, float w, float deltaTime)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)transform == (Object)null) && !((Object)(object)relativeTo == (Object)null))
			{
				Vector3 val = relativeTo.InverseTransformDirection(transform.position - relativeTo.position);
				if (firstUpdate)
				{
					lastRelativePos = val;
					firstUpdate = false;
				}
				Vector3 val2 = (val - lastRelativePos) / deltaTime;
				smoothDelta = ((speed <= 0f) ? val2 : Vector3.Lerp(smoothDelta, val2, deltaTime * speed));
				Vector3 v = relativeTo.TransformDirection(smoothDelta);
				Vector3 val3 = V3Tools.ExtractVertical(v, solver.GetRoot().up, verticalWeight) + V3Tools.ExtractHorizontal(v, solver.GetRoot().up, horizontalWeight);
				for (int i = 0; i < effectorLinks.Length; i++)
				{
					IKEffector effector = solver.GetEffector(effectorLinks[i].effector);
					effector.positionOffset += val3 * w * effectorLinks[i].weight;
				}
				lastRelativePos = val;
			}
		}

		private static Vector3 Multiply(Vector3 v1, Vector3 v2)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			v1.x *= v2.x;
			v1.y *= v2.y;
			v1.z *= v2.z;
			return v1;
		}
	}

	[Tooltip("The amplified bodies.")]
	public Body[] bodies;

	protected override void OnModifyOffset()
	{
		if (!ik.fixTransforms)
		{
			if (!Warning.logged)
			{
				Warning.Log("Amplifier needs the Fix Transforms option of the FBBIK to be set to true. Otherwise it might amplify to infinity, should the animator of the character stop because of culling.", ((Component)this).transform);
			}
			return;
		}
		Body[] array = bodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Update(ik.solver, weight, deltaTime);
		}
	}
}
