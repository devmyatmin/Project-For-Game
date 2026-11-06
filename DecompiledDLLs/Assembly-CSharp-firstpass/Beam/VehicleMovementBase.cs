using System;
using System.Collections.Generic;
using Beam.Events;
using Funlabs;
using MEC;
using UnityEngine;

namespace Beam;

public abstract class VehicleMovementBase : MonoBehaviour
{
	[SerializeField]
	private Transform _operatingPosition;

	[SerializeField]
	private VehicleSeat[] _passengerSeats;

	[SerializeField]
	private Transform _passengerSeat;

	private CoroutineHandle _inputSequence;

	private CoroutineHandle _movingSequence;

	public IPlayer Operator { get; set; }

	public bool Operating => Operator != null;

	public Transform OperatingPosition => _operatingPosition;

	public VehicleSeat[] PassengerSeats => _passengerSeats;

	public Transform PassengerSeat => _passengerSeat;

	public Rigidbody Rigidbody { get; set; }

	public float Throttle { get; protected set; }

	public float Steering { get; protected set; }

	public event Action<bool> Operated;

	public event Action OperatingFailed;

	public event Action Using;

	protected virtual void OnOperated(bool value)
	{
		Operated?.Invoke(value);
	}

	protected virtual void OnOperatingFailed()
	{
		OperatingFailed?.Invoke();
	}

	protected virtual void OnUsing()
	{
		Using?.Invoke();
	}

	public abstract void Move();

	protected virtual float GetThrottleInput()
	{
		if (!Operator.IsOwner)
		{
			return Operator.MultiplayerState.VehicleThrottle;
		}
		return Operator.Movement.GetVerticalInput();
	}

	protected virtual float GetSteeringInput()
	{
		if (!Operator.IsOwner)
		{
			return Operator.MultiplayerState.VehicleSteering;
		}
		return Operator.Movement.GetHorizontalInput();
	}

	public virtual float GetEngineLoad()
	{
		return Throttle;
	}

	public virtual bool EngineIsRunning()
	{
		return Operating;
	}

	private void TempCollision(IPlayer player, bool state)
	{
		((Collider)player.Movement.CharacterController).enabled = !state;
	}

	public void Operate(IPlayer player, bool operate)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Operator = player;
		OnOperated(operate);
		if (operate)
		{
			_inputSequence = Timing.RunCoroutine(Input_Sequence(), (Segment)0);
			_movingSequence = Timing.RunCoroutine(Movement_Sequence(), (Segment)1);
		}
		else
		{
			Operator = null;
			Timing.KillCoroutines(_inputSequence);
			Timing.KillCoroutines(_movingSequence);
		}
	}

	public virtual bool StartOperating(IPlayer player)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (Operator == null && player.Movement.Vehicle == null && player.Movement.grounded && !IsOverturned())
		{
			TempCollision(player, state: true);
			if (player.IsOwner)
			{
				RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: true, 10);
				RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: false, 0);
			}
			player.Holder.SetEnabled(enabled: false);
			player.Character.CurrentCharacterPosition = OperatingPosition;
			player.Character.CurrentCharacterPosition.rotation = OperatingPosition.rotation;
			player.Character.HideCurrentItem();
			player.Movement.sprinting = false;
			Operate(player, operate: true);
			return true;
		}
		return false;
	}

	public virtual bool StopOperating(IPlayer player)
	{
		if (Operator != null && Operator.Equals(player))
		{
			TempCollision(player, state: false);
			if (player.IsOwner)
			{
				RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: false, 10);
				RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: true, 0);
			}
			player.Holder.SetEnabled(enabled: true);
			player.Character.ShowCurrentItem();
			player.Movement.Vehicle = null;
			Operate(player, operate: false);
			return true;
		}
		return false;
	}

	public bool StartRiding(IPlayer passenger)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (!passenger.Movement.grounded)
		{
			return false;
		}
		if (Operator != null && Operator.Equals(passenger))
		{
			return false;
		}
		VehicleSeat passengerSeat = GetPassengerSeat(passenger.Id);
		if (passengerSeat != null)
		{
			return false;
		}
		passengerSeat = GetPassengerSeat();
		if (passengerSeat != null)
		{
			TempCollision(passenger, state: true);
			if (passenger.IsOwner)
			{
				RewiredUtilities.SetAndCacheMapsEnabled(passenger.Input, state: true, 10);
				RewiredUtilities.SetAndCacheMapsEnabled(passenger.Input, state: false, 0);
			}
			passenger.Character.CurrentCharacterPosition = _passengerSeat;
			passenger.Character.CurrentCharacterPosition.rotation = _passengerSeat.rotation;
			passenger.Holder.SetEnabled(enabled: false);
			passenger.Character.HideCurrentItem();
			passenger.Character.SetInteger("Vehicle_Pose", 3);
			passengerSeat.PlayerId = passenger.Id;
			return true;
		}
		return false;
	}

	public bool StopRiding(IPlayer passenger)
	{
		VehicleSeat passengerSeat = GetPassengerSeat(passenger.Id);
		if (passengerSeat != null)
		{
			TempCollision(passenger, state: false);
			if (passenger.IsOwner)
			{
				RewiredUtilities.SetAndCacheMapsEnabled(passenger.Input, state: false, 10);
				RewiredUtilities.SetAndCacheMapsEnabled(passenger.Input, state: true, 0);
			}
			passenger.Holder.SetEnabled(enabled: true);
			passenger.Character.ShowCurrentItem();
			passenger.Character.SetInteger("Vehicle_Pose", 0);
			passenger.Movement.Vehicle = null;
			passengerSeat.PlayerId = -1;
			return true;
		}
		return false;
	}

	public void Eject(IPlayer player)
	{
		if (!StopOperating(player))
		{
			StopRiding(player);
			player.Movement.Vehicle = null;
		}
	}

	public virtual bool IsOverturned()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Dot(((Component)this).transform.up, Vector3.up) < 0.7f;
	}

	protected virtual void Start()
	{
		EventManager.AddListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	protected virtual void OnDestroy()
	{
		Operated = null;
		OperatingFailed = null;
		Using = null;
		EventManager.RemoveListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	private void EventManager_PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		if (Operator != null && Operator.Id == disconnectedEvent.PlayerId)
		{
			Eject(Operator);
		}
	}

	protected IEnumerator<float> Input_Sequence()
	{
		while (Operating)
		{
			Input();
			yield return 0f;
		}
	}

	protected virtual void Input()
	{
		Throttle = GetThrottleInput();
		Steering = GetSteeringInput();
		if (Operator.IsOwner)
		{
			Operator.MultiplayerState.VehicleThrottle = Throttle;
			Operator.MultiplayerState.VehicleSteering = Steering;
		}
	}

	protected IEnumerator<float> Movement_Sequence()
	{
		while (Operating)
		{
			Move();
			yield return 0f;
		}
	}

	public VehicleSeat GetPassengerSeat(int id = -1)
	{
		for (int i = 0; i < _passengerSeats.Length; i++)
		{
			VehicleSeat vehicleSeat = _passengerSeats[i];
			if (vehicleSeat.PlayerId == id)
			{
				vehicleSeat.Rotation.w = 1f;
				return vehicleSeat;
			}
		}
		return null;
	}

	public VehicleSeatOrientation GetSeatOrientation(IPlayer player)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (Operator != null && Operator.Equals(player))
		{
			return new VehicleSeatOrientation
			{
				Position = OperatingPosition.position,
				Rotation = OperatingPosition.rotation
			};
		}
		VehicleSeat passengerSeat = GetPassengerSeat(player.Id);
		if (passengerSeat != null)
		{
			return new VehicleSeatOrientation
			{
				Position = ((Component)this).transform.TransformPoint(passengerSeat.Position),
				Rotation = ((Component)this).transform.rotation * passengerSeat.Rotation
			};
		}
		return default;
	}
}
