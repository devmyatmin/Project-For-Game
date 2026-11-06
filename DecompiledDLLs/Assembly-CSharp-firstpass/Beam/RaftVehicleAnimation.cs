namespace Beam;

public class RaftVehicleAnimation : VehicleAnimation<PaddleVehicleMovement>
{
	protected override void VehicleMovement_Operated(bool active)
	{
		base.VehicleMovement_Operated(active);
		VehicleMovement.Operator.Character.SetInteger("Vehicle_Pose", active ? 5 : 0);
	}
}
