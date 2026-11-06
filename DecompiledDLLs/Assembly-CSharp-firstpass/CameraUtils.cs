using Beam.PostEffects;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraUtils : MonoBehaviour
{
	[SerializeField]
	private Component[] _components;

	public void RemoveComponents()
	{
		Component[] components = _components;
		for (int i = 0; i < components.Length; i++)
		{
			BeamTeam.Destroy<Component>(components[i]);
		}
		BeamPfxLayer component = ((Component)this).GetComponent<BeamPfxLayer>();
		if (!((Object)(object)component == (Object)null))
		{
			MotionBlur val = default;
			if (component.Profile.TryGetSettings<MotionBlur>(ref val))
			{
				((ParameterOverride<bool>)(object)((PostProcessEffectSettings)val).enabled).Override(false);
			}
			HighlighterSettings highlighterSettings = default;
			if (component.Profile.TryGetSettings<HighlighterSettings>(ref highlighterSettings))
			{
				((ParameterOverride<bool>)(object)((PostProcessEffectSettings)highlighterSettings).enabled).Override(false);
			}
		}
	}
}
