using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class HoldingRaycastFilter : IRaycastFilter
{
	private readonly Transform _holdingContainer;

	public HoldingRaycastFilter(Transform holdingContainer)
	{
		_holdingContainer = holdingContainer;
	}

	public bool Validate(RaycastHit hit)
	{
		if ((Object)(object)_holdingContainer == (Object)null)
		{
			return true;
		}
		return !Beam.Utilities.ExtensionMethods.ContainsNestedChild(_holdingContainer, hit.transform);
	}
}
