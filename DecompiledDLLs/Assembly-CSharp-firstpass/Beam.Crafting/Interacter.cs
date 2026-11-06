using System;
using System.Collections.Generic;
using Beam.Events;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Interacter : MonoBehaviour, IGameInputHandler, IGameActionProvider, INeedPlayerComponent
{
	private class PostReplicateDoInteraction_GameAction : MultiplayerMessage
	{
		[Replicate]
		public ISaveablePrefab Obj;

		[Replicate]
		public IInteractable Interactable;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Interactable.Interact_(Player, Obj as IBase);
			ReplicateInteract replicateInteract = new ReplicateInteract();
			replicateInteract.Obj = Obj;
			replicateInteract.Interactable = Interactable;
			replicateInteract.Player = Player;
			replicateInteract.Post();
		}
	}

	private class ReplicateInteract : MultiplayerMessage
	{
		[Replicate]
		public ISaveablePrefab Obj;

		[Replicate]
		public IInteractable Interactable;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Interactable.Interact_(Player, Obj as IBase);
		}
	}

	private class ReplicateOnInteractingFellThrough : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public ISaveablePrefab RaycasterCurrentObject;

		public override void OnPeer()
		{
			Player.User.Controller.Raycaster.SetCurrentObject(RaycasterCurrentObject as IBase);
			Player.Interacter.OnInteractingFellThrough();
		}
	}

	private IPlayer _player;

	private IGameInputController _controller;

	private bool _time;

	private float _timer;

	private List<IGameAction> _gameActions = new List<IGameAction>();

	private GameAction _gameAction = new GameAction();

	private IInteractable _activeInteractable;

	IPlayer INeedPlayerComponent.Player => _player;

	public bool IsInPauseMenu { get; set; }

	public event Action<bool> InteractingEnabled;

	public event FloatEventHandler InteractingUpdated;

	public event EmptyEventHandler InteractingFinished;

	public event EmptyEventHandler InteractingFellThrough;

	public event Action<Interacter, IBase> RequestGameActions;

	protected virtual void OnInteractingEnabled(bool enabled)
	{
		InteractingEnabled?.Invoke(enabled);
	}

	protected virtual void OnInteractingUpdated(float value)
	{
		InteractingUpdated?.Invoke(value);
	}

	protected virtual void OnInteractingFinished()
	{
		InteractingFinished?.Invoke();
	}

	protected virtual void OnInteractingFellThrough()
	{
		InteractingFellThrough?.Invoke();
	}

	protected virtual void OnRequestGameActions(Interacter sender, IBase obj)
	{
		RequestGameActions?.Invoke(this, obj);
	}

	private void EventManager_SetActiveObjectEvent(SetActiveObjectEvent e)
	{
		IInteractable interactable = e.ActiveObject as IInteractable;
		if (!BeamTeam.IsNullOrDestroyed(interactable))
		{
			_activeInteractable = interactable;
		}
	}

	public void Initialize(IGameInputController controller)
	{
		_controller = controller;
		_controller.Raycaster.LeaveObject += Raycaster_LeaveObject;
		EventManager.AddListener<SetActiveObjectEvent>(EventManager_SetActiveObjectEvent);
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Movement.VehicleChanged += PlayerMovement_VehicleChanged;
		_gameAction.PlayerActionRequiredDescription = "PLAYER_ACTION_HOLD";
		_gameAction.ElementRequiredId = 9;
		_gameAction.Action = PostReplicatedDoInteraction_GameAction;
	}

	private void OnDestroy()
	{
		if (BeamTeam.IsValid(_controller) && Object.op_Implicit((Object)(object)_controller.Raycaster))
		{
			_controller.Raycaster.LeaveObject -= Raycaster_LeaveObject;
		}
		if (!BeamTeam.IsNullOrDestroyed(_player) && Object.op_Implicit((Object)(object)_player.Movement))
		{
			_player.Movement.VehicleChanged -= PlayerMovement_VehicleChanged;
		}
		EventManager.RemoveListener<SetActiveObjectEvent>(EventManager_SetActiveObjectEvent);
		InteractingEnabled = null;
		InteractingUpdated = null;
		InteractingFinished = null;
		InteractingFellThrough = null;
		RequestGameActions = null;
	}

	private void PlayerMovement_VehicleChanged(object sender, EventArgs e)
	{
		Movement movement = sender as Movement;
		if (Object.op_Implicit((Object)(object)movement))
		{
			_activeInteractable = movement.Vehicle as IInteractable;
		}
	}

	private bool CanInteractWithObject(IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return false;
		}
		if (BeamTeam.IsNullOrDestroyed(_activeInteractable))
		{
			return true;
		}
		IInteractable activeInteractable = _activeInteractable;
		if (activeInteractable is Constructing_BOAT_MOTOR || activeInteractable is Constructing_RUDDER)
		{
			if (_activeInteractable != obj)
			{
				if (!(obj is Constructing_SAIL))
				{
					return obj is Constructing_ANCHOR;
				}
				return true;
			}
			return true;
		}
		return _activeInteractable == obj;
	}

	private void Raycaster_LeaveObject(IBase obj)
	{
		_gameActions.Clear();
	}

	public void AddGameAction(IGameAction gameAction)
	{
		_gameActions.Add(gameAction);
	}

	IEnumerable<IGameAction> IGameActionProvider.GetGameActions()
	{
		_gameActions.Clear();
		if (!CanInteractWithObject(_controller.Raycaster.CurrentObject))
		{
			OnInteractingEnabled(enabled: false);
			return _gameActions;
		}
		OnRequestGameActions(this, _controller.Raycaster.CurrentObject);
		if (_gameActions.Count == 0)
		{
			IInteractable interactable = _controller.Raycaster.CurrentObject as IInteractable;
			if (!BeamTeam.IsNullOrDestroyed(interactable) && interactable.CanInteract)
			{
				string interactionDescription = interactable.GetInteractionDescription(_player.Id, _player.Holder.CurrentObject);
				if (!string.IsNullOrEmpty(interactionDescription))
				{
					_gameAction.ResultingActionDescription = interactionDescription;
					_gameActions.Add(_gameAction);
				}
			}
		}
		OnInteractingEnabled(_gameActions.Count > 0);
		return _gameActions;
	}

	public void CheckInput()
	{
		if (!_player.IsOwner || IsInPauseMenu)
		{
			return;
		}
		if (_time)
		{
			_controller.CurrentHandler = this;
			_timer += Time.deltaTime;
			OnInteractingUpdated(_timer);
			if (_timer > 0.35f || !_player.Input.GetButton(9))
			{
				_time = false;
				if (_timer < 0.35f)
				{
					ReplicatedOnInteractingFellThrough();
					OnInteractingFinished();
				}
				else
				{
					Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_gameActions)?.DoAction(_controller.Raycaster.CurrentObject);
					OnInteractingFinished();
				}
				_controller.CurrentHandler = null;
			}
		}
		else if (_player.Input.GetButtonDown(9))
		{
			if (_gameActions.Count > 0)
			{
				_timer = 0f;
				_time = true;
			}
			else
			{
				ReplicatedOnInteractingFellThrough();
			}
		}
	}

	public void PostReplicatedDoInteraction_GameAction(IBase obj = null)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		IInteractable interactable = obj as IInteractable;
		if (!BeamTeam.IsNullOrDestroyed(interactable) && interactable.CanInteract)
		{
			if (_player.Id == 1 && MPExtensions.IsMultiplayer(Game.Mode))
			{
				PostReplicateDoInteraction_GameAction postReplicateDoInteraction_GameAction = new PostReplicateDoInteraction_GameAction();
				postReplicateDoInteraction_GameAction.Obj = _player.Holder.CurrentObject;
				postReplicateDoInteraction_GameAction.Interactable = interactable;
				postReplicateDoInteraction_GameAction.Player = _player;
				postReplicateDoInteraction_GameAction.Post();
			}
			else
			{
				ReplicatedInteract(obj);
			}
		}
	}

	private void ReplicatedInteract(IBase obj)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		IInteractable interactable = obj as IInteractable;
		if (!BeamTeam.IsNullOrDestroyed(interactable) && interactable.CanInteract)
		{
			if (MPExtensions.IsMultiplayer(Game.Mode))
			{
				ReplicateInteract replicateInteract = new ReplicateInteract();
				replicateInteract.Player = _player;
				replicateInteract.Interactable = interactable;
				replicateInteract.Obj = _player.Holder.CurrentObject;
				replicateInteract.Post();
			}
			interactable.Interact_(_player, _player.Holder.CurrentObject);
		}
	}

	protected void ReplicatedOnInteractingFellThrough()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateOnInteractingFellThrough replicateOnInteractingFellThrough = new ReplicateOnInteractingFellThrough();
			replicateOnInteractingFellThrough.Player = _player;
			replicateOnInteractingFellThrough.RaycasterCurrentObject = _controller.Raycaster.CurrentObject as ISaveablePrefab;
			replicateOnInteractingFellThrough.Post();
		}
		OnInteractingFellThrough();
	}
}
