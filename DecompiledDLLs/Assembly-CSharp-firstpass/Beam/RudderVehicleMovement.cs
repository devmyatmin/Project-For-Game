using System;
using UnityEngine;

namespace Beam;

public class RudderVehicleMovement : VehicleMovementBase
{
	[SerializeField]
	private Transform _handle;

	[SerializeField]
	private Transform _rudder;

	[SerializeField]
	private AnimationCurve _powerCurve;

	[SerializeField]
	private AnimationClip _useClip;

	[SerializeField]
	private float _rudderDeflection = 1f;

	private float _rudderTurningAngle = 0.5f;

	private float _rudderInput;

	public override void Move()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Approximately(Steering, 0f))
		{
			_rudderInput = Mathf.Lerp(_rudderInput, 0f, Time.deltaTime * 3f);
		}
		else
		{
			_rudderInput += Time.deltaTime * (Steering * 1f);
			_rudderInput = Mathf.Clamp(_rudderInput, -1f, 1f);
		}
		float num = _rudderInput * _rudderTurningAngle * ((float)Math.PI / 180f);
		float num2 = (-60f * _rudderInput / 2f + num) % 360f;
		_rudder.localEulerAngles = new Vector3(0f, num2, 0f);
		float num3 = Vector3.Dot(Rigidbody.velocity, ((Component)this).transform.forward);
		Rigidbody.AddTorque(((Component)this).transform.up * num3 * _rudderDeflection * _rudderInput);
		((Component)OperatingPosition).transform.rotation = Operator.Character.PlayerCharacterPosition.rotation;
		OnUsing();
	}
}
