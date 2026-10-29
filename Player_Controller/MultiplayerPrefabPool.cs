using Beam;
using Photon.Bolt;
using UnityEngine;

namespace Funlabs;

public class MultiplayerPrefabPool : IPrefabPool
{
	private Transform _receivedEntities;

	public bool IsLocal { get; set; }

	public MultiplayerPrefabPool(Transform receivedEntities)
	{
		_receivedEntities = receivedEntities;
	}

	public GameObject Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation)
	{
		GameObject original = LoadPrefab(prefabId);
		Transform parent = null;
		if (Game.State.IsGame() && Game.Mode.IsClient() && !IsLocal && !(prefabId == BoltPrefabs.Peer) && !(prefabId == BoltPrefabs.Player))
		{
			parent = _receivedEntities;
		}
		return Object.Instantiate(original, position, rotation, parent);
	}

	public void Destroy(GameObject gameObject)
	{
		Object.Destroy(gameObject);
	}

	public GameObject LoadPrefab(PrefabId prefabId)
	{
		return PrefabDatabase.Find(prefabId);
	}
}
