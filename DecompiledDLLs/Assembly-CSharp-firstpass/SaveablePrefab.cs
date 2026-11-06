using System;
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
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_parentId = value;
			if (MPExtensions.IsMultiplayer(Game.Mode) && IsMultiplayerEntity)
			{
				if (value.IsUnknown())
				{
					SetTransformSyncInternal(enabled: false);
				}
				if (IsOwnerSafe(onException: false))
				{
					((MultiplayerBehaviour)this).GetMultiplayerState<ISaveablePrefabState>().ParentId = (int)value;
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
			return ((Component)this).gameObject.activeSelf;
		}
		set
		{
			if (IsMultiplayerEntity && IsOwner)
			{
				((MultiplayerBehaviour)this).GetMultiplayerState<ISaveablePrefabState>().Disabled = !value;
			}
			else
			{
				((Component)this).gameObject.SetActive(value);
			}
		}
	}

	public BoltEntity Entity => ((EntityBehaviour)this).entity;

	public MultiplayerTokens.SaveablePrefab AttachToken => (MultiplayerTokens.SaveablePrefab)(object)((EntityBehaviour)this).entity.AttachToken;

	public bool IsOwnerSafe(bool onException = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			return MPExtensions.IsSinglePlayer(Game.Mode) || !IsMultiplayerEntity || Entity.IsOwner;
		}
		catch (Exception)
		{
			return onException;
		}
	}

	public int GetRigidbodyId(Rigidbody rigidbody)
	{
		if (BeamTeam.IsNullOrDestroyed(rigidbody))
		{
			return -1;
		}
		for (int i = 0; i < AllRigidbodies.Length; i++)
		{
			if ((Object)(object)AllRigidbodies[i] == (Object)(object)rigidbody)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetColliderId(Collider collider)
	{
		if (BeamTeam.IsNullOrDestroyed(collider))
		{
			return -1;
		}
		for (int i = 0; i < AllColliders.Length; i++)
		{
			if ((Object)(object)AllColliders[i] == (Object)(object)collider)
			{
				return i;
			}
		}
		return -1;
	}

	protected virtual void SetTransformSyncInternal(bool enabled)
	{
		ISaveablePrefabState multiplayerState = ((MultiplayerBehaviour)this).GetMultiplayerState<ISaveablePrefabState>();
		((IState)multiplayerState).SetTransforms(multiplayerState.Transform, enabled ? ((Component)this).transform : null);
	}

	public override void Attached()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected Obj, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected Obj, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected Obj, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (IsValidPrefab)
		{
			base.Attached();
			ReferenceId = AttachToken.Id;
			SetTransformSyncInternal(enabled: true);
			ISaveablePrefabState multiplayerState = ((MultiplayerBehaviour)this).GetMultiplayerState<ISaveablePrefabState>();
			((IState)multiplayerState).AddCallback("Disabled", (PropertyCallbackSimple)OnDisabledChanged);
			((IState)multiplayerState).AddCallback("ParentId", (PropertyCallbackSimple)OnParentIdChanged);
			((IState)multiplayerState).AddCallback("TransformSerializationId", (PropertyCallbackSimple)OnTransformSerialisationIdChanged);
			if (MPExtensions.IsClient(Game.Mode))
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!MPExtensions.IsClient(Game.Mode) && IsMultiplayerEntity && !BeamTeam.IsNullOrDestroyed(((EntityBehaviour)this).entity) && ((EntityBehaviour)this).entity.IsAttached)
			{
				ISaveablePrefabState multiplayerState = ((MultiplayerBehaviour)this).GetMultiplayerState<ISaveablePrefabState>();
				multiplayerState.TransformSerializationId += 1;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Error in CompleteTransformSerialization\n" + ex.Message + "\n" + ex.StackTrace));
		}
	}

	protected virtual void OnTransformParentChanged()
	{
		ParentId = MPExtensions.GetTransfromId(((Component)this).transform.parent);
	}

	private void OnDisabledChanged()
	{
		((Component)this).gameObject.SetActive(!((MultiplayerBehaviour)this).GetMultiplayerState<ISaveablePrefabState>().Disabled);
	}

	private void OnParentIdChanged()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		TransformId parentId = (TransformId)((MultiplayerBehaviour)this).GetMultiplayerState<ISaveablePrefabState>().ParentId;
		SetTransformSyncInternal(parentId.IsValid());
		if (MPExtensions.IsClient(Game.Mode) && IsMultiplayerEntity && MultiplayerMng.ReceivedEntitiesActive && parentId != TransformId.Unknown && parentId != _parentId)
		{
			((Component)this).transform.SetParent(parentId.GetTransfrom(), false);
		}
		_parentId = parentId;
	}

	public virtual void OnCustomMapLoaded()
	{
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("prefab", JSerializer.Serialize((object)_prefabId));
		return jObject;
	}
}
