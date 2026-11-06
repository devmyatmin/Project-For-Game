using Beam.Language;
using Beam.Serialization.Json;
using Beam.UI;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Inventory : MonoBehaviour, ISlotStorageProvider<IPickupable>, INeedPlayerComponent, ISaveable
{
	[SerializeField]
	private SlotStorage _slotStorage;

	private IPlayer _player;

	private IDynamicParentProvider _dynamicParenter;

	private bool _firstPush = true;

	public bool Loaded => _slotStorage.LoadState.IsLoaded();

	IPlayer INeedPlayerComponent.Player => _player;

	public ISlotStorage<IPickupable> GetSlotStorage()
	{
		return _slotStorage;
	}

	private void Awake()
	{
		_slotStorage.Name = "INVENTORY_MENU_BACKPACK_TITLE";
		_slotStorage.Initialize();
		_slotStorage.Pushed += SlotStorage_Pushed;
		_slotStorage.Popped += SlotStorage_Popped;
	}

	private void Start()
	{
		_dynamicParenter = Object.FindObjectOfType<StrandedWorld>();
		StorageRegistry.Link(_slotStorage);
	}

	private void OnDestroy()
	{
		_slotStorage.Pushed -= SlotStorage_Pushed;
		_slotStorage.Popped -= SlotStorage_Popped;
		StorageRegistry.Unlink(_slotStorage);
	}

	public void SetPlayer(IPlayer player)
	{
		_slotStorage.ReferenceId = MiniGuid.PLAYER_STORAGE(player);
		_player = player;
	}

	private void SlotStorage_Pushed(IPickupable pickupable, bool success)
	{
		if (!success)
		{
			LocalizedNotification.Post(_player, NotificationPriority.Low, 4f, "INVENTORY_MENU_NTF_NOT_ENOUGH_ROOM_DESC");
			return;
		}
		pickupable.SetOwner(_player);
		pickupable.Hold(holding: false);
		if (_firstPush)
		{
			PlatformLocalizedText platformLocalizedText = new PlatformLocalizedText();
			platformLocalizedText.SetTerm("INVENTORY_MENU_NTF_ITEM_STORED_DESC");
			platformLocalizedText.SetArgs(pickupable.DisplayName);
			platformLocalizedText.SetActions(12);
			LocalizedNotification.Post(_player, NotificationPriority.Immediate, 8f, platformLocalizedText);
			_firstPush = false;
		}
	}

	private void SlotStorage_Popped(IPickupable pickupable)
	{
		pickupable.transform.parent = null;
		pickupable.TransformParent = _dynamicParenter?.GetParent();
		pickupable.Parent();
		pickupable.Hold(holding: false);
		pickupable.SetOwner(null);
	}

	public JObject Save()
	{
		return _slotStorage.Save();
	}

	public void Load(JObject data)
	{
		_slotStorage.Load(data);
	}
}
