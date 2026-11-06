using System;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

[Serializable]
[PostProcess(/*Could not decode attribute arguments.*/)]
public sealed class BlurSettings : PostProcessEffectSettings
{
	public IntParameter Downsample;

	public BlurModeParameter Mode;

	public IntParameter BlurIterations;

	public FloatParameter BlurSize;

	public FloatParameter Alpha;

	public TextureParameter BlurMask;

	public BlurSettings()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		//IL_0012: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		//IL_002f: Expected Obj, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0045: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected Obj, but got Unknown
		IntParameter val = new IntParameter();
		((ParameterOverride<int>)val).value = 1;
		Downsample = val;
		Mode = new BlurModeParameter();
		IntParameter val2 = new IntParameter();
		((ParameterOverride<int>)val2).value = 1;
		BlurIterations = val2;
		FloatParameter val3 = new FloatParameter();
		((ParameterOverride<float>)val3).value = 3f;
		BlurSize = val3;
		Alpha = new FloatParameter();
		BlurMask = new TextureParameter();
		((PostProcessEffectSettings)this)._002Ector();
	}
}
