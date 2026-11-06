namespace Beam;

public static class IHaveFuelMixins
{
	public static bool CanRefill(this IHaveFuel fuelable)
	{
		return fuelable.Fuel < fuelable.FuelCapacity;
	}
}
