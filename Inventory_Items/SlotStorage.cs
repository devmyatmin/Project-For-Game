using System;
using System.Collections.Generic;
using System.Linq;
using Beam.Crafting;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UdpKit;
using UnityEngine;

namespace Beam;

[Serializable]
public class SlotStorage : ISlotStorage<IPickupable>, IMultiplayerStorage<IPickupable>, IStorage<IPickupable>, IMultiplayerStorageReference
{
	private class ReplicateStorageCommand : I_MultiplayerMessage
	{
		public enum Command
		{
			Open,
			Close,
			Select,
			Push,
			Pop
		}

		[Replicate]
		public SlotStorage Storage;

		[I_Replicate]
		public IPickupable Pickupable;

		[Replicate]
		public int OpenPlayerId;

		public Command Cmd;

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)Cmd);
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			Cmd = (Command)packet.ReadByte();
		}

		public override void OnPeer()
		{
			switch (Cmd)
			{
			case Command.Open:
				Storage.Open(OpenPlayerId);
				break;
			case Command.Close:
				Storage.Close();
				break;
			case Command.Select:
				Storage.Select(Pickupable);
				break;
			case Command.Push:
				Storage.Push(Pickupable);
				break;
			case Command.Pop:
				Storage.Pop(Pickupable);
				break;
			}
		}

		public override string ToString()
		{
			return base.ToString() + $"({Cmd},{Pickupable})";
		}
	}

	private const int DEFAULT_STACK_SIZE = 4;

	private static readonly Dictionary<InteractiveType, int> STACK_SIZES = new Dictionary<InteractiveType, int>
	{
		[InteractiveType.CONTAINER] = 1,
		[InteractiveType.CRAFTING_LEAVES] = 24,
		[InteractiveType.CRAFTING_LOG] = 1,
		[InteractiveType.TOOLS_ARROW] = 24,
		[InteractiveType.TOOLS_SPEARGUN_ARROW] = 24,
		[InteractiveType.ANIMALS_FISH] = 8,
		[InteractiveType.FOOD_MEAT] = 8,
		[InteractiveType.FOOD_FRUIT] = 8
	};

	private static readonly List<InteractiveType> INTERACTIVE_ONLY_STACK_TYPES = new List<InteractiveType>
	{
		InteractiveType.ANIMALS_FISH,
		InteractiveType.TROPHY
	};

	public MiniGuid _referenceId;

	[SerializeField]
	private int _slotCount = 3;

	[SerializeField]
	private Transform _storageContainer;

	[SerializeField]
	private bool _sort;

	[SerializeField]
	private bool _storeOtherStorage;

	private List<StorageSlot<IPickupable>> _slotData;

	private List<StorageSlot<IPickupable>> _temp;

	private int _openPlayerId = -1;

	private LoadState _loadState;

	private bool _modified;

	private StorageSlot<IPickupable> _selectedSlot;

	public MiniGuid ReferenceId
	{
		get
		{
			return _referenceId;
		}
		set
		{
			_referenceId = value;
		}
	}

	public int SlotCount => _slotCount;

	public string Name { get; set; } = "Storage";


	public bool IsOpened => _openPlayerId != -1;

	public LoadState LoadState => _loadState;

	public bool Modified => _modified;

	public event Action<IPickupable> SelectionChanged;

	public event Action<IPickupable, bool> Pushed;

	public event Action<IPickupable> Popped;

	public event Action Opened;

	public event Action Closed;

	public IEnumerable<StorageSlot<IPickupable>> GetSlots(IComparer<StorageSlot<IPickupable>> comparer = null)
	{
		for (int j = 0; j < _slotData.Count; j++)
		{
			_temp[j] = _slotData[j];
		}
		if (comparer != null)
		{
			_temp.Sort(comparer);
		}
		for (int i = 0; i < _temp.Count; i++)
		{
			yield return _temp[i];
		}
	}

	public IEnumerable<IPickupable> GetStored()
	{
		foreach (StorageSlot<IPickupable> slotDatum in _slotData)
		{
			foreach (IPickupable @object in slotDatum.Objects)
			{
				yield return @object;
			}
		}
	}

	protected virtual void OnSelectionChanged(IPickupable pickupable)
	{
		this.SelectionChanged?.Invoke(pickupable);
	}

	protected virtual void OnPushed(IPickupable pickupable, bool success)
	{
		this.Pushed?.Invoke(pickupable, success);
	}

	protected virtual void OnPopped(IPickupable pickupable)
	{
		this.Popped?.Invoke(pickupable);
	}

	protected virtual void OnOpened()
	{
		this.Opened?.Invoke();
	}

	protected virtual void OnClosed()
	{
		this.Closed?.Invoke();
	}

	public void Initialize()
	{
		_slotData = new List<StorageSlot<IPickupable>>(_slotCount);
		for (int i = 0; i < _slotCount; i++)
		{
			_slotData.Add(new StorageSlot<IPickupable>(i));
		}
		_temp = new List<StorageSlot<IPickupable>>(_slotCount);
		for (int j = 0; j < _slotCount; j++)
		{
			_temp.Add(null);
		}
	}

	public void ClearEventSubscribers()
	{
		this.SelectionChanged = null;
		this.Pushed = null;
		this.Popped = null;
		this.Opened = null;
		this.Closed = null;
	}

	public void ReserveFor(int playerId)
	{
		_openPlayerId = playerId;
	}

	public void Open(int playerId)
	{
		_openPlayerId = playerId;
		OnOpened();
	}

	public void Close()
	{
		_openPlayerId = -1;
		OnClosed();
	}

	public void OnDisconnect(PlayerDisconnectedEvent @event)
	{
		if (_openPlayerId == @event.PlayerId)
		{
			Close();
		}
	}

	public bool CanPush(IPickupable pickupable, bool notification = true)
	{
		return CanPush(pickupable, force: false, notification);
	}

	private bool CanPush(IPickupable pickupable, bool force, bool notification)
	{
		if (pickupable.IsNullOrDestroyed() || (!force && !pickupable.CanPickUp) || Has(pickupable))
		{
			return false;
		}
		if (!_storeOtherStorage && pickupable.CraftingType.InteractiveType == InteractiveType.CONTAINER)
		{
			if (notification)
			{
				OnPushed(pickupable, success: false);
			}
			return false;
		}
		if (GetSlot(pickupable.CraftingType, assign: false) == null)
		{
			if (notification)
			{
				OnPushed(pickupable, success: false);
			}
			return false;
		}
		return true;
	}

	public bool Has(IPickupable pickupable)
	{
		if (pickupable.IsNullOrDestroyed())
		{
			return false;
		}
		foreach (IPickupable item in GetStored())
		{
			if (pickupable.ReferenceId.Equals(item.ReferenceId))
			{
				return true;
			}
		}
		return false;
	}

	public void Pop(IPickupable pickupable)
	{
		StorageSlot<IPickupable> storageSlot = FindSlot(pickupable);
		if (storageSlot != null)
		{
			pickupable.Release();
			storageSlot.Objects.Remove(pickupable);
			if (storageSlot.Objects.Count == 0)
			{
				storageSlot.CraftingType = new CraftingType(AttributeType.None, InteractiveType.None);
				SortSlots();
			}
			OnPopped(pickupable);
			_modified = true;
		}
	}

	public void PopNext(IPickupable pickupable)
	{
		if (_selectedSlot == null || _selectedSlot.Objects.Count == 0)
		{
			StorageSlot<IPickupable> storageSlot = FindSlot(pickupable.CraftingType);
			if (storageSlot == null)
			{
				storageSlot = FindSlot(pickupable.CraftingType.InteractiveType, AttributeType.None);
			}
			if (storageSlot != null)
			{
				_selectedSlot = storageSlot;
			}
		}
		IPickupable pickupable2 = null;
		StorageSlot<IPickupable> selectedSlot = _selectedSlot;
		if (selectedSlot != null && selectedSlot.Objects.Count > 0)
		{
			pickupable2 = _selectedSlot.Objects[0];
		}
		OnSelectionChanged(pickupable2);
	}

	public bool Push(IPickupable pickupable)
	{
		return Push(pickupable, force: false);
	}

	private bool Push(IPickupable pickupable, bool force)
	{
		if (CanPush(pickupable, force, notification: true))
		{
			StorageSlot<IPickupable> slot = GetSlot(pickupable.CraftingType);
			if (slot != null)
			{
				pickupable.transform.CheckForProjectilesInChildren();
				pickupable.transform.parent = null;
				pickupable.TransformParent = _storageContainer;
				pickupable.Parent();
				pickupable.transform.localPosition = Vector3.zero;
				pickupable.transform.localRotation = Quaternion.identity;
				pickupable.transform.localScale = Vector3.one;
				_modified = true;
				slot.Objects.Add(pickupable);
				OnPushed(pickupable, success: true);
				pickupable.Store();
				return true;
			}
		}
		return false;
	}

	public void Select(IPickupable pickupable)
	{
		_selectedSlot = _slotData.FirstOrDefault((StorageSlot<IPickupable> slot) => slot.Objects.Contains(pickupable));
		OnSelectionChanged(pickupable);
	}

	public void Select(int index)
	{
		if (index < _slotCount)
		{
			_selectedSlot = _slotData[index];
			IList<IPickupable> objects = _selectedSlot.Objects;
			if (objects != null && objects.Count > 0)
			{
				OnSelectionChanged(_selectedSlot.Objects[0]);
			}
			else
			{
				OnSelectionChanged(null);
			}
		}
	}

	private T Get<T>(Func<IPickupable, bool> predicate) where T : class, IPickupable
	{
		foreach (IPickupable item in GetStored())
		{
			if (predicate(item))
			{
				return item as T;
			}
		}
		return null;
	}

	public T Get<T>() where T : class, IPickupable
	{
		return Get<T>((IPickupable obj) => obj.GetType() == typeof(T));
	}

	public T Get<T>(CraftingType craftingType) where T : class, IPickupable
	{
		return Get<T>((IPickupable obj) => obj.CraftingType == craftingType);
	}

	public T Get<T>(MiniGuid referenceId) where T : class, IPickupable
	{
		return Get<T>((IPickupable obj) => obj.ReferenceId.Equals(referenceId));
	}

	public StorageSlot<IPickupable> FindSlot(CraftingType type)
	{
		return FindSlot(type.InteractiveType, type.AttributeType);
	}

	public StorageSlot<IPickupable> FindSlot(InteractiveType iType, AttributeType mType)
	{
		for (int i = 0; i < _slotData.Count; i++)
		{
			StorageSlot<IPickupable> storageSlot = _slotData[i];
			if (storageSlot.Objects.Count > 0)
			{
				CraftingType craftingType = storageSlot.CraftingType;
				if (CheckTypesMatch(craftingType, iType, mType))
				{
					return storageSlot;
				}
			}
		}
		return null;
	}

	public StorageSlot<IPickupable> FindSlot(IPickupable pickupable)
	{
		for (int i = 0; i < _slotData.Count; i++)
		{
			StorageSlot<IPickupable> storageSlot = _slotData[i];
			if (storageSlot.Objects.Contains(pickupable))
			{
				return storageSlot;
			}
		}
		return null;
	}

	public StorageSlot<IPickupable> GetSlot(InteractiveType interactiveType, AttributeType attributeType, bool assign = true)
	{
		for (int i = 0; i < _slotData.Count; i++)
		{
			StorageSlot<IPickupable> storageSlot = _slotData[i];
			if (storageSlot.Objects.Count > 0)
			{
				CraftingType craftingType = storageSlot.CraftingType;
				if (storageSlot.Objects.Count < GetStackSize(craftingType) && CheckTypesMatch(craftingType, interactiveType, attributeType))
				{
					return storageSlot;
				}
			}
		}
		for (int j = 0; j < _slotData.Count; j++)
		{
			StorageSlot<IPickupable> storageSlot2 = _slotData[j];
			if (storageSlot2.Objects.Count == 0)
			{
				if (assign)
				{
					storageSlot2.CraftingType = new CraftingType(attributeType, interactiveType);
				}
				return storageSlot2;
			}
		}
		return null;
	}

	public StorageSlot<IPickupable> GetSlot(CraftingType type, bool assign = true)
	{
		return GetSlot(type.InteractiveType, type.AttributeType, assign);
	}

	public void SortSlots()
	{
		if (!_sort)
		{
			return;
		}
		for (int i = 0; i < _slotCount; i++)
		{
			if (_slotData[i].Objects.Count == 0 && i + 1 < _slotCount)
			{
				StorageSlot<IPickupable> value = _slotData[i];
				_slotData[i] = _slotData[i + 1];
				_slotData[i + 1] = value;
			}
		}
	}

	private int GetStackSize(CraftingType type)
	{
		if (STACK_SIZES.TryGetValue(type.InteractiveType, out var value))
		{
			return value;
		}
		return 4;
	}

	private bool CheckTypesMatch(CraftingType craftingType, InteractiveType interactiveType, AttributeType attributeType)
	{
		if ((craftingType.AttributeType == AttributeType.None && craftingType.InteractiveType != 0) || (attributeType == AttributeType.None && interactiveType != 0) || INTERACTIVE_ONLY_STACK_TYPES.Contains(interactiveType))
		{
			if (craftingType.InteractiveType == InteractiveType.TOOLS_FISHING_SPEAR)
			{
				return interactiveType == InteractiveType.TOOLS_SPEAR;
			}
			if (interactiveType == InteractiveType.TOOLS_FISHING_SPEAR)
			{
				return craftingType.InteractiveType == InteractiveType.TOOLS_SPEAR;
			}
			return craftingType.InteractiveType == interactiveType;
		}
		if (craftingType.AttributeType == attributeType)
		{
			return craftingType.InteractiveType == interactiveType;
		}
		return false;
	}

	public void ReplicatedOpen(int playerId)
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateStorageCommand replicateStorageCommand = new ReplicateStorageCommand();
			replicateStorageCommand.Storage = this;
			replicateStorageCommand.OpenPlayerId = playerId;
			replicateStorageCommand.Cmd = ReplicateStorageCommand.Command.Open;
			replicateStorageCommand.Post();
		}
		Open(playerId);
	}

	public void ReplicatedClose()
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateStorageCommand replicateStorageCommand = new ReplicateStorageCommand();
			replicateStorageCommand.Storage = this;
			replicateStorageCommand.Cmd = ReplicateStorageCommand.Command.Close;
			replicateStorageCommand.Post();
		}
		Close();
	}

	public void ReplicatedPop(IPickupable pickupable)
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateStorageCommand replicateStorageCommand = new ReplicateStorageCommand();
			replicateStorageCommand.Storage = this;
			replicateStorageCommand.Pickupable = pickupable;
			replicateStorageCommand.Cmd = ReplicateStorageCommand.Command.Pop;
			replicateStorageCommand.Post();
		}
		Pop(pickupable);
	}

	public void ReplicatedPush(IPickupable pickupable)
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateStorageCommand replicateStorageCommand = new ReplicateStorageCommand();
			replicateStorageCommand.Storage = this;
			replicateStorageCommand.Pickupable = pickupable;
			replicateStorageCommand.Cmd = ReplicateStorageCommand.Command.Push;
			replicateStorageCommand.Post();
		}
		Push(pickupable);
	}

	public void ReplicatedSelect(IPickupable pickupable)
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateStorageCommand replicateStorageCommand = new ReplicateStorageCommand();
			replicateStorageCommand.Storage = this;
			replicateStorageCommand.Pickupable = pickupable;
			replicateStorageCommand.Cmd = ReplicateStorageCommand.Command.Select;
			replicateStorageCommand.Post();
		}
		Select(pickupable);
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		JObject jObject2 = new JObject();
		for (int i = 0; i < _slotData.Count; i++)
		{
			StorageSlot<IPickupable> storageSlot = _slotData[i];
			if (storageSlot.Objects.Count == 0)
			{
				continue;
			}
			JObject jObject3 = new JObject();
			for (int j = 0; j < storageSlot.Objects.Count; j++)
			{
				JObject jObject4 = SavingUtilities.SavePrefabToJObject(storageSlot.Objects[j]);
				if (jObject4 != null)
				{
					jObject3.Add(jObject4);
				}
				else
				{
					Debug.LogError($"Inventory::Save:: Error saving an inventory object for slot: [{i}, {j}] {storageSlot.CraftingType}");
				}
			}
			jObject2.Add(jObject3);
		}
		jObject.AddField("Slots", jObject2);
		return jObject;
	}

	public void Load(JObject data)
	{
		_loadState = LoadState.Loading;
		JObject slotsData = data.GetField("Slots");
		if (slotsData != null && !slotsData.IsNull())
		{
			int numItems = 0;
			for (int i = 0; i < slotsData.Children.Count; i++)
			{
				JObject jObject = slotsData.Children[i];
				if (jObject.IsValid() && !jObject.IsNull())
				{
					numItems += jObject.Children.Count;
				}
			}
			if (numItems == 0)
			{
				_loadState = LoadState.Loaded;
				return;
			}
			List<IPickupable> items = new List<IPickupable>(numItems);
			for (int j = 0; j < slotsData.Children.Count; j++)
			{
				JObject jObject2 = slotsData.Children[j];
				if (!jObject2.IsValid() || jObject2.IsNull())
				{
					continue;
				}
				for (int k = 0; k < jObject2.Children.Count; k++)
				{
					SavingUtilities.LoadPrefabFromJData(MultiplayerZoneSync.Usage.Storage, jObject2.Children[k], delegate(SaveablePrefab instance)
					{
						instance?.transform?.SetParent(null);
					}, delegate(SaveablePrefab instance)
					{
						IPickupable @interface = instance.GetInterface<IPickupable>();
						@interface.CanPickUp = true;
						items.Add(@interface);
						if (items.Count == numItems)
						{
							LevelLoader.CurrentLoader.DeferredLoading.Add(delegate
							{
								OrderedPush(items, slotsData);
							});
						}
					});
				}
			}
		}
		else
		{
			_loadState = LoadState.Loaded;
		}
	}

	private void OrderedPush(List<IPickupable> items, JObject slotsData)
	{
		for (int i = 0; i < slotsData.Children.Count; i++)
		{
			JObject jObject = slotsData.Children[i];
			if (!jObject.IsValid() || jObject.IsNull())
			{
				continue;
			}
			for (int j = 0; j < jObject.Children.Count; j++)
			{
				IPickupable pickupable = items.FirstOrDefault_NonAlloc((IPickupable itm, MiniGuid referenceId) => itm.ReferenceId.Equals(referenceId), Prefabs.GetReferenceId(jObject.Children[j]));
				if (pickupable.IsNullOrDestroyed())
				{
					Debug.LogError($"Slot Storage {ReferenceId} Load - Unable to find reference item {Prefabs.GetReferenceId(jObject.Children[j])}");
				}
				Push(pickupable);
			}
		}
		_loadState = LoadState.Loaded;
	}
}
