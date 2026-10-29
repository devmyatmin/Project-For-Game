using System.Collections.Generic;
using Beam.Events;
using Beam.Utilities;
using Rewired;
using UnityEngine;

namespace Beam.UI;

public class InventoryMenuPresenter : MonoBehaviour, IRadialMenuController, INeedPlayerComponent
{
	[SerializeField]
	private InventoryRadialMenuPresenter _radialPresenter;

	[SerializeField]
	private StorageMenuPresenter _storagePresenter;

	private bool _open;

	private IPlayer _player;

	private RadialMenuInput _input;

	private bool _suspendInputThisFrame;

	private bool _suspend;

	public ISlotStorage<IPickupable> Storage { get; private set; }

	public ISlotStorage<IPickupable> DestinationStorage { get; set; }

	public OpenState Open
	{
		get
		{
			if (!_open)
			{
				return OpenState.Close;
			}
			return OpenState.Open;
		}
	}

	IPlayer INeedPlayerComponent.Player => _player;

	private void Start()
	{
		_radialPresenter.Inventory = this;
		_radialPresenter.Title = "INVENTORY_MENU_BACKPACK_TITLE";
		_radialPresenter.SelectionChanged += RadialPresenter_SelectionChanged;
		_radialPresenter.ElementClick += RadialPresenter_ElementClick;
		_radialPresenter.ElementSecondaryClick += RadialPresenter_ElementSecondaryClick;
		_radialPresenter.ElementSecondaryShortPress += RadialPresenter_ElementSecondaryShortPress;
		_radialPresenter.Hide();
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.GameInput.Interacter.InteractingUpdated += Interacter_InteractingUpdated;
		_player.GameInput.Interacter.InteractingFinished += Interacter_InteractingFellThrough;
		Storage = _player.Inventory.GetSlotStorage();
		InitializeInput(_player.Input);
	}

	private void OnDestroy()
	{
		_input.OpenKeyUp -= RadialMenuInput_OpenKeyUp;
		_radialPresenter.SelectionChanged -= RadialPresenter_SelectionChanged;
		_radialPresenter.ElementClick -= RadialPresenter_ElementClick;
		_radialPresenter.ElementSecondaryClick -= RadialPresenter_ElementSecondaryClick;
		_radialPresenter.ElementSecondaryShortPress -= RadialPresenter_ElementSecondaryShortPress;
		if (!_player.IsNullOrDestroyed() && (bool)_player.GameInput && (bool)_player.GameInput.Interacter)
		{
			_player.GameInput.Interacter.InteractingUpdated -= Interacter_InteractingUpdated;
			_player.GameInput.Interacter.InteractingFinished -= Interacter_InteractingFellThrough;
		}
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
	}

	private void Interacter_InteractingUpdated(float value)
	{
		_suspend = true;
	}

	private void Interacter_InteractingFellThrough()
	{
		_suspend = false;
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
			_input.OpenKeyUp += RadialMenuInput_OpenKeyUp;
		}
		else
		{
			_input = new RockerRadialMenuInput(this, input);
			_input.OpenKeyUp += RadialMenuInput_OpenKeyUp;
		}
	}

	private void EventManager_OptionsAppliedEvent(OptionsAppliedEvent e)
	{
		InitializeInput(_player.Input);
	}

	private void Update()
	{
		if (_suspendInputThisFrame)
		{
			_suspendInputThisFrame = false;
		}
		else
		{
			if (_storagePresenter.Open.IsOpen())
			{
				return;
			}
			if (_open && _player.Statistics.IsKnockedOut)
			{
				Close(selectCurrent: false);
				Hide();
			}
			else
			{
				if (_suspend || _player.Movement.CheckVehicle())
				{
					return;
				}
				_input.CheckInput();
				if (_open)
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
		}
	}

	public void SuspendInputThisFrame()
	{
		_suspendInputThisFrame = true;
	}

	private void RadialMenuInput_OpenKeyUp()
	{
		Close();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		Close(selectCurrent: false);
		Hide();
	}

	private IList<IList<InventoryData>> GetInventoryData()
	{
		IList<IList<InventoryData>> list = new List<IList<InventoryData>>();
		foreach (StorageSlot<IPickupable> slot in Storage.GetSlots())
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

	public void Show()
	{
		if (!_player.Statistics.IsKnockedOut && !MainMenuPresenter.Instance.IsGamePaused && !_player.Movement.IsInCutscene && !_open && !_player.Input.GetButtonDown(39))
		{
			Storage.ReplicatedOpen(_player.Id);
			_player.Input.SetAndCacheMapsEnabled(state: true, 1);
			_player.Input.SetAndCacheMapsEnabled(state: false, 0);
			_player.Input.SetCursorEnabled(enabled: false);
			_player.PlayerUI.Refresh(RefreshUIMode.Clear);
			IList<IList<InventoryData>> inventoryData = GetInventoryData();
			_radialPresenter.Initialize(inventoryData);
			_radialPresenter.Show();
			_open = true;
		}
	}

	public void Close(bool selectCurrent = true)
	{
		if (_open && selectCurrent)
		{
			_radialPresenter.SelectCurrentElement();
		}
		if (Storage.IsOpened)
		{
			Storage.ReplicatedClose();
		}
		if (_open)
		{
			_storagePresenter.Close();
		}
		DestinationStorage = null;
	}

	public void Hide()
	{
		if (_open)
		{
			_player.Input.SetAndCacheMapsEnabled(state: false, 1);
			_player.Input.SetAndCacheMapsEnabled(state: true, 0);
			_player.Input.SetCursorEnabled(enabled: false);
			_player.PlayerUI.Refresh(RefreshUIMode.Refresh);
			_radialPresenter.Hide();
			_open = false;
		}
	}

	private void RadialPresenter_SelectionChanged(IRadialMenuElement<InventoryData> element)
	{
		if (!(element as StorageRadialMenuElement) && element.SelectedValue != null)
		{
			IPickupable value = element.SelectedValue.Value;
			Storage.ReplicatedSelect(value);
		}
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
			_storagePresenter.DestinationStorage = Storage;
			_storagePresenter.Show(DestinationStorage);
			return;
		}
		IPickupable value = element.SelectedValue.Value;
		if (!DestinationStorage.IsNullOrDestroyed())
		{
			if (DestinationStorage.CanPush(value))
			{
				_player.Holder.ReplicatedRelease(value);
				DestinationStorage.ReplicatedPush(value);
				IList<IList<InventoryData>> inventoryData = GetInventoryData();
				_radialPresenter.Initialize(inventoryData);
				_radialPresenter.Refresh();
			}
		}
		else
		{
			Storage.ReplicatedSelect(value);
			Hide();
		}
	}

	private void RadialPresenter_ElementSecondaryClick(IRadialMenuElement<InventoryData> element)
	{
	}

	private void RadialPresenter_ElementSecondaryShortPress(IRadialMenuElement<InventoryData> element)
	{
		IPickupable pickupable = element.SelectedValue?.Value;
		if (!pickupable.IsNullOrDestroyed())
		{
			_player.Holder.ReplicatedRelease(pickupable);
			IList<IList<InventoryData>> inventoryData = GetInventoryData();
			_radialPresenter.Initialize(inventoryData);
			_radialPresenter.Refresh();
		}
	}
}
