using UnityEngine;

namespace Beam.Crafting;

public class NullRaycastFilter : IRaycastFilter
{
	public static NullRaycastFilter Instance { get; } = new NullRaycastFilter();

	public bool Validate(RaycastHit hit)
	{
		return true;
	}
}
