using System;
using Beam.Crafting;
using UnityEngine;

namespace Beam;

public class MotorVehicleMovement : VehicleMovementBase, IHaveFuel
{
	private const float IDLE_GAS_PER_HOUR = 0.1f;

	private const float RUNNING_GAS_PER_HOUR = 0.45454544f;

	private float _fuel;

	[SerializeField]
	private Transform _rudder;

	[SerializeField]
	private float _engineForce = 2f;

	[SerializeField]
	private float _rudderDeflection = 1f;

	private float _propellerTurningAngle = 0.5f;

	private float _rudderInput;

	[SerializeField]
	private Constructing_BOAT_MOTOR _motorConstruction;

	private Construction_STRUCTURE_RAFT _raftStructure;

	private bool _ranOutOfFuel;

	public float FuelCapacity => 4f;

	public float Fuel => _fuel;

	public event Action FuelRanOut;

	protected virtual void OnFuelRanOut()
	{
		FuelRanOut?.Invoke();
	}

	private void Awake()
	{
		_ranOutOfFuel = _fuel == 0f;
	}

	float IHaveFuel.Refill(float availableFuel)
	{
		float num = Mathf.Min(FuelCapacity - _fuel, availableFuel);
		if (num > 0f)
		{
			_fuel += num;
			_fuel = Mathf.Clamp(_fuel, 0f, FuelCapacity);
			if (_fuel > 0f)
			{
				_ranOutOfFuel = false;
			}
		}
		return num;
	}

	private bool AnchorsDeployed()
	{
		if ((Object)(object)_raftStructure == (Object)null)
		{
			AssignRaftStructure();
		}
		if ((Object)(object)_raftStructure != (Object)null)
		{
			return _raftStructure.AnchorsDeployed > 0;
		}
		return false;
	}

	private void AssignRaftStructure()
	{
		if ((Object)(object)_motorConstruction != (Object)null)
		{
			_raftStructure = _motorConstruction.Structure as Construction_STRUCTURE_RAFT;
		}
	}

	public override bool EngineIsRunning()
	{
		if (_ranOutOfFuel)
		{
			return false;
		}
		return base.EngineIsRunning();
	}

	public override void Move()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Approximately(Steering, 0f))
		{
			_rudderInput = Mathf.Lerp(_rudderInput, 0f, Time.deltaTime * 3f);
		}
		else
		{
			_rudderInput += Time.deltaTime * (Steering * 2f);
			_rudderInput = Mathf.Clamp(_rudderInput, -1f, 1f);
		}
		float num = _rudderInput * _propellerTurningAngle * ((float)Math.PI / 180f);
		float num2 = (-65f * _rudderInput / 2f + num) % 360f;
		_rudder.localEulerAngles = new Vector3(0f, num2, 0f);
		_fuel -= ((Mathf.Abs(Throttle) > 0f) ? 0.45454544f : 0.1f) * GameTime.DeltaTimeHour;
		if (_fuel > 0f)
		{
			if (!AnchorsDeployed() && !Rigidbody.isKinematic)
			{
				Vector3 val = ((Component)this).transform.forward * Mathf.Cos(num) - ((Component)Rigidbody).transform.right * Mathf.Sin(num);
				Rigidbody.AddForce(val * (_engineForce * Throttle));
			}
		}
		else if (!_ranOutOfFuel)
		{
			_fuel = 0f;
			_ranOutOfFuel = true;
			OnFuelRanOut();
		}
		if (!Rigidbody.isKinematic)
		{
			float num3 = Vector3.Dot(Rigidbody.velocity, ((Component)this).transform.forward);
			Rigidbody.AddTorque(((Component)this).transform.up * (num3 * _rudderDeflection * _rudderInput));
		}
		((Component)OperatingPosition).transform.rotation = Operator.Character.PlayerCharacterPosition.rotation;
		OnUsing();
	}
}
