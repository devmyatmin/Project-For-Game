namespace Beam;

public class RudderVehicleAnimation : VehicleAnimation<RudderVehicleMovement>
{
	protected override void VehicleMovement_Operated(bool active)
	{
		base.VehicleMovement_Operated(active);
		VehicleMovement.Operator.Character.SetInteger("Vehicle_Pose", active ? 4 : 0);
	}
}
