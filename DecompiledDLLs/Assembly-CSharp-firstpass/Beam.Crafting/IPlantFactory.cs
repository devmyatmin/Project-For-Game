namespace Beam.Crafting;

public interface IPlantFactory
{
	Plant CreatePlant(PlantType type);

	void DestroyPlant(Plant plant);
}
