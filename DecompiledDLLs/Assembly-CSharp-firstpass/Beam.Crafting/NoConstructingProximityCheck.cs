using UnityEngine;

namespace Beam.Crafting;

public class NoConstructingProximityCheck : IConstructingProximityCheck
{
	public bool CheckProximity(Constructing constructing, Transform ghost, float distance)
	{
		return false;
	}
}
