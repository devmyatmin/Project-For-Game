using UnityEngine;

namespace Beam.Crafting;

public class ConnectorCheck_Height : ConnectorCheck
{
	public override bool Check(Placement placement, Connector connector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return Physics.Raycast(new Ray(placement.Position + new Vector3(0f, 0.01f, 0f), Vector3.down), 2.5f, 1 << Layers.TERRAIN);
	}
}
