namespace Beam;

public interface ICraftingDisplayableStorable : IDisplayableStorable, IStorable
{
	string GetHealthTerm();

	float GetHealthUnits();

	float GetHealthRange();

	InteractiveObjectClassification GetClassification();
}
