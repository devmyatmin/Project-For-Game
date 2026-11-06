using System;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

[Serializable]
public sealed class BlurModeParameter : ParameterOverride<BlurEffect.Mode>
{
}
