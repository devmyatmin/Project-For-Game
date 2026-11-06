namespace Beam;

public interface IAttachable : IPickupable, IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial, IPhysical
{
	bool CanAttach { get; }

	bool IsAttached { get; }

	IAttachingAnchor AttachingAnchor { get; }

	event AttachableEventHandler<IAttachable> DetachedEvent;

	void Attach(IAttachingAnchor anchor);

	void Detach();
}
