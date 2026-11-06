using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class PenetrationAvoidance : OffsetModifier
{
	[Serializable]
	public class Avoider
	{
		[Serializable]
		public class EffectorLink
		{
			[Tooltip("Effector to apply the offset to.")]
			public FullBodyBipedEffector effector;

			[Tooltip("Multiplier of the offset value, can be negative.")]
			public float weight;
		}

		[Tooltip("Bones to start the raycast from. Multiple raycasts can be used by assigning more than 1 bone.")]
		public Transform[] raycastFrom;

		[Tooltip("The Transform to raycast towards. Usually the body part that you want to keep from penetrating.")]
		public Transform raycastTo;

		[Tooltip("If 0, will use simple raycasting, if > 0, will use sphere casting (better, but slower).")]
		[Range(0f, 1f)]
		public float raycastRadius;

		[Tooltip("Linking this to FBBIK effectors.")]
		public EffectorLink[] effectors;

		[Tooltip("The time of smooth interpolation of the offset value to avoid penetration.")]
		public float smoothTimeIn = 0.1f;

		[Tooltip("The time of smooth interpolation of the offset value blending out of penetration avoidance.")]
		public float smoothTimeOut = 0.3f;

		[Tooltip("Layers to keep penetrating from.")]
		public LayerMask layers;

		private Vector3 offset;

		private Vector3 offsetTarget;

		private Vector3 offsetV;

		public void Solve(IKSolverFullBodyBiped solver, float weight)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			offsetTarget = GetOffsetTarget(solver);
			float num = ((offsetTarget.sqrMagnitude > offset.sqrMagnitude) ? smoothTimeIn : smoothTimeOut);
			offset = Vector3.SmoothDamp(offset, offsetTarget, ref offsetV, num);
			EffectorLink[] array = effectors;
			foreach (EffectorLink effectorLink in array)
			{
				IKEffector effector = solver.GetEffector(effectorLink.effector);
				effector.positionOffset += offset * weight * effectorLink.weight;
			}
		}

		private Vector3 GetOffsetTarget(IKSolverFullBodyBiped solver)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = Vector3.zero;
			Transform[] array = raycastFrom;
			foreach (Transform val2 in array)
			{
				val += Raycast(val2.position, raycastTo.position + val);
			}
			return val;
		}

		private Vector3 Raycast(Vector3 from, Vector3 to)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = to - from;
			float magnitude = val.magnitude;
			RaycastHit val2 = default;
			if (raycastRadius <= 0f)
			{
				Physics.Raycast(from, val, ref val2, magnitude, LayerMask.op_Implicit(layers));
			}
			else
			{
				Physics.SphereCast(from, raycastRadius, val, ref val2, magnitude, LayerMask.op_Implicit(layers));
			}
			if ((Object)(object)val2.collider == (Object)null)
			{
				return Vector3.zero;
			}
			return Vector3.Project(-val.normalized * (magnitude - val2.distance), val2.normal);
		}
	}

	[Tooltip("Definitions of penetration avoidances.")]
	public Avoider[] avoiders;

	protected override void OnModifyOffset()
	{
		Avoider[] array = avoiders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Solve(ik.solver, weight);
		}
	}
}
