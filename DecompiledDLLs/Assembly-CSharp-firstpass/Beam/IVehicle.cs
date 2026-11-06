namespace Beam;

public interface IVehicle : IBase, IHasCraftingType
{
	VehicleMovementBase VehicleMovement { get; }
}
