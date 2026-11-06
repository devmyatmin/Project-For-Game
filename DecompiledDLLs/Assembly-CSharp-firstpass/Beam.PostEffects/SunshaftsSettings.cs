using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

[Serializable]
[PostProcess(/*Could not decode attribute arguments.*/)]
public class SunshaftsSettings : PostProcessEffectSettings
{
	public IntParameter Downsampling;

	[Range(1f, 4f)]
	public IntParameter BlurIterations;

	public FloatParameter BlurRadius;

	public FloatParameter DistanceFalloff;

	public FloatParameter Intensity;

	public SunshaftsSettings()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		//IL_0012: Expected Obj, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected Obj, but got Unknown
		//IL_0024: Expected Obj, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected Obj, but got Unknown
		//IL_003a: Expected Obj, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected Obj, but got Unknown
		//IL_0050: Expected Obj, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		//IL_0066: Expected Obj, but got Unknown
		IntParameter val = new IntParameter();
		((ParameterOverride<int>)val).value = 1;
		Downsampling = val;
		IntParameter val2 = new IntParameter();
		((ParameterOverride<int>)val2).value = 2;
		BlurIterations = val2;
		FloatParameter val3 = new FloatParameter();
		((ParameterOverride<float>)val3).value = 2.5f;
		BlurRadius = val3;
		FloatParameter val4 = new FloatParameter();
		((ParameterOverride<float>)val4).value = 0.75f;
		DistanceFalloff = val4;
		FloatParameter val5 = new FloatParameter();
		((ParameterOverride<float>)val5).value = 1.15f;
		Intensity = val5;
		((PostProcessEffectSettings)this)._002Ector();
	}
}
