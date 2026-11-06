namespace Beam;

public interface IHaveFuel
{
	float FuelCapacity { get; }

	float Fuel { get; }

	float Refill(float availableFuel);
}
