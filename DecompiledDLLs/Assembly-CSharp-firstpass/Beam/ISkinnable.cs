namespace Beam;

public interface ISkinnable : IDisintegratable, IBase, IHasCraftingType, INeedSkills
{
	SkinningRewardType MeatType { get; }

	bool CanSkin { get; set; }

	int FailedAttempts { get; set; }

	int Skinned { get; set; }

	int Quantity { get; }

	int LeatherQuantity { get; set; }

	InteracterUnityEvent OnSkinned { get; }
}
