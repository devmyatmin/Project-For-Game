using UnityEngine;

namespace Beam.Crafting;

public class ConnectorCheck_CheckDistanceFromZone : ConnectorCheck
{
	private const float MAX_DISTANCE_FROM_ZONE = 125f;

	public override bool Check(Placement placement, Connector connector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = StrandedWorld.GetZone(placement.Position);
		if (!Object.op_Implicit((Object)(object)zone) || Vector3Tools.SqrDistanceOnAxis(((Component)zone).transform.position, placement.Position, Vector3Tools.xz) > 15625f)
		{
			return false;
		}
		return true;
	}
}
