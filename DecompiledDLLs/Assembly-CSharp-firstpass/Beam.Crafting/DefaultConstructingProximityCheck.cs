using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class DefaultConstructingProximityCheck : IConstructingProximityCheck
{
	public bool CheckProximity(Constructing constructing, Transform ghost, float distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		IList<Constructing> componentsInSphere = BeamPhysics.GetComponentsInSphere<Constructing>(ghost.position, distance, (1 << Layers.CONSTRUCTIONS) | (1 << Layers.CONSTRUCTIONS_RAFTS));
		if (Object.op_Implicit((Object)(object)constructing.SnappedConnector))
		{
			for (int i = 0; i < componentsInSphere.Count; i++)
			{
				Constructing constructing2 = componentsInSphere[i];
				if ((Object)(object)constructing.SnappedConnector.Constructing.Structure != (Object)(object)constructing2.Structure)
				{
					return true;
				}
			}
			return false;
		}
		return componentsInSphere.Count > 0;
	}
}
