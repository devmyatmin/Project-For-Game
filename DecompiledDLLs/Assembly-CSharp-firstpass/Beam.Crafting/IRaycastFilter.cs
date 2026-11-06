using UnityEngine;

namespace Beam.Crafting;

public interface IRaycastFilter
{
	bool Validate(RaycastHit hit);
}
