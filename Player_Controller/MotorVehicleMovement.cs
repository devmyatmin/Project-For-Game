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
		this.FuelRanOut?.Invoke();
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
		if (_raftStructure == null)
		{
			AssignRaftStructure();
		}
		if (_raftStructure != null)
		{
			return _raftStructure.AnchorsDeployed > 0;
		}
		return false;
	}

	private void AssignRaftStructure()
	{
		if (_motorConstruction != null)
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
		if (Mathf.Approximately(base.Steering, 0f))
		{
			_rudderInput = Mathf.Lerp(_rudderInput, 0f, Time.deltaTime * 3f);
		}
		else
		{
			_rudderInput += Time.deltaTime * (base.Steering * 2f);
			_rudderInput = Mathf.Clamp(_rudderInput, -1f, 1f);
		}
		float num = _rudderInput * _propellerTurningAngle * (MathF.PI / 180f);
		float y = (-65f * _rudderInput / 2f + num) % 360f;
		_rudder.localEulerAngles = new Vector3(0f, y, 0f);
		_fuel -= ((Mathf.Abs(base.Throttle) > 0f) ? 0.45454544f : 0.1f) * GameTime.DeltaTimeHour;
		if (_fuel > 0f)
		{
			if (!AnchorsDeployed() && !base.Rigidbody.isKinematic)
			{
				Vector3 vector = base.transform.forward * Mathf.Cos(num) - base.Rigidbody.transform.right * Mathf.Sin(num);
				base.Rigidbody.AddForce(vector * (_engineForce * base.Throttle));
			}
		}
		else if (!_ranOutOfFuel)
		{
			_fuel = 0f;
			_ranOutOfFuel = true;
			OnFuelRanOut();
		}
		if (!base.Rigidbody.isKinematic)
		{
			float num2 = Vector3.Dot(base.Rigidbody.velocity, base.transform.forward);
			base.Rigidbody.AddTorque(base.transform.up * (num2 * _rudderDeflection * _rudderInput));
		}
		base.OperatingPosition.transform.rotation = base.Operator.Character.PlayerCharacterPosition.rotation;
		OnUsing();
	}
}
