using System;
using UnityEngine;

namespace Ceto;

[Serializable]
public struct CausticModifier(float aboveDistortion, float belowDistortion, float depthFade, float intensity, Color tint)
{
	[Range(0f, 3f)]
	public float aboveDistortion = aboveDistortion;

	[Range(0f, 3f)]
	public float belowDistortion = belowDistortion;

	[Range(0f, 1f)]
	public float depthFade = depthFade;

	[Range(0f, 3f)]
	public float intensity = intensity;

	public Color tint = tint;
}
