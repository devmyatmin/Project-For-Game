namespace Beam;

public interface IUseable : IAnimatablePrimary, IBase, IHasCraftingType
{
	bool CanUse { get; set; }

	bool CanUseOnObjects { get; set; }

	bool SingleUse { get; }

	UsingConstraint UsingConstraints { get; }

	BaseActionUnityEvent Used { get; }

	void Use();

	void UseOnObject(IBase obj);

	string GetUsingDescription(IBase obj);
}
