using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class ConnectorCheck_FoundationBelow : ConnectorCheck
{
	public override bool Check(Placement placement, Connector connector)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)connector))
		{
			return false;
		}
		if (connector.Type == ConnectorType.FLOOR)
		{
			return BeamPhysics.CheckComponentInSphere<Construction_FOUNDATION>(((Component)connector).transform.position + Vector3.down * 3f, 0.6f, 1 << Layers.CONSTRUCTIONS);
		}
		return true;
	}
}
