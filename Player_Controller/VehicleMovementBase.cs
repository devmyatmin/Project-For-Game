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
		this.Operated?.Invoke(value);
	}

	protected virtual void OnOperatingFailed()
	{
		this.OperatingFailed?.Invoke();
	}

	protected virtual void OnUsing()
	{
		this.Using?.Invoke();
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
		player.Movement.CharacterController.enabled = !state;
	}

	public void Operate(IPlayer player, bool operate)
	{
		Operator = player;
		OnOperated(operate);
		if (operate)
		{
			_inputSequence = Timing.RunCoroutine(Input_Sequence(), Segment.Update);
			_movingSequence = Timing.RunCoroutine(Movement_Sequence(), Segment.FixedUpdate);
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
		if (Operator == null && player.Movement.Vehicle == null && player.Movement.grounded && !IsOverturned())
		{
			TempCollision(player, state: true);
			if (player.IsOwner)
			{
				player.Input.SetAndCacheMapsEnabled(state: true, 10);
				player.Input.SetAndCacheMapsEnabled(state: false, 0);
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
				player.Input.SetAndCacheMapsEnabled(state: false, 10);
				player.Input.SetAndCacheMapsEnabled(state: true, 0);
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
				passenger.Input.SetAndCacheMapsEnabled(state: true, 10);
				passenger.Input.SetAndCacheMapsEnabled(state: false, 0);
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
				passenger.Input.SetAndCacheMapsEnabled(state: false, 10);
				passenger.Input.SetAndCacheMapsEnabled(state: true, 0);
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
		return Vector3.Dot(base.transform.up, Vector3.up) < 0.7f;
	}

	protected virtual void Start()
	{
		EventManager.AddListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	protected virtual void OnDestroy()
	{
		this.Operated = null;
		this.OperatingFailed = null;
		this.Using = null;
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
		if (Operator != null && Operator.Equals(player))
		{
			VehicleSeatOrientation result = default(VehicleSeatOrientation);
			result.Position = OperatingPosition.position;
			result.Rotation = OperatingPosition.rotation;
			return result;
		}
		VehicleSeat passengerSeat = GetPassengerSeat(player.Id);
		if (passengerSeat != null)
		{
			VehicleSeatOrientation result = default(VehicleSeatOrientation);
			result.Position = base.transform.TransformPoint(passengerSeat.Position);
			result.Rotation = base.transform.rotation * passengerSeat.Rotation;
			return result;
		}
		return default(VehicleSeatOrientation);
	}
}
