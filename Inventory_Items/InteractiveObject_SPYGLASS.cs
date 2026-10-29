using UnityEngine;
using UnityEngine.Rendering;

namespace Beam.Crafting;

public class InteractiveObject_SPYGLASS : InteractiveObject
{
	public override bool ValidatePrimary(IBase obj)
	{
		return false;
	}

	public void ToggleShadowsOnly(bool value)
	{
		if (base.renderers == null)
		{
			return;
		}
		Renderer[] array = base.renderers;
		foreach (Renderer renderer in array)
		{
			if (!(renderer == null))
			{
				renderer.shadowCastingMode = ((!value) ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly);
			}
		}
	}
}
