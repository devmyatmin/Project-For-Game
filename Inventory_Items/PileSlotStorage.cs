using System;
using System.Collections.Generic;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UdpKit;
using UnityEngine;

namespace Beam.Crafting;

[Serializable]
public class PileSlotStorage : ISlotStorage<IPickupable>, IMultiplayerStorage<IPickupable>, IStorage<IPickupable>, IMultiplayerStorageReference, ISaveable, IDisposable
{
	public enum ECanPushResult
	{
		Success,
		NullOrDestroyed,
		NotPileType,
		NotMatchingType,
		Full
	}

	public enum EReplicateStorageCommand
	{
		Open,
		Close,
		Select,
		Push,
		Pop
	}

	private class ReplicateStorageCommand : I_MultiplayerMessage
	{
		[Replicate]
		public IMultiplayerStorage<IPickupable> Storage;

		[I_Replicate]
		public IPickupable Obj;

		public EReplicateStorageCommand Cmd;

		[Replicate]
		public int OpenPlayerId;

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)Cmd);
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			Cmd = (EReplicateStorageCommand)packet.ReadByte();
		}

		public override void OnPeer()
		{
			switch (Cmd)
			{
			case EReplicateStorageCommand.Open:
				Storage.Open(OpenPlayerId);
				break;
			case EReplicateStorageCommand.Close:
				Storage.Close();
				break;
			case EReplicateStorageCommand.Select:
				Storage.Select(Obj);
				break;
			case EReplicateStorageCommand.Push:
				Storage.Push(Obj);
				break;
			case EReplicateStorageCommand.Pop:
				Storage.Pop(Obj);
				break;
			}
		}

		public override string ToString()
		{
			return base.ToString() + $"({Cmd},{Obj})";
		}
	}

	[SerializeField]
	private PileData _pileData;

	public MiniGuid _referenceId;

	private int _openPlayerId = -1;

	private DummyPickupable _handle;

	private LoadState _loadState;

	private StorageSlot<IPickupable> _storageSlot = new StorageSlot<IPickupable>(0);

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

	public int SlotCount => 1;

	public string Name { get; set; } = "Storage";


	public bool IsOpened => _openPlayerId != -1;

	public LoadState LoadState => _loadState;

	public StorageSlot<IPickupable> StorageSlot => _storageSlot;

	public event Action<IPickupable> SelectionChanged;

	public event Action<IPickupable> Popped;

	public event Action<IPickupable, bool> Pushed;

	public event Action Opened;

	public event Action Closed;

	public void Dispose()
	{
		this.SelectionChanged = null;
		this.Pushed = null;
		this.Popped = null;
		this.Opened = null;
		this.Closed = null;
		_handle?.Dispose();
		_handle = null;
	}

	public void ReserveFor(int playerId)
	{
		_openPlayerId = playerId;
	}

	public void Open(int playerId)
	{
		_openPlayerId = playerId;
		this.Opened?.Invoke();
	}

	public void Close()
	{
		_openPlayerId = -1;
		this.Closed?.Invoke();
	}

	public void OnDisconnect(PlayerDisconnectedEvent @event)
	{
		if (_openPlayerId == @event.PlayerId)
		{
			Close();
		}
	}

	public IEnumerable<StorageSlot<IPickupable>> GetSlots(IComparer<StorageSlot<IPickupable>> comparer = null)
	{
		yield return _storageSlot;
	}

	public IEnumerable<IPickupable> GetStored()
	{
		return _storageSlot.Objects;
	}

	public int GetStoredCount()
	{
		return _storageSlot.Objects.Count;
	}

	public bool Has(IPickupable obj)
	{
		if (obj.CraftingType == _storageSlot.CraftingType)
		{
			return _storageSlot.Objects.Count > 0;
		}
		return false;
	}

	public void Pop(IPickupable obj)
	{
		if (_storageSlot.Objects.Count > 0)
		{
			_storageSlot.Objects.RemoveAt(_storageSlot.Objects.Count - 1);
		}
		if (_storageSlot.Objects.Count == 0)
		{
			_handle.Dispose();
			_handle = null;
			_storageSlot.CraftingType = CraftingType.Empty;
		}
		this.Popped?.Invoke(obj);
	}

	public void PopNext(IPickupable obj)
	{
	}

	public bool CanPush(IPickupable obj, bool notification = true)
	{
		ECanPushResult eCanPushResult = ECanPushResult.Success;
		if (obj.IsNullOrDestroyed())
		{
			eCanPushResult = ECanPushResult.NullOrDestroyed;
		}
		else if (!obj.CraftingType.IsPileType())
		{
			eCanPushResult = ECanPushResult.NotPileType;
		}
		else if (_storageSlot.CraftingType != CraftingType.Empty && obj.CraftingType != _storageSlot.CraftingType)
		{
			eCanPushResult = ECanPushResult.NotMatchingType;
		}
		else if (_storageSlot.Objects.Count >= _pileData.GetPileCapacity(obj.CraftingType))
		{
			eCanPushResult = ECanPushResult.Full;
		}
		if (eCanPushResult != 0)
		{
			this.Pushed?.Invoke(obj, arg2: false);
		}
		return eCanPushResult == ECanPushResult.Success;
	}

	public bool Push(IPickupable obj)
	{
		if (!CanPush(obj))
		{
			return false;
		}
		if (_handle == null)
		{
			_handle = new DummyPickupable(obj);
			_handle.OnConsumed = Proxy_OnConsumedCallback;
		}
		_storageSlot.Objects.Add(_handle);
		if (_storageSlot.CraftingType == CraftingType.Empty)
		{
			_storageSlot.CraftingType = obj.CraftingType;
		}
		this.Pushed?.Invoke(obj, arg2: true);
		if (obj.gameObject != null && !obj.gameObject.IsPrefab())
		{
			MultiplayerMng.Destroy((SaveablePrefab)obj, DestroyDelay.LATE);
		}
		return true;
	}

	private void Proxy_OnConsumedCallback(DummyPickupable proxy)
	{
		Pop(null);
	}

	public void Select(IPickupable obj)
	{
		this.SelectionChanged?.Invoke(obj);
	}

	public T Get<T>(CraftingType craftingType) where T : class, IPickupable
	{
		throw new NotImplementedException();
	}

	public T Get<T>(MiniGuid referenceId) where T : class, IPickupable
	{
		throw new NotImplementedException();
	}

	public T Get<T>() where T : class, IPickupable
	{
		throw new NotImplementedException();
	}

	public void ReplicatedOpen(int playerId)
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateStorageCommand replicateStorageCommand = new ReplicateStorageCommand();
			replicateStorageCommand.Storage = this;
			replicateStorageCommand.OpenPlayerId = playerId;
			replicateStorageCommand.Cmd = EReplicateStorageCommand.Open;
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
			replicateStorageCommand.Cmd = EReplicateStorageCommand.Close;
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
			replicateStorageCommand.Obj = pickupable;
			replicateStorageCommand.Cmd = EReplicateStorageCommand.Pop;
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
			replicateStorageCommand.Obj = pickupable;
			replicateStorageCommand.Cmd = EReplicateStorageCommand.Push;
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
			replicateStorageCommand.Obj = pickupable;
			replicateStorageCommand.Cmd = EReplicateStorageCommand.Select;
			replicateStorageCommand.Post();
		}
		Select(pickupable);
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Id", _handle?.PrefabId ?? 0);
		jObject.AddField("Count", _storageSlot.Objects.Count);
		return jObject;
	}

	public void Load(JObject data)
	{
		uint value = data.GetField("Id").GetValue<uint>();
		if (value == 0)
		{
			return;
		}
		int value2 = data.GetField("Count").GetValue<int>();
		if (value2 != 0)
		{
			IPickupable @interface = Prefabs.GetPrefab(value).GetInterface<IPickupable>();
			for (int i = 0; i < value2; i++)
			{
				Push(@interface);
			}
		}
	}
}
