using Ceto;
using UnityEngine;

namespace Beam.Crafting;

public class ConnectorCheck_Underwater : ConnectorCheck
{
	[SerializeField]
	private bool AllowedUnderwater;

	public override bool Check(Placement placement, Connector connector)
	{
		if (!AllowedUnderwater)
		{
			float num = Ceto.Ocean.Instance.QueryWaves(placement.Position.x, placement.Position.z);
			return placement.Position.y > num;
		}
		return base.Check(placement, connector);
	}
}
