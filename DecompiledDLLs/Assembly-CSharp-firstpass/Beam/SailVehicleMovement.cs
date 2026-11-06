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
		if (!Operator.IsOwner)
		{
			return Operator.MultiplayerState.VehicleSail;
		}
		return Operator.Input.GetAxis(64);
	}

	public override bool EngineIsRunning()
	{
		return false;
	}

	protected override void Input()
	{
		base.Input();
		Sail = GetSailInput();
		if (Operator.IsOwner)
		{
			Operator.MultiplayerState.VehicleSail = Sail;
		}
	}

	public override void Move()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (Validate())
		{
			float num = Wind.KmH / 3.6f * Time.deltaTime;
			_velocity = ((Component)this).transform.forward * num;
			Rigidbody.AddForce(_velocity, (ForceMode)2);
			Rigidbody.velocity = Vector3.ClampMagnitude(Rigidbody.velocity, 3.7f);
			OnUsing();
		}
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
		if ((Object)(object)_sailConstruction != (Object)null)
		{
			_raftStructure = _sailConstruction.Structure as Construction_STRUCTURE_RAFT;
		}
	}

	private bool Validate()
	{
		if (Operator == null)
		{
			return false;
		}
		if (IsOverturned())
		{
			Eject(Operator);
			return false;
		}
		if (AnchorsDeployed())
		{
			return false;
		}
		return true;
	}
}
