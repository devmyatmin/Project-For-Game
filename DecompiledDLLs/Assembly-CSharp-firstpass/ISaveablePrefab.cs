using Photon.Bolt;
using UnityEngine;

public interface ISaveablePrefab : ISaveableReference, ISaveable
{
	uint PrefabId { get; set; }

	bool IsValidPrefab { get; }

	bool IsMultiplayerEntity { get; }

	bool IsOwner { get; }

	BoltEntity Entity { get; }

	Collider[] AllColliders { get; set; }

	Rigidbody[] AllRigidbodies { get; set; }

	int GetColliderId(Collider collider);

	int GetRigidbodyId(Rigidbody rigidbody);
}
