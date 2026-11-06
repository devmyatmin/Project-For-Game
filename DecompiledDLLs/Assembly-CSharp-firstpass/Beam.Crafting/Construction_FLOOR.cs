using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class Construction_FLOOR : Construction_FOUNDATION, IHaveAttribute
{
	protected override void Awake()
	{
		base.Awake();
		ProximityDistance -= 0.3f;
	}

	protected override IList<Connector> GetConnectorsAtPlacement()
	{
		return BeamPhysics.GetComponentsInBounds<Connector>(Colliders, (1 << Layers.CONNECTOR_FOUNDATION) | (1 << Layers.CONNECTOR_PANEL));
	}

	protected override bool CheckPlacementIsSupported(IList<Connector> connectors)
	{
		foreach (Connector connector in connectors)
		{
			Construction_FOUNDATION construction_FOUNDATION = connector.Constructing as Construction_FOUNDATION;
			if ((Object)(object)construction_FOUNDATION != (Object)null && construction_FOUNDATION.Walls > 1 && Object.op_Implicit((Object)(object)(construction_FOUNDATION as Construction_FLOOR)))
			{
				return true;
			}
			if ((Object)(object)construction_FOUNDATION != (Object)null && construction_FOUNDATION.Walls > 1 && !Object.op_Implicit((Object)(object)(construction_FOUNDATION as Construction_FLOOR)))
			{
				return true;
			}
		}
		return CheckSupportingNeighbors();
	}

	private bool CheckSupportingNeighbors()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Connector[] myConnectors = MyConnectors;
		foreach (Connector connector in myConnectors)
		{
			if (connector.Type != ConnectorType.FLOOR)
			{
				continue;
			}
			foreach (Construction_FLOOR item in BeamPhysics.GetComponentsInSphere<Construction_FLOOR>(((Component)connector).transform.position, 0.3f, 1 << Layers.CONSTRUCTIONS))
			{
				Construction_FOUNDATION componentRaycastAll = BeamPhysics.GetComponentRaycastAll<Construction_FOUNDATION>(new Ray(item.GeometryCenter.position, Vector3.down), 3f, 1 << Layers.CONSTRUCTIONS);
				if ((Object)(object)componentRaycastAll != (Object)null && componentRaycastAll.Walls > 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Damage(float damage, IBase damager)
	{
		base.Damage(damage, damager);
	}
}
