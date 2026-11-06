using UnityEngine;

namespace Beam.Crafting;

public class PlacementModifier_SnapRotationToConnector : PlacementModifier
{
	public override Placement Modify(Placement placement, BeamRay ray)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Connector snappedConnector = ((Component)this).GetComponent<Constructing>().SnappedConnector;
		if (Object.op_Implicit((Object)(object)snappedConnector))
		{
			placement.Rotation *= ((Component)snappedConnector).transform.rotation;
		}
		return placement;
	}
}
