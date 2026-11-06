namespace Beam;

public interface ISpearedInstance : IKillable
{
	IPhysical PhysicsObject { get; }

	IPickupable PickupableObject { get; }
}
