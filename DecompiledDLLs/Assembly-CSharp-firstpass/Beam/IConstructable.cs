namespace Beam;

public interface IConstructable : ICraftable, IBase, IHasCraftingType, IPlaceable
{
	float HealthPoints { get; set; }

	void CheckHealth();

	void Dismantle();

	bool HasMaxHealth();
}
