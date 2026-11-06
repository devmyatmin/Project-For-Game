using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Events;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using Rewired;
using UnityEngine;

namespace Beam;

public class HotkeyController : MonoBehaviour, INeedPlayerComponent, ISaveable
{
	private class ReplicateQuickAccessSlotUnlocked : MultiplayerMessage
	{
		[Replicate]
		public int Level;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Player.Hotkeys.DoQuickAccessSlotUnloked(Level);
		}
	}

	private class ReplicateDoHotkeyAssignment : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public int Number;

		[Replicate]
		public IPickupable Obj;

		public override void OnPeer()
		{
			Player.Hotkeys.DoHotkeyAssignment(Number, Obj);
		}
	}

	private HotkeyView _view;

	private IPlayer _player;

	private HotkeyData[] _hotkeys;

	private InventoryRadialMenuPresenter _inventoryRadialMenu;

	private bool _subbedToInventoryEvents;

	private float _keyPressTime;

	private int _level;

	public IPlayer Player => _player;

	private void Awake()
	{
		_hotkeys = new HotkeyData[10];
		for (int i = 0; i < 10; i++)
		{
			int num = i + 1;
			if (num == 10)
			{
				num = 0;
			}
			_hotkeys[i] = new HotkeyData
			{
				Number = num
			};
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		PlayerUI playerUI = _player.PlayerUI;
		_view = ((playerUI != null) ? ((Component)playerUI.Canvas).GetComponentInChildren<HotkeyView>() : null);
		if ((Object)(object)_view == (Object)null)
		{
			if (_player.IsOwner)
			{
				Debug.LogError((object)$"HotkeyController:: No view component found! for player with id {_player.Id}");
			}
			return;
		}
		_view.Initialize(_hotkeys);
		_player.Holder.Dropped += Holder_Dropped;
		_inventoryRadialMenu = _player.PlayerUI.GetController<InventoryRadialMenuPresenter>();
		SubscribeToInputEvents(_player.Input);
		EventManager.AddListener<QuickAccessSlotUnlockedEvent>(EventManager_QuickAccessSlotUnlocked);
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		LoadOptions();
	}

	private void OnDestroy()
	{
		if (!BeamTeam.IsNullOrDestroyed(_player))
		{
			if (BeamTeam.IsNullOrDestroyed(_player) && (Object)(object)_player.Holder != (Object)null)
			{
				_player.Holder.Dropped -= Holder_Dropped;
			}
			if (_subbedToInventoryEvents && (Object)(object)_inventoryRadialMenu != (Object)null)
			{
				_inventoryRadialMenu.View.ShowView -= Show;
				_inventoryRadialMenu.View.HideView -= Hide;
			}
			UnsubscribeFromInputEvents(_player?.Input);
			EventManager.RemoveListener<QuickAccessSlotUnlockedEvent>(EventManager_QuickAccessSlotUnlocked);
			EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		}
	}

	private void SubscribeToInputEvents(Player input)
	{
		if (input == null)
		{
			Debug.LogError((object)"HotkeyController:: Null input.");
			return;
		}
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 67);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 67);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 67);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 69);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 69);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 69);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 70);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 70);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 70);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 71);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 71);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 71);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 72);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 72);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 72);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 73);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 73);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 73);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 74);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 74);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 74);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 75);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 75);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 75);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 76);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 76);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 76);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 77);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 77);
		input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 77);
	}

	private void UnsubscribeFromInputEvents(Player input)
	{
		if (input != null)
		{
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 67);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 67);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 67);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 69);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 69);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 69);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 70);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 70);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 70);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 71);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 71);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 71);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 72);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 72);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 72);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 73);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 73);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 73);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 74);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 74);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 74);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 75);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 75);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 75);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 76);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 76);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 76);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 77);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonLongPressed, (UpdateLoopType)0, (InputActionEventType)14, 77);
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_HotkeyButtonReleased, (UpdateLoopType)0, (InputActionEventType)4, 77);
		}
	}

	private void LoadOptions()
	{
		if (Options.GeneralSettings.AlwaysShowHotkeyBar)
		{
			if (_subbedToInventoryEvents)
			{
				_inventoryRadialMenu.View.ShowView += Show;
				_inventoryRadialMenu.View.HideView += Hide;
				_subbedToInventoryEvents = false;
			}
			_view.Show();
		}
		else
		{
			if (!_subbedToInventoryEvents)
			{
				_inventoryRadialMenu.View.ShowView += Show;
				_inventoryRadialMenu.View.HideView += Hide;
				_subbedToInventoryEvents = true;
			}
			_view.Hide();
		}
	}

	private void EventManager_OptionsApplied(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void EventManager_QuickAccessSlotUnlocked(QuickAccessSlotUnlockedEvent e)
	{
		if (e.PlayerId == _player.Id)
		{
			ReplicatedQuickAccessSlotUnlocked(e);
		}
	}

	private void DoQuickAccessSlotUnloked(int level)
	{
		_level = level;
		UpdateLevel();
		LocalizedNotification.Post(_player, NotificationPriority.Immediate, 4f, "HOTKEYS_NTF_NEW_SLOTS_UNLOCKED_DESC");
	}

	private void Holder_Dropped(IPickupable obj)
	{
		ClearExistingHotkey(obj.ReferenceId);
	}

	private void Rewired_HotkeyButtonPressed(InputActionEventData eventData)
	{
		_keyPressTime = Time.realtimeSinceStartup;
	}

	private void Rewired_HotkeyButtonLongPressed(InputActionEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		int hotkeyNumber = GetHotkeyNumber(eventData.actionId);
		if (hotkeyNumber != -1 && !_hotkeys[hotkeyNumber].Locked.Value)
		{
			IHotkeySource hotkeySource;
			if (!_inventoryRadialMenu.View.Visible)
			{
				IHotkeySource holder = _player.Holder;
				hotkeySource = holder;
			}
			else
			{
				IHotkeySource holder = _inventoryRadialMenu;
				hotkeySource = holder;
			}
			IHotkeySource hotkeySource2 = hotkeySource;
			ReplicatedDoHotkeyAssignment(hotkeyNumber, hotkeySource2.CurrentObject);
		}
		_keyPressTime = 0f;
	}

	private void Rewired_HotkeyButtonReleased(InputActionEventData eventData)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Time.realtimeSinceStartup < _keyPressTime + 1f)
		{
			int hotkeyNumber = GetHotkeyNumber(eventData.actionId);
			if (hotkeyNumber != -1 && !_hotkeys[hotkeyNumber].Locked.Value)
			{
				DoHotkey(hotkeyNumber);
			}
		}
		_keyPressTime = 0f;
	}

	private bool BlockHotkeyInput()
	{
		return _player.GameInput.CurrentHandler != null;
	}

	private void DoHotkey(int number)
	{
		if (BlockHotkeyInput())
		{
			return;
		}
		MiniGuid referenceId = _hotkeys[number].ReferenceId;
		if (referenceId.IsDefault())
		{
			return;
		}
		IPickupable pickupable = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_player.Holder.Storage.GetStored(), (IPickupable item, MiniGuid id) => item.ReferenceId.Equals(id), referenceId);
		if (BeamTeam.IsNullOrDestroyed(pickupable))
		{
			if (BeamTeam.IsNullOrDestroyed(_player.Holder.CurrentObject) || !_player.Holder.CurrentObject.ReferenceId.Equals(referenceId))
			{
				_view.OnAssignmentFailed();
			}
		}
		else
		{
			_player.Holder.ReplicatedSelect(pickupable);
		}
	}

	private void DoHotkeyAssignment(int number, IPickupable obj)
	{
		if (BlockHotkeyInput())
		{
			return;
		}
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			_view?.OnAssignmentFailed();
			return;
		}
		HotkeyData hotkeyData = _hotkeys[number];
		if (hotkeyData.ReferenceId.Equals(obj.ReferenceId))
		{
			ClearHotkey(hotkeyData);
			_view?.OnAssignmentRemoved();
			return;
		}
		ClearExistingHotkey(obj.ReferenceId);
		_hotkeys[number].ReferenceId = obj.ReferenceId;
		_hotkeys[number].CraftingType.Value = obj.CraftingType;
		_view?.OnAssignmentCreated();
	}

	public void Show()
	{
		_view.Show();
	}

	public void Hide()
	{
		_view.Hide();
	}

	private void ClearHotkey(HotkeyData hotkeyData)
	{
		hotkeyData.ReferenceId = default;
		hotkeyData.CraftingType.Value = CraftingType.Empty;
	}

	private void ClearExistingHotkey(MiniGuid referenceId)
	{
		HotkeyData[] hotkeys = _hotkeys;
		foreach (HotkeyData hotkeyData in hotkeys)
		{
			if (hotkeyData.ReferenceId.Equals(referenceId))
			{
				ClearHotkey(hotkeyData);
			}
		}
	}

	private void UpdateLevel()
	{
		_hotkeys[0].Locked.Value = _level < 1;
		_hotkeys[1].Locked.Value = _level < 2;
		_hotkeys[2].Locked.Value = _level < 2;
		_hotkeys[3].Locked.Value = _level < 3;
		_hotkeys[4].Locked.Value = _level < 3;
		_hotkeys[5].Locked.Value = _level < 3;
		_hotkeys[6].Locked.Value = _level < 4;
		_hotkeys[7].Locked.Value = _level < 4;
		_hotkeys[8].Locked.Value = _level < 4;
		_hotkeys[9].Locked.Value = _level < 4;
	}

	private int GetHotkeyNumber(int actionId)
	{
		return actionId switch
		{
			67 => 0, 
			69 => 1, 
			70 => 2, 
			71 => 3, 
			72 => 4, 
			73 => 5, 
			74 => 6, 
			75 => 7, 
			76 => 8, 
			77 => 9, 
			_ => -1, 
		};
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Level", _level);
		JObject jObject2 = new JObject();
		for (int i = 0; i < _hotkeys.Length; i++)
		{
			HotkeyData hotkeyData = _hotkeys[i];
			JObject jObject3 = new JObject();
			jObject3.AddField("Locked", hotkeyData.Locked.Value);
			jObject3.AddField("ReferenceId", hotkeyData.ReferenceId.ToString());
			jObject2.Add(jObject3);
		}
		jObject.AddField("Hotkeys", jObject2);
		return jObject;
	}

	public void Load(JObject data)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_level = data.GetField("Level").GetValue<int>();
		UpdateLevel();
		JObject field = data.GetField("Hotkeys");
		Timing.RunCoroutine(DeferredLoad(field));
	}

	private IEnumerator<float> DeferredLoad(JObject hotkeysData)
	{
		float startTime = Time.unscaledTime;
		while (!_player.Inventory.Loaded || !_player.Holder.Loaded)
		{
			if (Time.unscaledTime > startTime + PlayerRegistry.LocalPeer.GlobalTimeout)
			{
				MultiplayerMng.LogError($"Timeout while waiting for Holder & Inventory ({_player.Inventory.GetSlotStorage().LoadState}) to load.");
				yield break;
			}
			yield return 0f;
		}
		List<IPickupable> list = new List<IPickupable>();
		list.AddRange(_player.Inventory.GetSlotStorage().GetStored());
		if (!BeamTeam.IsNullOrDestroyed(_player.Holder.CurrentObject))
		{
			list.Add(_player.Holder.CurrentObject);
		}
		for (int i = 0; i < hotkeysData.Children.Count; i++)
		{
			JObject jObject = hotkeysData.Children[i];
			bool value = jObject.GetField("Locked").GetValue<bool>();
			if (value)
			{
				break;
			}
			_hotkeys[i].Locked.Value = value;
			MiniGuid miniGuid = Beam.Utilities.ExtensionMethods.ToMiniGuid(jObject.GetField("ReferenceId").GetValue<string>());
			if (!miniGuid.IsDefault())
			{
				IPickupable pickupable = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(list, (IPickupable o, MiniGuid id) => o.ReferenceId.Equals(id), miniGuid);
				if (!BeamTeam.IsNullOrDestroyed(pickupable))
				{
					_hotkeys[i].ReferenceId = miniGuid;
					_hotkeys[i].CraftingType.Value = pickupable.CraftingType;
				}
			}
		}
	}

	private void ReplicatedQuickAccessSlotUnlocked(QuickAccessSlotUnlockedEvent e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateQuickAccessSlotUnlocked replicateQuickAccessSlotUnlocked = new ReplicateQuickAccessSlotUnlocked();
			replicateQuickAccessSlotUnlocked.Level = e.Level;
			replicateQuickAccessSlotUnlocked.Player = _player;
			replicateQuickAccessSlotUnlocked.Post();
		}
		DoQuickAccessSlotUnloked(e.Level);
	}

	private void ReplicatedDoHotkeyAssignment(int number, IPickupable obj)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateDoHotkeyAssignment replicateDoHotkeyAssignment = new ReplicateDoHotkeyAssignment();
			replicateDoHotkeyAssignment.Player = _player;
			replicateDoHotkeyAssignment.Number = number;
			replicateDoHotkeyAssignment.Obj = obj;
			replicateDoHotkeyAssignment.Post();
		}
		DoHotkeyAssignment(number, obj);
	}
}
