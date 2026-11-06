using System.Collections.Generic;
using Beam.Serialization.Json;
using Beam.Utilities;
using MEC;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class Interactive_PADDLE : InteractiveObject, IAttachable, IPickupable, IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial, IPhysical
{
	[Header("ATTACHING")]
	[SerializeField]
	private StaticAttachingAnchor _attachingAnchor;

	private IAttachingAnchor _attachedAnchor;

	private CoroutineHandle _followSequence;

	[SerializeField]
	private UnityEvent _onHeld;

	public IAttachingAnchor AttachingAnchor => _attachingAnchor;

	public bool CanAttach { get; set; } = true;

	public bool IsAttached { get; protected set; }

	public event AttachableEventHandler<IAttachable> DetachedEvent;

	protected virtual void OnDetached(IAttachable sender)
	{
		DetachedEvent?.Invoke(sender);
		DetachedEvent = null;
	}

	public override void Hold(bool holding)
	{
		base.Hold(holding);
		if (holding)
		{
			_onHeld.Invoke();
		}
	}

	public override bool Store()
	{
		Detach();
		return base.Store();
	}

	public virtual void Attach(IAttachingAnchor anchor)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!IsAttached)
		{
			DisablePhysics();
			CanDrag = false;
			IsAttached = true;
			_attachedAnchor = anchor;
			if (_attachedAnchor != null && !_attachedAnchor.IsKinematic())
			{
				_followSequence = Timing.RunCoroutine(Follow_Sequence());
			}
		}
	}

	public virtual void Detach()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (IsAttached)
		{
			OnDetached(this);
			CanDrag = true;
			IsAttached = false;
			Timing.KillCoroutines(_followSequence);
			_attachedAnchor = null;
		}
	}

	private IEnumerator<float> Follow_Sequence()
	{
		while (IsAttached && !BeamTeam.IsNullOrDestroyed(_attachedAnchor))
		{
			transform.position = _attachedAnchor.GetPosition() + transform.rotation * _attachingAnchor.GetPosition();
			transform.rotation = _attachedAnchor.GetRotation() * _attachingAnchor.GetRotation();
			yield return 0f;
		}
		if (BeamTeam.IsNullOrDestroyed(_attachedAnchor))
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
	}

	public override void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_followSequence);
		DetachedEvent = null;
		base.OnDestroy();
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		CanUseOnObjects = true;
		CanDamage = true;
	}
}
