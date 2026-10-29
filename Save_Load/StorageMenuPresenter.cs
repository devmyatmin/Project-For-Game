using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Events;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using Rewired;
using UnityEngine;

namespace Beam.UI;

public class StorageMenuPresenter : MonoBehaviour, IRadialMenuController, INeedPlayerComponent, IGameActionProvider
{
	private class ReplicateDummyPickupableExchange : MultiplayerMessage
	{
		[Replicate]
		public short DummyPickupablePrefabId;

		[Replicate]
		public IPlayer Player;

		[Replicate]
		public ISlotStorage<IPickupable> Src;

		[Replicate]
		public ISlotStorage<IPickupable> Dst;

		public override void OnPeer()
		{
			int num = 0;
			foreach (IPickupable item in Src.GetStored())
			{
				_ = item;
				num++;
			}
			if (num == 0)
			{
				MultiplayerMng.LogError("Msg overflow guard: Received more messages than items in storage.");
				return;
			}
			DummyPickupable dummyPickupable = new DummyPickupable((uint)DummyPickupablePrefabId);
			Exchange(Player, Src, Dst, dummyPickupable.GetInstance());
		}
	}

	private class ServerValidated_Request : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public ISaveablePrefab RaycasterCurrentObject;

		[Replicate]
		public BoltEntity RaycasterCurrentObjectBE;

		public override void OnPeer()
		{
			ISlotStorageProvider<IPickupable> slotStorageProvider = RaycasterCurrentObject as ISlotStorageProvider<IPickupable>;
			if (slotStorageProvider.IsNullOrDestroyed())
			{
				slotStorageProvider = RaycasterCurrentObjectBE?.gameObject.GetComponentInChildren<ISlotStorageProvider<IPickupable>>();
			}
			ISlotStorage<IPickupable> slotStorage = slotStorageProvider.GetSlotStorage();
			bool success = false;
			if (!slotStorage.IsOpened)
			{
				success = true;
				slotStorage.ReserveFor(Player.Id);
			}
			ServerValidated_Answer serverValidated_Answer = new ServerValidated_Answer();
			serverValidated_Answer.Player = Player;
			serverValidated_Answer.RaycasterCurrentObject = RaycasterCurrentObject;
			serverValidated_Answer.RaycasterCurrentObjectBE = RaycasterCurrentObjectBE;
			serverValidated_Answer.Success = success;
			serverValidated_Answer.Post();
		}
	}

	private class ServerValidated_Answer : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public ISaveablePrefab RaycasterCurrentObject;

		[Replicate]
		public BoltEntity RaycasterCurrentObjectBE;

		[Replicate]
		public bool Success;

		public override void OnPeer()
		{
			Player.PlayerUI.StoragePresenter._openState = OpenState.Close;
			if (Success)
			{
				IBase @base = RaycasterCurrentObject as IBase;
				if (@base.IsNullOrDestroyed())
				{
					@base = RaycasterCurrentObjectBE?.GetComponentInChildren<ISlotStorageProvider<IPickupable>>() as IBase;
				}
				Player.User.Controller.Raycaster.SetCurrentObject(@base);
				Player.PlayerUI.StoragePresenter.DoShow();
			}
		}
	}

	[SerializeField]
	private StorageRadialMenuPresenter _radialPresenter;

	[SerializeField]
	private InventoryMenuPresenter _inventoryMenuPresenter;

	private OpenState _openState;

	private IPlayer _player;

	private Raycaster _raycaster;

	private RadialMenuInput _input;

	private ISlotStorage<IPickupable> _storage;

	private List<IGameAction> _gameActions = new List<IGameAction>();

	private GameAction _gameAction = new GameAction();

	private bool _storageRefreshCallbacksAdded;

	public ISlotStorage<IPickupable> DestinationStorage { get; set; }

	public OpenState Open => _openState;

	IPlayer INeedPlayerComponent.Player => _player;

	private void Start()
	{
		_radialPresenter.ElementClick += RadialPresenter_ElementClick;
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_raycaster = _player.GameInput.Raycaster;
		_gameAction.PlayerActionRequiredDescription = "PLAYER_ACTION_PRESS";
		_gameAction.ElementRequiredId = 12;
		_gameAction.ResultingActionDescription = "ITEM_INTERACTION_DESCRIPTION_OPEN";
		_gameActions.Add(_gameAction);
		InitializeInput(_player.Input);
	}

	private void OnDestroy()
	{
		_input.OpenKeyUp -= RadialMenuInput_OpenKeyUp;
		_radialPresenter.ElementClick -= RadialPresenter_ElementClick;
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
	}

	private void InitializeInput(Rewired.Player input)
	{
		if (_input != null)
		{
			_input.OpenKeyUp -= RadialMenuInput_OpenKeyUp;
		}
		if (Options.InputSettings.ToggleRadialMenus == Quality_OFF_ON.ON)
		{
			_input = new ToggleRadialMenuInput(this, input);
		}
		else
		{
			_input = new RockerRadialMenuInput(this, input);
		}
		_input.OpenKeyUp += RadialMenuInput_OpenKeyUp;
	}

	private void EventManager_OptionsAppliedEvent(OptionsAppliedEvent e)
	{
		InitializeInput(_player.Input);
	}

	private void Update()
	{
		if (_inventoryMenuPresenter.Open.IsOpen())
		{
			return;
		}
		if (_openState.IsOpen() && _player.Statistics.IsKnockedOut)
		{
			Close();
			Hide();
			return;
		}
		_input.CheckInput();
		if (_openState.IsOpen())
		{
			_radialPresenter.UpdateSelector();
			if (_player.Input.GetButtonDown(63))
			{
				_radialPresenter.NavigateLeft();
			}
			else if (_player.Input.GetNegativeButtonDown(63))
			{
				_radialPresenter.NavigateRight();
			}
		}
	}

	private void RadialMenuInput_OpenKeyUp()
	{
		Close();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		Close();
		Hide();
	}

	private IList<IList<InventoryData>> GetInventoryData()
	{
		IList<IList<InventoryData>> list = new List<IList<InventoryData>>();
		foreach (StorageSlot<IPickupable> slot in _storage.GetSlots())
		{
			List<InventoryData> list2 = new List<InventoryData>();
			foreach (IPickupable @object in slot.Objects)
			{
				InventoryData item = new InventoryData(@object);
				list2.Add(item);
			}
			list.Add(list2);
		}
		return list;
	}

	IEnumerable<IGameAction> IGameActionProvider.GetGameActions()
	{
		ISlotStorageProvider<IPickupable> slotStorageProvider = _raycaster.CurrentObject as ISlotStorageProvider<IPickupable>;
		if (!slotStorageProvider.IsNullOrDestroyed())
		{
			ISlotStorage<IPickupable> slotStorage = slotStorageProvider.GetSlotStorage();
			if (!slotStorage.IsNullOrDestroyed() && !slotStorage.IsOpened)
			{
				return _gameActions;
			}
		}
		return null;
	}

	public void Show()
	{
		if (Game.Mode.IsClient())
		{
			ServerValidated_DoShow();
		}
		else
		{
			DoShow();
		}
	}

	private void DoShow()
	{
		ISlotStorageProvider<IPickupable> slotStorageProvider = _raycaster.CurrentObject as ISlotStorageProvider<IPickupable>;
		if (!slotStorageProvider.IsNullOrDestroyed())
		{
			ISlotStorage<IPickupable> slotStorage = slotStorageProvider.GetSlotStorage();
			if (!slotStorage.IsOpened)
			{
				DestinationStorage = _inventoryMenuPresenter.Storage;
				Show(slotStorage);
			}
		}
	}

	public void Show(ISlotStorage<IPickupable> storage)
	{
		if (_openState == OpenState.Close)
		{
			_storage = storage;
			AddRefreshCallbacks(_storage);
			_storage.ReplicatedOpen(_player.Id);
			_player.Input.SetAndCacheMapsEnabled(state: true, 1);
			_player.Input.SetAndCacheMapsEnabled(state: false, 0);
			_player.Input.SetCursorEnabled(enabled: false);
			_player.PlayerUI.Refresh(RefreshUIMode.Clear);
			IList<IList<InventoryData>> inventoryData = GetInventoryData();
			_radialPresenter.Title = _storage.Name;
			_radialPresenter.Initialize(inventoryData);
			_radialPresenter.Show();
			_openState = OpenState.Open;
		}
	}

	private void AddRefreshCallbacks(ISlotStorage<IPickupable> storage)
	{
		if (_storageRefreshCallbacksAdded)
		{
			throw new Exception("Refresh callback already added");
		}
		_storageRefreshCallbacksAdded = true;
		storage.Pushed += SlotStorage_Pushed;
		storage.Popped += SlotStorage_Popped;
	}

	private void RemoveRefreshCallbacks(ISlotStorage<IPickupable> storage)
	{
		if (_storageRefreshCallbacksAdded)
		{
			_storage.Pushed -= SlotStorage_Pushed;
			_storage.Popped -= SlotStorage_Popped;
			_storageRefreshCallbacksAdded = false;
		}
	}

	public void Close()
	{
		if (_storage.IsValid())
		{
			_storage.ReplicatedClose();
			RemoveRefreshCallbacks(_storage);
		}
		_storage = null;
		if (_openState == OpenState.Open)
		{
			_inventoryMenuPresenter.Close();
			_inventoryMenuPresenter.SuspendInputThisFrame();
		}
		DestinationStorage = null;
	}

	public void Hide()
	{
		if (_openState == OpenState.Open)
		{
			RemoveRefreshCallbacks(_storage);
			_player.Input.SetAndCacheMapsEnabled(state: false, 1);
			_player.Input.SetAndCacheMapsEnabled(state: true, 0);
			_player.Input.SetCursorEnabled(enabled: false);
			_player.PlayerUI.Refresh(RefreshUIMode.Refresh);
			_radialPresenter.Hide();
			_openState = OpenState.Close;
		}
	}

	private void Refresh()
	{
		IList<IList<InventoryData>> inventoryData = GetInventoryData();
		_radialPresenter.Title = _storage.Name;
		_radialPresenter.Initialize(inventoryData);
		_radialPresenter.Refresh();
	}

	private void SlotStorage_Pushed(IPickupable pickupable, bool success)
	{
		Refresh();
	}

	private void SlotStorage_Popped(IPickupable pickupable)
	{
		Refresh();
	}

	private void RadialPresenter_ElementClick(IRadialMenuElement<InventoryData> element)
	{
		if (element.SelectedValue == null)
		{
			return;
		}
		if (element as StorageRadialMenuElement != null)
		{
			Hide();
			_inventoryMenuPresenter.DestinationStorage = _storage;
			_inventoryMenuPresenter.Show();
			return;
		}
		IPickupable value = element.SelectedValue.Value;
		if (DestinationStorage.CanPush(value))
		{
			MPFriendlyExchange(_storage, DestinationStorage, value);
		}
	}

	private void MPFriendlyExchange(ISlotStorage<IPickupable> src, ISlotStorage<IPickupable> dst, IPickupable pickupable)
	{
		if (!pickupable.IsNullOrDestroyed() && pickupable.IsDummy() && dst.IsValid())
		{
			DummyPickupable dummyPickupable = (DummyPickupable)pickupable;
			if (Game.Mode.IsClient())
			{
				ReplicateDummyPickupableExchange replicateDummyPickupableExchange = new ReplicateDummyPickupableExchange();
				replicateDummyPickupableExchange.Player = _player;
				replicateDummyPickupableExchange.Src = src;
				replicateDummyPickupableExchange.Dst = dst;
				replicateDummyPickupableExchange.DummyPickupablePrefabId = (short)dummyPickupable.PrefabId;
				replicateDummyPickupableExchange.Post();
				return;
			}
			pickupable = dummyPickupable.GetInstance();
		}
		Exchange(_player, src, dst, pickupable);
	}

	private static void Exchange(IPlayer player, ISlotStorage<IPickupable> src, ISlotStorage<IPickupable> dst, IPickupable pickupable)
	{
		player.Holder.ReplicatedRelease(pickupable);
		src.ReplicatedPop(pickupable);
		dst.ReplicatedPush(pickupable);
	}

	private void ServerValidated_DoShow()
	{
		if (!_raycaster.IsNullOrDestroyed() && !_raycaster.CurrentObject.IsNullOrDestroyed() && !(_raycaster.CurrentObject as ISlotStorageProvider<IPickupable>).IsNullOrDestroyed())
		{
			_openState = OpenState.Reserved;
			ServerValidated_Request serverValidated_Request = new ServerValidated_Request();
			serverValidated_Request.Player = _player;
			serverValidated_Request.RaycasterCurrentObject = _raycaster.CurrentObject as ISaveablePrefab;
			serverValidated_Request.RaycasterCurrentObjectBE = _raycaster.CurrentObject.gameObject.GetComponentInParent<BoltEntity>();
			serverValidated_Request.Post();
		}
	}
}
