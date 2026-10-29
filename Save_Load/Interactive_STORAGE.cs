using System;
using System.Runtime.CompilerServices;
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
			if (base.entity.IsOwner)
			{
				MultiplayerState.LootSpawned = value;
			}
		}
	}

	public IInteractive_STORAGEState MultiplayerState => GetMultiplayerState<IInteractive_STORAGEState>();

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
			return _slotStorage.GetStored().Any_NonAlloc();
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
		this.DetachedEvent?.Invoke(sender);
		this.DetachedEvent = null;
	}

	protected virtual void OnOpened(Interactive_STORAGE storage)
	{
		this.Opened?.Invoke(this);
	}

	protected virtual void OnStorageNameChanged()
	{
		this.StorageNameChanged?.Invoke(base.DisplayName);
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
		_slotStorage.Name = base.DisplayName;
		OnStorageNameChanged();
	}

	protected override void Awake()
	{
		base.Awake();
		_cacheCanDrag = CanDrag;
		_cacheCanPickup = CanPickUp;
		_shouldCache = true;
		_slotStorage.Name = base.DisplayName;
		_slotStorage.Opened += SlotStorage_Opened;
		_slotStorage.Closed += SlotStorage_Closed;
		_slotStorage.Initialize();
		StorageRegistry.Link(_slotStorage);
	}

	public override void Attached()
	{
		base.Attached();
		_slotStorage.ReferenceId = new MiniGuid(base.entity.NetworkId.PackedValue);
	}

	public override void Damage(float damage, IBase damager)
	{
		if (HasStoredItems)
		{
			IPickupable pickupable = damager as IPickupable;
			if (!pickupable.IsNullOrDestroyed())
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
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		_slotStorage.ClearEventSubscribers();
		this.StorageNameChanged = null;
		this.Opened = null;
		this.DetachedEvent = null;
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
		if (_storageData != null && !_storageData.IsNull() && !_slotStorage.LoadState.IsLoaded())
		{
			if (_slotStorage.LoadState.IsIdle())
			{
				StorageUtilities.LoadStorage(_slotStorage, _storageData);
			}
		}
		else if (Game.Mode.IsMaster() && !_created && _spawnItemLoot && !LootSpawned)
		{
			LootSpawned = true;
			if (_startingLoot.Length != 0)
			{
				for (int i = 0; i < _startingLoot.Length; i++)
				{
					InteractiveObject interactiveObject = MultiplayerMng.Instantiate<InteractiveObject>(_startingLoot[i].GetComponent<ISaveablePrefab>().PrefabId, MiniGuid.New());
					if (interactiveObject != null && interactiveObject.CanPickUp)
					{
						_slotStorage.ReplicatedPush(interactiveObject);
						continue;
					}
					Debug.LogWarning("Interactive Storage :: Failed to generate Item - " + interactiveObject.gameObject.name);
					MultiplayerMng.Destroy(interactiveObject);
				}
			}
			if (_itemLoot.Length != 0)
			{
				for (int j = 0; j < _slotStorage.SlotCount; j++)
				{
					float value = UnityEngine.Random.value;
					GameObject item = LootSpawn.ChooseItem(_itemLoot, value).Item;
					if (item != null)
					{
						InteractiveObject interactiveObject2 = MultiplayerMng.Instantiate<InteractiveObject>(item.GetComponent<ISaveablePrefab>().PrefabId, MiniGuid.New());
						if (interactiveObject2 != null && interactiveObject2.CanPickUp)
						{
							_slotStorage.ReplicatedPush(interactiveObject2);
							continue;
						}
						Debug.LogWarning("Interactive Storage :: Failed to generate Item - " + interactiveObject2.gameObject.name);
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
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateOnSlotStorageOpen replicateOnSlotStorageOpen = new ReplicateOnSlotStorageOpen();
			replicateOnSlotStorageOpen.Storage = this;
			replicateOnSlotStorageOpen.Post();
		}
		OnSlotStorageOpen();
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject jObject2 = (_slotStorage.Modified ? _slotStorage.Save() : _storageData);
		if (jObject2 != null)
		{
			jObject.AddField("Storage", jObject2);
		}
		jObject.AddField("Crafted", _created);
		if (Game.Mode.IsServer() && LevelLoader.IsServerJoinInProgress && _slotStorage.GetStored().Any_NonAlloc())
		{
			jObject.AddField("serverJIPHasItems", instance: true);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
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
		if (Game.Mode.IsClient() && MultiplayerMng.ServerGameState.IsGame() && _storageData.IsValid() && !_storageData.IsNull() && _slotStorage.LoadState.IsIdle() && data.GetField("serverJIPHasItems").IsValid())
		{
			_slotStorage.Load(_storageData);
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
