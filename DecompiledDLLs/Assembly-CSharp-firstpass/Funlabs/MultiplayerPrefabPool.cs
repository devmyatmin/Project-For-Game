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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = LoadPrefab(prefabId);
		Transform val2 = null;
		if (MPExtensions.IsGame(Game.State) && MPExtensions.IsClient(Game.Mode) && !IsLocal && !(prefabId == BoltPrefabs.Peer) && !(prefabId == BoltPrefabs.Player))
		{
			val2 = _receivedEntities;
		}
		return Object.Instantiate<GameObject>(val, position, rotation, val2);
	}

	public void Destroy(GameObject gameObject)
	{
		Object.Destroy((Object)(object)gameObject);
	}

	public GameObject LoadPrefab(PrefabId prefabId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return PrefabDatabase.Find(prefabId);
	}
}
