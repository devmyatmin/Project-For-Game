namespace Beam.Crafting;

public class Constructing_ROOF : Constructing, IHaveAttribute
{
	public IconType Type => IconType.Construction;

	public float GetValue()
	{
		return HealthPoints / MaximumHealthPoints;
	}
}
