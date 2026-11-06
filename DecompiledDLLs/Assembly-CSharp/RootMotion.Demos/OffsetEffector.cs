using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class OffsetEffector : OffsetModifier
{
	[Serializable]
	public class EffectorLink
	{
		public FullBodyBipedEffector effectorType;

		public float weightMultiplier = 1f;

		[HideInInspector]
		public Vector3 localPosition;
	}

	[Tooltip("Optional. Assign the bone Transform that is closest to this OffsetEffector to be able to call OffsetEffector.Anchor() in LateUpdate to match its position and rotation to animation.")]
	public Transform anchor;

	public EffectorLink[] effectorLinks;

	private Vector3 posRelToAnchor;

	private Quaternion rotRelToAnchor = Quaternion.identity;

	protected override void Start()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		if ((Object)(object)anchor != (Object)null)
		{
			posRelToAnchor = anchor.InverseTransformPoint(((Component)this).transform.position);
			rotRelToAnchor = Quaternion.Inverse(anchor.rotation) * ((Component)this).transform.rotation;
		}
		EffectorLink[] array = effectorLinks;
		foreach (EffectorLink effectorLink in array)
		{
			Transform bone = ik.solver.GetEffector(effectorLink.effectorType).bone;
			effectorLink.localPosition = ((Component)this).transform.InverseTransformPoint(bone.position);
			if (effectorLink.effectorType == FullBodyBipedEffector.Body)
			{
				ik.solver.bodyEffector.effectChildNodes = false;
			}
		}
	}

	protected override void OnModifyOffset()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		EffectorLink[] array = effectorLinks;
		foreach (EffectorLink effectorLink in array)
		{
			Vector3 val = ((Component)this).transform.TransformPoint(effectorLink.localPosition);
			IKEffector effector = ik.solver.GetEffector(effectorLink.effectorType);
			effector.positionOffset += (val - (ik.solver.GetEffector(effectorLink.effectorType).bone.position + ik.solver.GetEffector(effectorLink.effectorType).positionOffset)) * weight * effectorLink.weightMultiplier;
		}
	}

	public void Anchor()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)anchor == (Object)null))
		{
			((Component)this).transform.position = anchor.TransformPoint(posRelToAnchor);
			((Component)this).transform.rotation = anchor.rotation * rotRelToAnchor;
		}
	}

	public OffsetEffector()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
