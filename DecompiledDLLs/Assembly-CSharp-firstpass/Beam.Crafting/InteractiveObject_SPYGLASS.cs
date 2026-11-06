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
		if (renderers == null)
		{
			return;
		}
		Renderer[] array = renderers;
		foreach (Renderer val in array)
		{
			if (!((Object)(object)val == (Object)null))
			{
				val.shadowCastingMode = (ShadowCastingMode)((!value) ? 1 : 3);
			}
		}
	}
}
