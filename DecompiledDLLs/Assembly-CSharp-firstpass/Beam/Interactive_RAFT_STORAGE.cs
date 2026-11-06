using Beam.Crafting;
using Beam.Language;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using UnityEngine;

namespace Beam;

public class Interactive_RAFT_STORAGE : MultiplayerBehaviour, ISelectable, IBase, IHasCraftingType, ISlotStorageProvider<IPickupable>, ISaveable, IMultiplayerState<IInteractiveObject_LIFERAFTState>
{
	private class ReplicateOnSlotStorageOpen : MultiplayerMessage
	{
		[Replicate]
		public BoltEntity Entity;

		public override void OnPeer()
		{
			((Component)Entity).GetComponentInChildren<Interactive_RAFT_STORAGE>().OnSlotStorageOpen();
		}
	}

	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _displayName;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	[SerializeField]
	private SlotStorage _slotStorage;

	[SerializeField]
	private InteractiveObject[] _startingPrefabs;

	private JObject _storageData;

	public string DisplayName => _displayName;

	public Selecting Selecting => _selecting;

	public Transform TransformParent { get; set; }

	public CraftingType CraftingType => new CraftingType(AttributeType.None, InteractiveType.CONTAINER);

	public RaycastMode RaycastMode => RaycastMode.Collider;

	public bool CanParent { get; set; }

	public bool IsDamageable => false;

	public IInteractiveObject_LIFERAFTState MultiplayerState => base.GetMultiplayerState<IInteractiveObject_LIFERAFTState>();

	private bool CreatedItems
	{
		get
		{
			return MultiplayerState.CreatedItems;
		}
		set
		{
			if (((EntityBehaviour)this).entity.IsOwner)
			{
				MultiplayerState.CreatedItems = value;
			}
		}
	}

	public ISlotStorage<IPickupable> GetSlotStorage()
	{
		return _slotStorage;
	}

	private void Awake()
	{
		_slotStorage.Name = DisplayName;
		_slotStorage.Opened += SlotStorage_Opened;
		_slotStorage.Initialize();
		_slotStorage.ReferenceId = MiniGuid.VEHICLE_LIFERAFT_STORAGE;
		StorageRegistry.Link(_slotStorage);
	}

	private void OnDestroy()
	{
		_slotStorage.ClearEventSubscribers();
		StorageRegistry.Unlink(_slotStorage);
	}

	private void SlotStorage_Opened()
	{
		ReplicatedOnSlotStorageOpen();
	}

	private void OnSlotStorageOpen()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (_storageData != null && !_storageData.IsNull() && !_slotStorage.LoadState.IsLoaded())
		{
			if (_slotStorage.LoadState.IsIdle())
			{
				StorageUtilities.LoadStorage(_slotStorage, _storageData);
			}
		}
		else if (MPExtensions.IsMaster(Game.Mode) && !CreatedItems)
		{
			for (int i = 0; i < _startingPrefabs.Length; i++)
			{
				InteractiveObject pickupable = MultiplayerMng.Instantiate<InteractiveObject>(_startingPrefabs[i].PrefabId, MiniGuid.New());
				_slotStorage.ReplicatedPush(pickupable);
			}
			CreatedItems = true;
		}
	}

	public void Damage(float damage, IBase damager1)
	{
	}

	public void Destroy()
	{
	}

	public void Parent()
	{
	}

	private void ReplicatedOnSlotStorageOpen()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateOnSlotStorageOpen replicateOnSlotStorageOpen = new ReplicateOnSlotStorageOpen();
			replicateOnSlotStorageOpen.Entity = ((EntityBehaviour)this).entity;
			replicateOnSlotStorageOpen.Post();
		}
		OnSlotStorageOpen();
	}

	public JObject Save()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = new JObject();
		JObject jObject2 = (_slotStorage.Modified ? _slotStorage.Save() : _storageData);
		if (jObject2 != null && !jObject2.IsNull())
		{
			jObject.AddField("Storage", jObject2);
		}
		if (MPExtensions.IsServer(Game.Mode) && LevelLoader.IsServerJoinInProgress && Beam.Utilities.ExtensionMethods.Any_NonAlloc(_slotStorage.GetStored()))
		{
			jObject.AddField("serverJIPHasItems", instance: true);
		}
		return jObject;
	}

	public void Load(JObject data)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		_storageData = data.GetField("Storage");
		if (_storageData != null)
		{
			CreatedItems = true;
		}
		if (MPExtensions.IsClient(Game.Mode) && MPExtensions.IsGame(MultiplayerMng.ServerGameState) && BeamTeam.IsValid(_storageData) && !_storageData.IsNull() && _slotStorage.LoadState.IsIdle() && BeamTeam.IsValid(data.GetField("serverJIPHasItems")))
		{
			_slotStorage.Load(_storageData);
		}
	}
}
