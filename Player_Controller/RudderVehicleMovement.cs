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
		if (Mathf.Approximately(base.Steering, 0f))
		{
			_rudderInput = Mathf.Lerp(_rudderInput, 0f, Time.deltaTime * 3f);
		}
		else
		{
			_rudderInput += Time.deltaTime * (base.Steering * 1f);
			_rudderInput = Mathf.Clamp(_rudderInput, -1f, 1f);
		}
		float num = _rudderInput * _rudderTurningAngle * (MathF.PI / 180f);
		float y = (-60f * _rudderInput / 2f + num) % 360f;
		_rudder.localEulerAngles = new Vector3(0f, y, 0f);
		float num2 = Vector3.Dot(base.Rigidbody.velocity, base.transform.forward);
		base.Rigidbody.AddTorque(base.transform.up * num2 * _rudderDeflection * _rudderInput);
		base.OperatingPosition.transform.rotation = base.Operator.Character.PlayerCharacterPosition.rotation;
		OnUsing();
	}
}
