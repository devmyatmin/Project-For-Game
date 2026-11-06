namespace Beam;

public interface IChargeable : IUseable, IAnimatablePrimary, IBase, IHasCraftingType, IReloadable, IAnimatableSecondary
{
	bool CanUseSecondary { get; set; }

	BaseActionUnityEvent Charged { get; }

	BaseActionUnityEvent UsedSecondary { get; }

	void PreSecondary();

	void UseSecondary();

	void PostSecondary();
}
