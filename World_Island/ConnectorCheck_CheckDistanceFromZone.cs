namespace Beam.Crafting;

public class ConnectorCheck_CheckDistanceFromZone : ConnectorCheck
{
	private const float MAX_DISTANCE_FROM_ZONE = 125f;

	public override bool Check(Placement placement, Connector connector)
	{
		Zone zone = StrandedWorld.GetZone(placement.Position);
		if (!zone || Vector3Tools.SqrDistanceOnAxis(zone.transform.position, placement.Position, Vector3Tools.xz) > 15625f)
		{
			return false;
		}
		return true;
	}
}
