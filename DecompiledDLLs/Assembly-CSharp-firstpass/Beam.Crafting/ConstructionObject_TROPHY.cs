using UnityEngine;

namespace Beam.Crafting;

public class ConstructionObject_TROPHY : ConstructionObject, IAttachable, IPickupable, IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial, IPhysical
{
	[Header("ATTACHING")]
	[SerializeField]
	private bool _canAttach;

	[SerializeField]
	private StaticAttachingAnchor _attachingAnchor;

	public bool CanAttach
	{
		get
		{
			return _canAttach;
		}
		set
		{
			_canAttach = value;
		}
	}

	public bool IsAttached { get; protected set; }

	public IAttachingAnchor AttachingAnchor => _attachingAnchor;

	public event AttachableEventHandler<IAttachable> DetachedEvent;

	protected virtual void OnDetached(IAttachable sender)
	{
		DetachedEvent?.Invoke(sender);
		DetachedEvent = null;
	}

	public override bool Store()
	{
		Detach();
		return base.Store();
	}

	public void Attach(IAttachingAnchor anchor)
	{
		if (!IsAttached)
		{
			IsAttached = true;
			DisablePhysics();
		}
	}

	public void Detach()
	{
		if (IsAttached)
		{
			IsAttached = false;
			OnDetached(this);
		}
	}

	public override void OnDestroy()
	{
		DetachedEvent = null;
		base.OnDestroy();
	}
}
