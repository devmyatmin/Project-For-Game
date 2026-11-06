using System;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

[Serializable]
[PostProcess(/*Could not decode attribute arguments.*/)]
public sealed class AtmosphericFogSettings : PostProcessEffectSettings
{
	public FloatParameter Height;

	public FloatParameter StartDistance;

	public AtmosphericFogSettings()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		//IL_0016: Expected Obj, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		//IL_002c: Expected Obj, but got Unknown
		FloatParameter val = new FloatParameter();
		((ParameterOverride<float>)val).value = 80f;
		Height = val;
		FloatParameter val2 = new FloatParameter();
		((ParameterOverride<float>)val2).value = 200f;
		StartDistance = val2;
		((PostProcessEffectSettings)this)._002Ector();
	}
}
