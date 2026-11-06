using System;
using System.Collections.Generic;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;

namespace Beam.Crafting;

public class Constructing_CONTAINER_SHELF : Constructing, IInteractable, IBase, IHasCraftingType, IAttachingAnchor
{
	[SerializeField]
	private bool _canInteract;

	[Header("ATTACHING")]
	[SerializeField]
	private StaticAttachingAnchor _attachingPosition;

	[SerializeField]
	private StaticAttachingAnchor _attachingPosition2;

	[SerializeField]
	private StaticAttachingAnchor _attachingPosition3;

	[SerializeField]
	private List<CraftingType> _attachingMask;

	[SerializeField]
	private Rigidbody _anchorRigidbody;

	private List<IAttachable> _attachables;

	private List<IAttachingAnchor> _attachableAnchors;

	private CoroutineHandle _followSequence;

	public bool CanInteract
	{
		get
		{
			return _canInteract;
		}
		set
		{
			_canInteract = value;
		}
	}

	public IAttachingAnchor AttachingPosition => _attachingPosition;

	public IAttachingAnchor AttachingPosition2 => _attachingPosition2;

	public IAttachingAnchor AttachingPosition3 => _attachingPosition3;

	private List<IAttachable> Attachables
	{
		get
		{
			if (_attachables == null)
			{
				_attachables = new List<IAttachable> { null, null, null };
			}
			return _attachables;
		}
	}

	private List<IAttachingAnchor> AttachableAnchors
	{
		get
		{
			if (_attachableAnchors == null)
			{
				_attachableAnchors = new List<IAttachingAnchor> { _attachingPosition, _attachingPosition2, _attachingPosition3 };
			}
			return _attachableAnchors;
		}
	}

	string IInteractable.GetInteractionDescription(int playerId, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		if (HasFreeSlots())
		{
			IAttachable attachable = obj as IAttachable;
			if (CanAttach(attachable))
			{
				return "ITEM_INTERACTION_DESCRIPTION_ATTACH";
			}
		}
		return string.Empty;
	}

	Vector3 IAttachingAnchor.GetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		return transform.position + transform.rotation * _attachingPosition.GetPosition();
	}

	Quaternion IAttachingAnchor.GetRotation()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return transform.rotation * _attachingPosition.GetRotation();
	}

	bool IAttachingAnchor.IsKinematic()
	{
		if (!((Object)(object)_anchorRigidbody == (Object)null))
		{
			return _anchorRigidbody.isKinematic;
		}
		return true;
	}

	public bool Interact(IPlayer player)
	{
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Attach(obj, player.Holder.DropCurrent);
	}

	public bool Attach(IBase obj, Action onAttached = null)
	{
		if (!HasFreeSlots())
		{
			return false;
		}
		IAttachable attachable = obj as IAttachable;
		if (CanAttach(attachable))
		{
			onAttached?.Invoke();
			Attach(attachable);
			return true;
		}
		return false;
	}

	private int GetUnusedPositionIndex()
	{
		for (int i = 0; i < Attachables.Count; i++)
		{
			if (BeamTeam.IsNullOrDestroyed(Attachables[i]))
			{
				return i;
			}
		}
		return 0;
	}

	private bool HasFreeSlots()
	{
		for (int i = 0; i < Attachables.Count; i++)
		{
			if (BeamTeam.IsNullOrDestroyed(Attachables[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasContainersStored()
	{
		for (int i = 0; i < Attachables.Count; i++)
		{
			if (!BeamTeam.IsNullOrDestroyed(Attachables[i]))
			{
				return true;
			}
		}
		return false;
	}

	private void Attach(IAttachable attachable, int index = -1)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (index == -1)
		{
			index = GetUnusedPositionIndex();
		}
		IAttachingAnchor attachingAnchor = AttachableAnchors[index];
		attachable.transform.rotation = transform.rotation * (attachingAnchor.GetRotation() * attachable.AttachingAnchor.GetRotation());
		Vector3 val = transform.rotation * attachingAnchor.GetPosition();
		Vector3 val2 = attachable.transform.rotation * attachable.AttachingAnchor.GetPosition();
		attachable.transform.position = transform.position + val + val2;
		attachable.Attach(this);
		attachable.DetachedEvent += Attachable_Detached;
		Attachables[index] = attachable;
		if (!_followSequence.IsRunning && HasContainersStored())
		{
			_followSequence = Timing.RunCoroutine(Follow_Sequence());
		}
	}

	private void Detach(IAttachable attachable)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(attachable))
		{
			attachable.DetachedEvent -= Attachable_Detached;
			for (int i = 0; i < Attachables.Count; i++)
			{
				if (Attachables[i] == attachable)
				{
					Attachables[i] = null;
					break;
				}
			}
			CanInteract = true;
		}
		if (!HasContainersStored())
		{
			Timing.KillCoroutines(_followSequence);
		}
	}

	private void Attachable_Detached(IAttachable attachable)
	{
		Detach(attachable);
	}

	private bool CanAttach(IAttachable attachable)
	{
		if (!BeamTeam.IsNullOrDestroyed(attachable) && attachable.CanAttach)
		{
			if (_attachingMask != null && _attachingMask.Count != 0)
			{
				return _attachingMask.Contains(attachable.CraftingType);
			}
			return true;
		}
		return false;
	}

	public override void Damage(float damage, IBase damager)
	{
		if (HasContainersStored())
		{
			IPickupable pickupable = damager as IPickupable;
			if (!BeamTeam.IsNullOrDestroyed(pickupable))
			{
				IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
				if (!BeamTeam.IsNullOrDestroyed(playerFromId))
				{
					LocalizedNotification.Post(playerFromId, NotificationPriority.Low, 4f, "CONSTRUCTING_NTF_CANT_DESTROY_WITH_ATTACHED_DESC");
				}
			}
		}
		else
		{
			base.Damage(damage, damager);
		}
	}

	private IEnumerator<float> Follow_Sequence()
	{
		while (HasContainersStored())
		{
			for (int i = 0; i < Attachables.Count; i++)
			{
				IAttachable attachable = Attachables[i];
				IAttachingAnchor attachingAnchor = AttachableAnchors[i];
				if (!BeamTeam.IsNullOrDestroyed(attachable))
				{
					attachable.transform.position = transform.position + attachable.transform.rotation * attachingAnchor.GetPosition();
					attachable.transform.rotation = transform.rotation * attachingAnchor.GetRotation();
				}
			}
			yield return 0f;
		}
	}

	public override void CheckStructure()
	{
		if (!BeamTeam.IsNullOrDestroyed(SnappedConnector))
		{
			base.CheckStructure();
		}
	}

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_followSequence);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if (!BeamTeam.IsNullOrDestroyed(Attachables[0]))
		{
			jObject.AddField("Attached", Attachables[0].ReferenceId.ToString());
		}
		if (!BeamTeam.IsNullOrDestroyed(Attachables[1]))
		{
			jObject.AddField("Attached2", Attachables[1].ReferenceId.ToString());
		}
		if (!BeamTeam.IsNullOrDestroyed(Attachables[2]))
		{
			jObject.AddField("Attached3", Attachables[2].ReferenceId.ToString());
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("Attached");
		JObject field2 = data.GetField("Attached2");
		JObject field3 = data.GetField("Attached3");
		List<JObject> list = new List<JObject> { field, field2, field3 };
		for (int i = 0; i < list.Count; i++)
		{
			int index = i;
			JObject json = list[index];
			if (json == null)
			{
				continue;
			}
			LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
			{
				InteractiveObject interactiveObject = MultiplayerMng.FindObject<InteractiveObject>(Beam.Utilities.ExtensionMethods.ToMiniGuid(json.GetValue<string>()));
				if ((Object)(object)interactiveObject != (Object)null && interactiveObject is IAttachable attachable)
				{
					Attach(attachable, index);
				}
			});
		}
	}
}
