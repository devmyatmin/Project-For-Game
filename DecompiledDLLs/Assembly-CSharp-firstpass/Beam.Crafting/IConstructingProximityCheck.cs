using UnityEngine;

namespace Beam.Crafting;

public interface IConstructingProximityCheck
{
	bool CheckProximity(Constructing constructing, Transform ghost, float distance);
}
