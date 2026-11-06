using System.Collections.Generic;
using Beam.Events;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using MEC;
using Rewired;
using UnityEngine;

namespace Beam;

public class ControllerFocus : MonoBehaviour
{
	private static ControllerFocus _Instance;

	private List<IMenuPanel> _panels = new List<IMenuPanel>();

	private CoroutineHandle _sequence;

	public static ControllerFocus Instance
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_Instance == (Object)null && !Game.IsQuitting)
			{
				ControllerFocus controllerFocus = new GameObject("~ControllerFocus").AddComponent<ControllerFocus>();
				((Component)controllerFocus).transform.position = Vector3.zero;
				((Component)controllerFocus).transform.rotation = Quaternion.identity;
				((Component)controllerFocus).transform.SetAsFirstSibling();
				return controllerFocus;
			}
			return _Instance;
		}
	}

	private void Awake()
	{
		if ((Object)(object)_Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		_Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Options.Saved += Options_Saved;
		Options.Loaded += Options_Loaded;
		ReInput.ControllerConnectedEvent += Rewired_OnControllerConnected;
		ReInput.ControllerDisconnectedEvent += Rewired_OnControllerDisconnected;
	}

	private void Start()
	{
		DoBeginCheck();
	}

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_sequence);
		_panels.Clear();
		_panels = null;
		Options.Saved -= Options_Saved;
		Options.Loaded -= Options_Loaded;
		ReInput.ControllerConnectedEvent -= Rewired_OnControllerConnected;
		ReInput.ControllerDisconnectedEvent -= Rewired_OnControllerDisconnected;
		_Instance = null;
	}

	public void Subscribe(IMenuPanel panel)
	{
		_panels.Add(panel);
	}

	public void Unsubscribe(IMenuPanel panel)
	{
		_panels.Remove(panel);
	}

	private void Options_Saved()
	{
		RefreshAssignments();
	}

	private void Options_Loaded(JObject data)
	{
		RefreshAssignments();
	}

	private void RefreshAssignments()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected Obj, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected Obj, but got Unknown
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			foreach (Player player in ReInput.players.Players)
			{
				if (player.controllers.ContainsController((Controller)(object)joystick))
				{
					ReInput.controllers.RemoveControllerFromAllPlayers((Controller)(object)joystick, true);
					Rewired_OnControllerDisconnected(new ControllerStatusChangedEventArgs(((Controller)joystick).name, ((Controller)joystick).id, ((Controller)joystick).type));
				}
			}
		}
		foreach (Joystick joystick2 in ReInput.controllers.Joysticks)
		{
			Rewired_OnControllerConnected(new ControllerStatusChangedEventArgs(((Controller)joystick2).name, ((Controller)joystick2).id, ((Controller)joystick2).type));
		}
	}

	private void RefreshAssignment(Controller controller)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (Options.InputSettings.ControllerAssignments.TryGetValue(controller.deviceInstanceGuid.ToString(), out var value))
		{
			Player player = ReInput.players.GetPlayer(value.PlayerId);
			if (player != null)
			{
				player.controllers.AddController(controller, true);
				RewiredUtilities.RefreshMapsEnabled(player, controller.type);
				Debug.Log((object)$":: Controller [{controller.name}] assigned to P{player.id + 1}");
				EventManager.RaiseEvent(new RewiredControllerChangedEvent(0));
				DoBeginCheck();
				return;
			}
		}
		Debug.LogError((object)$":: Controller [{controller.name}] unassigned.\n{value}");
	}

	private void Rewired_OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)args.controllerType == 2)
		{
			Debug.Log((object)(":: Controller connected [" + args.name + "] (unassigned)."));
			RefreshAssignment(args.controller);
		}
	}

	private void Rewired_OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		Debug.Log((object)(":: Controller disconnected [" + args.name + "]"));
		EventManager.RaiseEvent(new RewiredControllerChangedEvent(0));
		DoEndCheck();
	}

	private void DoBeginCheck()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (_sequence.IsRunning)
		{
			return;
		}
		foreach (Player player in RewiredUtilities.Players)
		{
			if (RewiredUtilities.HasJoystick(player))
			{
				_sequence = Timing.RunCoroutine(CheckSelected_Sequence());
				break;
			}
		}
	}

	private void DoEndCheck()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (!_sequence.IsRunning)
		{
			return;
		}
		bool flag = false;
		foreach (Player player in RewiredUtilities.Players)
		{
			flag |= RewiredUtilities.HasJoystick(player);
		}
		if (!flag)
		{
			Timing.KillCoroutines(_sequence);
		}
	}

	private IEnumerator<float> CheckSelected_Sequence()
	{
		while (true)
		{
			for (int num = _panels.Count - 1; num >= 0; num--)
			{
				IMenuPanel panel = _panels[num];
				if (FixNoSelection(panel))
				{
					break;
				}
			}
			yield return 0f;
		}
	}

	private bool FixNoSelection(IMenuPanel panel)
	{
		if (!panel.Active)
		{
			return false;
		}
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (!BeamTeam.IsNullOrDestroyed(allPlayer.Input) && panel.Input.id == allPlayer.Input.id && (Object)(object)allPlayer.PlayerUI.UserInterface.EventSystem.currentSelectedGameObject == (Object)null)
			{
				panel.SelectFirstButton();
				return true;
			}
		}
		return false;
	}
}
