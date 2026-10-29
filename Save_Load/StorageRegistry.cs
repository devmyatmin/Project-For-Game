using System.Collections.Generic;
using Beam.Events;
using Beam.Utilities;
using Funlabs;

namespace Beam;

public static class StorageRegistry
{
	private static List<IMultiplayerStorageReference> _allStorages = new List<IMultiplayerStorageReference>();

	public static void Link(IMultiplayerStorageReference slotStorage)
	{
		if (!_allStorages.Contains(slotStorage))
		{
			_allStorages.Add(slotStorage);
			EventManager.AddListener<PlayerDisconnectedEvent>(slotStorage.OnDisconnect);
		}
	}

	public static void Unlink(IMultiplayerStorageReference slotStorage)
	{
		_allStorages.Remove(slotStorage);
		EventManager.RemoveListener<PlayerDisconnectedEvent>(slotStorage.OnDisconnect);
	}

	public static IMultiplayerStorageReference Get(MiniGuid referenceId)
	{
		return _allStorages.FirstOrDefault_NonAlloc((IMultiplayerStorageReference slotStorage, MiniGuid id) => slotStorage.ReferenceId.Equals(id), referenceId);
	}
}
