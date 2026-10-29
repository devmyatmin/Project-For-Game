using Beam.Crafting;
using UnityEngine;

namespace Beam;

public class SailVehicleMovement : VehicleMovementBase
{
	[SerializeField]
	private Transform _sail;

	private float _sailTurningAngle = 0.5f;

	private float _sailInput;

	private Vector3 _velocity;

	[SerializeField]
	private Constructing_SAIL _sailConstruction;

	private Construction_STRUCTURE_RAFT _raftStructure;

	public float Sail { get; protected set; }

	private float GetSailInput()
	{
		if (!base.Operator.IsOwner)
		{
			return base.Operator.MultiplayerState.VehicleSail;
		}
		return base.Operator.Input.GetAxis(64);
	}

	public override bool EngineIsRunning()
	{
		return false;
	}

	protected override void Input()
	{
		base.Input();
		Sail = GetSailInput();
		if (base.Operator.IsOwner)
		{
			base.Operator.MultiplayerState.VehicleSail = Sail;
		}
	}

	public override void Move()
	{
		if (Validate())
		{
			float num = Wind.KmH / 3.6f * Time.deltaTime;
			_velocity = base.transform.forward * num;
			base.Rigidbody.AddForce(_velocity, ForceMode.VelocityChange);
			base.Rigidbody.velocity = Vector3.ClampMagnitude(base.Rigidbody.velocity, 3.7f);
			OnUsing();
		}
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
		if (_sailConstruction != null)
		{
			_raftStructure = _sailConstruction.Structure as Construction_STRUCTURE_RAFT;
		}
	}

	private bool Validate()
	{
		if (base.Operator == null)
		{
			return false;
		}
		if (IsOverturned())
		{
			Eject(base.Operator);
			return false;
		}
		if (AnchorsDeployed())
		{
			return false;
		}
		return true;
	}
}
