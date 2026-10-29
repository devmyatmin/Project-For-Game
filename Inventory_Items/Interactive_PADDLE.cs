using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
		this.DetachedEvent?.Invoke(sender);
		this.DetachedEvent = null;
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
		while (IsAttached && !_attachedAnchor.IsNullOrDestroyed())
		{
			base.transform.position = _attachedAnchor.GetPosition() + base.transform.rotation * _attachingAnchor.GetPosition();
			base.transform.rotation = _attachedAnchor.GetRotation() * _attachingAnchor.GetRotation();
			yield return 0f;
		}
		if (_attachedAnchor.IsNullOrDestroyed())
		{
			BeamTeam.Destroy(base.gameObject);
		}
	}

	public override void OnDestroy()
	{
		Timing.KillCoroutines(_followSequence);
		this.DetachedEvent = null;
		base.OnDestroy();
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		base.CanUseOnObjects = true;
		base.CanDamage = true;
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
