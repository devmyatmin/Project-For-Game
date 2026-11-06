using UnityEngine;

namespace Beam.Crafting;

public class PlacementModifier : MonoBehaviour
{
	public virtual Placement Modify(Placement placement, BeamRay ray)
	{
		return placement;
	}
}
