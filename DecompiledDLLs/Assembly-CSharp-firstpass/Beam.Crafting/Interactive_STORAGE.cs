using System;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using UnityEngine;

namespace Beam.Crafting;

public class Interactive_STORAGE : ConstructionObject, IAttachable, IPickupable, IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial, IPhysical, ISlotStorageProvider<IPickupable>, IMultiplayerState<IInteractive_STORAGEState>
{
	private class ReplicateOnSlotStorageOpen : MultiplayerMessage
	{
		[Replicate]
		public Interactive_STORAGE Storage;

		public override void OnPeer()
		{
			Storage.OnSlotStorageOpen();
		}
	}

	[SerializeField]
	private GameObject _label;

	[Header("ATTACHING")]
	[SerializeField]
	private bool _canAttach;

	private bool _isAttached;

	private IAttachingAnchor _attachedAnchor;

	[SerializeField]
	private StaticAttachingAnchor _attachingAnchor;

	[SerializeField]
	private SlotStorage _slotStorage;

	[SerializeField]
	private bool _spawnItemLoot;

	[SerializeField]
	private GameObject[] _startingLoot;

	[SerializeField]
	private LootSpawn.LootItem[] _itemLoot;

	private bool _cacheCanDrag;

	private bool _cacheCanPickup;

	private bool _shouldCache;

	private JObject _storageData;

	private bool _created;

	public bool CanAttach
	{
		get
		{
			return _canAttach;
		}
		set
		{
			_canAttach = value;
		}
	}

	public bool IsAttached => _isAttached;

	public IAttachingAnchor AttachingAnchor => _attachingAnchor;

	protected bool LootSpawned
	{
		get
		{
			return MultiplayerState.LootSpawned;
		}
		set
		{
			if (((EntityBehaviour)this).entity.IsOwner)
			{
				MultiplayerState.LootSpawned = value;
			}
		}
	}

	public IInteractive_STORAGEState MultiplayerState => ((MultiplayerBehaviour)this).GetMultiplayerState<IInteractive_STORAGEState>();

	protected bool HasStoredItems
	{
		get
		{
			if (!_created && _spawnItemLoot && !LootSpawned)
			{
				return true;
			}
			if (_storageData != null && !_storageData.IsNull() && !_slotStorage.LoadState.IsLoaded())
			{
				return true;
			}
			return Beam.Utilities.ExtensionMethods.Any_NonAlloc(_slotStorage.GetStored());
		}
	}

	public event AttachableEventHandler<IAttachable> DetachedEvent;

	public event Action<Interactive_STORAGE> Opened;

	public event Action<string> StorageNameChanged;

	public ISlotStorage<IPickupable> GetSlotStorage()
	{
		return _slotStorage;
	}

	protected virtual void OnDetached(IAttachable sender)
	{
		DetachedEvent?.Invoke(sender);
		DetachedEvent = null;
	}

	protected virtual void OnOpened(Interactive_STORAGE storage)
	{
		Opened?.Invoke(this);
	}

	protected virtual void OnStorageNameChanged()
	{
		StorageNameChanged?.Invoke(DisplayName);
	}

	protected override void OnDisplayNameChanged()
	{
		base.OnDisplayNameChanged();
		try
		{
			_label.SetActive(HasCustomDisplayName());
		}
		catch (Exception)
		{
		}
		_slotStorage.Name = DisplayName;
		OnStorageNameChanged();
	}

	protected override void Awake()
	{
		base.Awake();
		_cacheCanDrag = CanDrag;
		_cacheCanPickup = CanPickUp;
		_shouldCache = true;
		_slotStorage.Name = DisplayName;
		_slotStorage.Opened += SlotStorage_Opened;
		_slotStorage.Closed += SlotStorage_Closed;
		_slotStorage.Initialize();
		StorageRegistry.Link(_slotStorage);
	}

	public override void Attached()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		base.Attached();
		SlotStorage slotStorage = _slotStorage;
		NetworkId networkId = ((EntityBehaviour)this).entity.NetworkId;
		slotStorage.ReferenceId = new MiniGuid(networkId.PackedValue);
	}

	public override void Damage(float damage, IBase damager)
	{
		if (HasStoredItems)
		{
			IPickupable pickupable = damager as IPickupable;
			if (!BeamTeam.IsNullOrDestroyed(pickupable))
			{
				LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
				localizedNotification.Priority = NotificationPriority.Low;
				localizedNotification.Duration = 4f;
				localizedNotification.PlayerId = pickupable.PreviousOwnerId;
				localizedNotification.MessageText.SetTerm("STORAGE_NTF_CANT_DESTROY_ITEMS_INSIDE");
				localizedNotification.Raise();
				return;
			}
		}
		base.Damage(damage, damager);
	}

	public override void OnDestroy()
	{
		try
		{
			Detach();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		_slotStorage.ClearEventSubscribers();
		StorageNameChanged = null;
		Opened = null;
		DetachedEvent = null;
		StorageRegistry.Unlink(_slotStorage);
		base.OnDestroy();
	}

	public override void OnCrafted(IPlayer player)
	{
		_created = true;
		base.OnCrafted(player);
	}

	private void SlotStorage_Opened()
	{
		if (_shouldCache)
		{
			_cacheCanDrag = CanDrag;
			_cacheCanPickup = CanPickUp;
			CanDrag = false;
			CanPickUp = false;
			_shouldCache = false;
		}
		EventManager.RaiseEvent(new AddStatsEvent(0, new Stats
		{
			ItemsLooted = 1
		}));
		ReplicatedOnSlotStorageOpen();
	}

	private void SlotStorage_Closed()
	{
		CanDrag = _cacheCanDrag;
		CanPickUp = _cacheCanPickup;
		_shouldCache = true;
	}

	private void OnSlotStorageOpen()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (_storageData != null && !_storageData.IsNull() && !_slotStorage.LoadState.IsLoaded())
		{
			if (_slotStorage.LoadState.IsIdle())
			{
				StorageUtilities.LoadStorage(_slotStorage, _storageData);
			}
		}
		else if (MPExtensions.IsMaster(Game.Mode) && !_created && _spawnItemLoot && !LootSpawned)
		{
			LootSpawned = true;
			if (_startingLoot.Length != 0)
			{
				for (int i = 0; i < _startingLoot.Length; i++)
				{
					InteractiveObject interactiveObject = MultiplayerMng.Instantiate<InteractiveObject>(_startingLoot[i].GetComponent<ISaveablePrefab>().PrefabId, MiniGuid.New());
					if ((Object)(object)interactiveObject != (Object)null && interactiveObject.CanPickUp)
					{
						_slotStorage.ReplicatedPush(interactiveObject);
						continue;
					}
					Debug.LogWarning((object)("Interactive Storage :: Failed to generate Item - " + ((Object)((Component)interactiveObject).gameObject).name));
					MultiplayerMng.Destroy(interactiveObject);
				}
			}
			if (_itemLoot.Length != 0)
			{
				for (int j = 0; j < _slotStorage.SlotCount; j++)
				{
					float value = Random.value;
					GameObject item = LootSpawn.ChooseItem(_itemLoot, value).Item;
					if ((Object)(object)item != (Object)null)
					{
						InteractiveObject interactiveObject2 = MultiplayerMng.Instantiate<InteractiveObject>(item.GetComponent<ISaveablePrefab>().PrefabId, MiniGuid.New());
						if ((Object)(object)interactiveObject2 != (Object)null && interactiveObject2.CanPickUp)
						{
							_slotStorage.ReplicatedPush(interactiveObject2);
							continue;
						}
						Debug.LogWarning((object)("Interactive Storage :: Failed to generate Item - " + ((Object)((Component)interactiveObject2).gameObject).name));
						MultiplayerMng.Destroy(interactiveObject2);
					}
				}
			}
		}
		OnOpened(this);
	}

	public override bool Store()
	{
		Detach();
		return base.Store();
	}

	public void Attach(IAttachingAnchor anchor)
	{
		if (!_isAttached)
		{
			_isAttached = true;
			DisablePhysics();
			CanDrag = false;
		}
	}

	public void Detach()
	{
		if (_isAttached)
		{
			_isAttached = false;
			OnDetached(this);
			CanDrag = true;
		}
	}

	private void ReplicatedOnSlotStorageOpen()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateOnSlotStorageOpen replicateOnSlotStorageOpen = new ReplicateOnSlotStorageOpen();
			replicateOnSlotStorageOpen.Storage = this;
			replicateOnSlotStorageOpen.Post();
		}
		OnSlotStorageOpen();
	}

	public override JObject Save()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = base.Save();
		JObject jObject2 = (_slotStorage.Modified ? _slotStorage.Save() : _storageData);
		if (jObject2 != null)
		{
			jObject.AddField("Storage", jObject2);
		}
		jObject.AddField("Crafted", _created);
		if (MPExtensions.IsServer(Game.Mode) && LevelLoader.IsServerJoinInProgress && Beam.Utilities.ExtensionMethods.Any_NonAlloc(_slotStorage.GetStored()))
		{
			jObject.AddField("serverJIPHasItems", instance: true);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data);
		_storageData = data.GetField("Storage");
		if (_storageData != null)
		{
			LootSpawned = true;
		}
		JObject field = data.GetField("Crafted");
		if (field != null)
		{
			_created = field.GetValue<bool>();
		}
		if (MPExtensions.IsClient(Game.Mode) && MPExtensions.IsGame(MultiplayerMng.ServerGameState) && BeamTeam.IsValid(_storageData) && !_storageData.IsNull() && _slotStorage.LoadState.IsIdle() && BeamTeam.IsValid(data.GetField("serverJIPHasItems")))
		{
			_slotStorage.Load(_storageData);
		}
	}
}
