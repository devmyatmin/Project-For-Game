using System;
using System.Collections.Generic;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;

namespace Beam.Crafting;

public class Constructing_HOOK : Constructing, IInteractable, IBase, IHasCraftingType, IAttachingAnchor
{
	[SerializeField]
	private bool _canInteract;

	[Header("ATTACHING")]
	[SerializeField]
	private InteractiveObject _anchorParent;

	[SerializeField]
	private StaticAttachingAnchor _attachingPosition;

	[SerializeField]
	private List<CraftingType> _attachingMask;

	private IAttachable _attached;

	[SerializeField]
	private bool _isLightHook;

	private Vector3 _attachedOffset;

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

	public IAttachable Attached1 => _attached;

	string IInteractable.GetInteractionDescription(int playerId, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		if (_attached == null)
		{
			IAttachable attachable = obj as IAttachable;
			if (CanAttach(attachable))
			{
				return "ITEM_INTERACTION_DESCRIPTION_ATTACH";
			}
		}
		if (_attached != null)
		{
			return "ITEM_INTERACTION_DESCRIPTION_DETACH";
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
		if (!((Object)(object)_anchorParent == (Object)null))
		{
			return _anchorParent.IsKinematic;
		}
		return true;
	}

	private IEnumerator<float> Follow_Sequence()
	{
		while (!BeamTeam.IsNullOrDestroyed(_attached))
		{
			if (!BeamTeam.IsNullOrDestroyed(_attached))
			{
				Vector3 val = transform.rotation * _attachingPosition.GetPosition();
				Vector3 val2 = _attached.transform.rotation * _attached.AttachingAnchor.GetPosition();
				_attachedOffset = val + val2;
				_attached.transform.position = transform.position + _attachedOffset;
				_attached.transform.rotation = transform.rotation * _attachingPosition.GetRotation() * _attached.AttachingAnchor.GetRotation();
			}
			yield return 0f;
		}
	}

	public bool Interact(IPlayer player)
	{
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		if (player.Movement.CheckVehicle())
		{
			return false;
		}
		return Attach(obj, player.Holder.DropCurrent);
	}

	public bool Attach(IBase obj, Action onAttached = null)
	{
		if (!BeamTeam.IsNullOrDestroyed(_attached))
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

	private void Attach(IAttachable attachable)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		_attached = attachable;
		_attached.transform.rotation = transform.rotation * (_attachingPosition.GetRotation() * _attached.AttachingAnchor.GetRotation());
		Vector3 val = transform.rotation * _attachingPosition.GetPosition();
		Vector3 val2 = _attached.transform.rotation * _attached.AttachingAnchor.GetPosition();
		_attachedOffset = val + val2;
		_attached.transform.position = transform.position + _attachedOffset;
		_attached.Attach(this);
		_attached.DetachedEvent += Attachable_Detached;
		CanInteract = false;
		if (_isLightHook && !_followSequence.IsRunning && !BeamTeam.IsNullOrDestroyed(_attached))
		{
			_followSequence = Timing.RunCoroutine(Follow_Sequence());
		}
	}

	private void Detach()
	{
		if (!BeamTeam.IsNullOrDestroyed(_attached))
		{
			_attached.DetachedEvent -= Attachable_Detached;
			_attached = null;
			CanInteract = true;
		}
	}

	private void Attachable_Detached(IAttachable attachable)
	{
		Detach();
	}

	public override void Damage(float damage, IBase damager)
	{
		if (!BeamTeam.IsNullOrDestroyed(_attached))
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

	public override void CheckStructure()
	{
		if (_isLightHook && !BeamTeam.IsNullOrDestroyed(SnappedConnector))
		{
			base.CheckStructure();
		}
	}

	private bool CanAttach(IAttachable attachable)
	{
		if (!BeamTeam.IsNullOrDestroyed(attachable) && attachable.CanAttach && attachable.CraftingType.InteractiveType != InteractiveType.CONTAINER)
		{
			if (_attachingMask != null && _attachingMask.Count != 0)
			{
				return _attachingMask.Contains(attachable.CraftingType);
			}
			return true;
		}
		return false;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if (!BeamTeam.IsNullOrDestroyed(_attached))
		{
			jObject.AddField("Attached", _attached.ReferenceId.ToString());
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject attachedData = data.GetField("Attached");
		if (attachedData == null)
		{
			return;
		}
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			InteractiveObject interactiveObject = MultiplayerMng.FindObject<InteractiveObject>(Beam.Utilities.ExtensionMethods.ToMiniGuid(attachedData.GetValue<string>()));
			if ((Object)(object)interactiveObject != (Object)null && interactiveObject is IAttachable attachable)
			{
				Attach(attachable);
			}
		});
	}
}
