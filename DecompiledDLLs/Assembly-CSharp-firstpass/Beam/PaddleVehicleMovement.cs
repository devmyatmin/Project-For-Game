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
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (Operator != null && Operating)
		{
			if (Operator.PlayerCamera.MouseLook.TimeSinceLastMoved > 3f)
			{
				Operator.PlayerCamera.MouseLook.ClampViewToPosUpdate(OperatingPosition.forward, -20f, 90f, -50f, 30f, lerpAngles: true);
			}
			else
			{
				Operator.PlayerCamera.MouseLook.ClampViewToPosUpdate(OperatingPosition.forward, -80f, 130f, 0f, 0f);
			}
		}
	}

	public override void Move()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		if (Operator.Statistics.IsKnockedOut)
		{
			ReplicatedStopOperating(Operator);
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
		float num = Mathf.Clamp01(Throttle);
		float num2 = Steering;
		Vector2 val = new Vector2(num, num2);
		val.Normalize();
		if (val.x > 0.85f || Mathf.Abs(num2) < 0.001f)
		{
			num2 = 0f;
		}
		Vector3 val2 = ((Component)this).transform.forward * num;
		val2 += ((Component)this).transform.right * num2;
		if (!(val2.sqrMagnitude > 0f))
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
			float num3 = (Time.time - _start) / 1.4f;
			if (_liferaft.IsOwner)
			{
				float num4 = _powerCurve.Evaluate(num3);
				Rigidbody.AddForce(OperatingPosition.forward * num * _engineForce * num4);
				Rigidbody.AddTorque(((Component)this).transform.up * num2 * _engineTorque * num4);
			}
			if (Time.time > _start + 1.4f)
			{
				_row = false;
			}
		}
	}

	private void ReplicatedStopOperating(IPlayer @operator)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateStopOperating replicateStopOperating = new ReplicateStopOperating();
			replicateStopOperating.Player = Operator;
			replicateStopOperating.VehicleMovement = this;
			replicateStopOperating.Post();
		}
		StopOperating(@operator);
	}
}
