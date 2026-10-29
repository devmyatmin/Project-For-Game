using System;
using System.Runtime.CompilerServices;
using Beam;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using UnityEngine;

public abstract class SaveablePrefab : SaveableReference, ISaveablePrefab, ISaveableReference, ISaveable, IGenerationObject, IMapEditorObject
{
	[SerializeField]
	[HideInInspector]
	private uint _prefabId;

	[PREFAB_PROCESSOR_GetComponentsInChildren]
	[SerializeField]
	private Collider[] _allColliders;

	[PREFAB_PROCESSOR_GetComponentsInChildren]
	[SerializeField]
	private Rigidbody[] _allRigidBodies;

	[SerializeField]
	private bool _canParent = true;

	private TransformId _parentId;

	public uint PrefabId
	{
		get
		{
			return _prefabId;
		}
		set
		{
			_prefabId = value;
		}
	}

	public Collider[] AllColliders
	{
		get
		{
			return _allColliders;
		}
		set
		{
			_allColliders = value;
		}
	}

	public Rigidbody[] AllRigidbodies
	{
		get
		{
			return _allRigidBodies;
		}
		set
		{
			_allRigidBodies = value;
		}
	}

	public bool CanParent
	{
		get
		{
			return _canParent;
		}
		set
		{
			_canParent = value;
		}
	}

	public virtual bool CanReorder => CanParent;

	public TransformId ParentId
	{
		get
		{
			return _parentId;
		}
		set
		{
			_parentId = value;
			if (Game.Mode.IsMultiplayer() && IsMultiplayerEntity)
			{
				if (value.IsUnknown())
				{
					SetTransformSyncInternal(enabled: false);
				}
				if (IsOwnerSafe(onException: false))
				{
					GetMultiplayerState<ISaveablePrefabState>().ParentId = (int)value;
				}
			}
		}
	}

	public virtual bool IsValidPrefab => PrefabId != 0;

	public virtual bool IsMultiplayerEntity => Prefabs.IsMultiplayerEntity(_prefabId);

	public bool IsOwner => IsOwnerSafe();

	public bool Active
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			if (IsMultiplayerEntity && IsOwner)
			{
				GetMultiplayerState<ISaveablePrefabState>().Disabled = !value;
			}
			else
			{
				base.gameObject.SetActive(value);
			}
		}
	}

	public BoltEntity Entity => base.entity;

	public MultiplayerTokens.SaveablePrefab AttachToken => (MultiplayerTokens.SaveablePrefab)base.entity.AttachToken;

	public bool IsOwnerSafe(bool onException = true)
	{
		try
		{
			return Game.Mode.IsSinglePlayer() || !IsMultiplayerEntity || Entity.IsOwner;
		}
		catch (Exception)
		{
			return onException;
		}
	}

	public int GetRigidbodyId(Rigidbody rigidbody)
	{
		if (rigidbody.IsNullOrDestroyed())
		{
			return -1;
		}
		for (int i = 0; i < AllRigidbodies.Length; i++)
		{
			if (AllRigidbodies[i] == rigidbody)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetColliderId(Collider collider)
	{
		if (collider.IsNullOrDestroyed())
		{
			return -1;
		}
		for (int i = 0; i < AllColliders.Length; i++)
		{
			if (AllColliders[i] == collider)
			{
				return i;
			}
		}
		return -1;
	}

	protected virtual void SetTransformSyncInternal(bool enabled)
	{
		ISaveablePrefabState multiplayerState = GetMultiplayerState<ISaveablePrefabState>();
		multiplayerState.SetTransforms(multiplayerState.Transform, enabled ? base.transform : null);
	}

	public override void Attached()
	{
		if (IsValidPrefab)
		{
			base.Attached();
			base.ReferenceId = AttachToken.Id;
			SetTransformSyncInternal(enabled: true);
			ISaveablePrefabState multiplayerState = GetMultiplayerState<ISaveablePrefabState>();
			multiplayerState.AddCallback("Disabled", OnDisabledChanged);
			multiplayerState.AddCallback("ParentId", OnParentIdChanged);
			multiplayerState.AddCallback("TransformSerializationId", OnTransformSerialisationIdChanged);
			if (Game.Mode.IsClient())
			{
				MultiplayerZoneSync.Instance.LinkInstance(this);
			}
		}
	}

	public virtual void OnTransformSerialisationIdChanged()
	{
	}

	public void CompleteTransformSerialization()
	{
		try
		{
			if (!Game.Mode.IsClient() && IsMultiplayerEntity && !base.entity.IsNullOrDestroyed() && base.entity.IsAttached)
			{
				GetMultiplayerState<ISaveablePrefabState>().TransformSerializationId++;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Error in CompleteTransformSerialization\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	protected virtual void OnTransformParentChanged()
	{
		ParentId = base.transform.parent.GetTransfromId();
	}

	private void OnDisabledChanged()
	{
		base.gameObject.SetActive(!GetMultiplayerState<ISaveablePrefabState>().Disabled);
	}

	private void OnParentIdChanged()
	{
		TransformId parentId = (TransformId)GetMultiplayerState<ISaveablePrefabState>().ParentId;
		SetTransformSyncInternal(parentId.IsValid());
		if (Game.Mode.IsClient() && IsMultiplayerEntity && MultiplayerMng.ReceivedEntitiesActive && parentId != TransformId.Unknown && parentId != _parentId)
		{
			base.transform.SetParent(parentId.GetTransfrom(), worldPositionStays: false);
		}
		_parentId = parentId;
	}

	public virtual void OnCustomMapLoaded()
	{
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("prefab", JSerializer.Serialize(_prefabId));
		return jObject;
	}

	[SpecialName]
	GameObject IGenerationObject.get_gameObject()
	{
		return base.gameObject;
	}
}
