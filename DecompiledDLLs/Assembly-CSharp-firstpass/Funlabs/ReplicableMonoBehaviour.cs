using System;
using Beam.Serialization.Json;
using Photon.Bolt;
using UnityEngine;

namespace Funlabs;

public abstract class ReplicableMonoBehaviour : MonoBehaviour, ISaveablePrefab, ISaveableReference, ISaveable
{
	public uint PrefabId
	{
		get
		{
			return 0u;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool IsValidPrefab => false;

	public bool IsMultiplayerEntity => false;

	public bool IsOwner => true;

	public BoltEntity Entity => null;

	public bool Active
	{
		get
		{
			return ((Component)this).gameObject.activeSelf;
		}
		set
		{
			((Component)this).gameObject.SetActive(value);
		}
	}

	public abstract MiniGuid ReferenceId { get; set; }

	public Collider[] AllColliders
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public Rigidbody[] AllRigidbodies
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public int GetColliderId(Collider collider)
	{
		throw new NotImplementedException();
	}

	public int GetRigidbodyId(Rigidbody rigidbody)
	{
		throw new NotImplementedException();
	}

	public virtual void Load(JObject data)
	{
	}

	public virtual JObject Save()
	{
		return null;
	}
}
