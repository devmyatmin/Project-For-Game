using Beam.Crafting;
using Funlabs;
using UnityEngine;

namespace Beam;

public class PaddleVehicleMovement : VehicleMovementBase
{
	private class ReplicateStopOperating : MultiplayerMessage
	{
		[Replicate]
		public PaddleVehicleMovement VehicleMovement;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			VehicleMovement.StopOperating(Player);
		}
	}

	private const float POWER_CURVE_DURATION = 1.4f;

	[SerializeField]
	private Buoyancy raft;

	[SerializeField]
	private InteractiveObject_LIFERAFT _liferaft;

	[SerializeField]
	private AnimationCurve _powerCurve;

	[SerializeField]
	private float _engineForce = 2f;

	[SerializeField]
	private float _engineTorque = 2f;

	private bool _row;

	private float _start;

	private void Update()
	{
		if (base.Operator != null && base.Operating)
		{
			if (base.Operator.PlayerCamera.MouseLook.TimeSinceLastMoved > 3f)
			{
				base.Operator.PlayerCamera.MouseLook.ClampViewToPosUpdate(base.OperatingPosition.forward, -20f, 90f, -50f, 30f, lerpAngles: true);
			}
			else
			{
				base.Operator.PlayerCamera.MouseLook.ClampViewToPosUpdate(base.OperatingPosition.forward, -80f, 130f, 0f, 0f);
			}
		}
	}

	public override void Move()
	{
		if (base.Operator.Statistics.IsKnockedOut)
		{
			ReplicatedStopOperating(base.Operator);
		}
		if (!_liferaft.IsOwner)
		{
			raft.CheckIfInWater();
			if (!raft.IsInWater)
			{
				return;
			}
		}
		if (_liferaft.IsOwner && !raft.Floating)
		{
			return;
		}
		float num = Mathf.Clamp01(base.Throttle);
		float num2 = base.Steering;
		Vector2 vector = new Vector2(num, num2);
		vector.Normalize();
		if (vector.x > 0.85f || Mathf.Abs(num2) < 0.001f)
		{
			num2 = 0f;
		}
		Vector3 vector2 = base.transform.forward * num;
		if (!((vector2 + base.transform.right * num2).sqrMagnitude > 0f))
		{
			return;
		}
		if (!_row)
		{
			OnUsing();
			_start = Time.time;
			_row = true;
		}
		if (_row)
		{
			float time = (Time.time - _start) / 1.4f;
			if (_liferaft.IsOwner)
			{
				float num3 = _powerCurve.Evaluate(time);
				base.Rigidbody.AddForce(base.OperatingPosition.forward * num * _engineForce * num3);
				base.Rigidbody.AddTorque(base.transform.up * num2 * _engineTorque * num3);
			}
			if (Time.time > _start + 1.4f)
			{
				_row = false;
			}
		}
	}

	private void ReplicatedStopOperating(IPlayer @operator)
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateStopOperating replicateStopOperating = new ReplicateStopOperating();
			replicateStopOperating.Player = base.Operator;
			replicateStopOperating.VehicleMovement = this;
			replicateStopOperating.Post();
		}
		StopOperating(@operator);
	}
}
