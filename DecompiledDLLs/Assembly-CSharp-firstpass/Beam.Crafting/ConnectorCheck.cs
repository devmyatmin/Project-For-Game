using UnityEngine;

namespace Beam.Crafting;

public class ConnectorCheck : MonoBehaviour
{
	public virtual bool Check(Placement placement, Connector connector)
	{
		return true;
	}
}
