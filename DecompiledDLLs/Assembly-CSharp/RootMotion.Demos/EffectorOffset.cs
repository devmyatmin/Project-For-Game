using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class EffectorOffset : OffsetModifier
{
	[Range(0f, 1f)]
	public float handsMaintainRelativePositionWeight;

	public Vector3 bodyOffset;

	public Vector3 leftShoulderOffset;

	public Vector3 rightShoulderOffset;

	public Vector3 leftThighOffset;

	public Vector3 rightThighOffset;

	public Vector3 leftHandOffset;

	public Vector3 rightHandOffset;

	public Vector3 leftFootOffset;

	public Vector3 rightFootOffset;

	protected override void OnModifyOffset()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		ik.solver.leftHandEffector.maintainRelativePositionWeight = handsMaintainRelativePositionWeight;
		ik.solver.rightHandEffector.maintainRelativePositionWeight = handsMaintainRelativePositionWeight;
		IKEffector bodyEffector = ik.solver.bodyEffector;
		bodyEffector.positionOffset += ((Component)this).transform.rotation * bodyOffset * weight;
		IKEffector leftShoulderEffector = ik.solver.leftShoulderEffector;
		leftShoulderEffector.positionOffset += ((Component)this).transform.rotation * leftShoulderOffset * weight;
		IKEffector rightShoulderEffector = ik.solver.rightShoulderEffector;
		rightShoulderEffector.positionOffset += ((Component)this).transform.rotation * rightShoulderOffset * weight;
		IKEffector leftThighEffector = ik.solver.leftThighEffector;
		leftThighEffector.positionOffset += ((Component)this).transform.rotation * leftThighOffset * weight;
		IKEffector rightThighEffector = ik.solver.rightThighEffector;
		rightThighEffector.positionOffset += ((Component)this).transform.rotation * rightThighOffset * weight;
		IKEffector leftHandEffector = ik.solver.leftHandEffector;
		leftHandEffector.positionOffset += ((Component)this).transform.rotation * leftHandOffset * weight;
		IKEffector rightHandEffector = ik.solver.rightHandEffector;
		rightHandEffector.positionOffset += ((Component)this).transform.rotation * rightHandOffset * weight;
		IKEffector leftFootEffector = ik.solver.leftFootEffector;
		leftFootEffector.positionOffset += ((Component)this).transform.rotation * leftFootOffset * weight;
		IKEffector rightFootEffector = ik.solver.rightFootEffector;
		rightFootEffector.positionOffset += ((Component)this).transform.rotation * rightFootOffset * weight;
	}
}
