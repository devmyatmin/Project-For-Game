using System;
using UnityEngine;

namespace Ceto;

[Serializable]
public struct AbsorptionModifier(float scale, float intensity, Color tint)
{
	[Range(0f, 50f)]
	public float scale = scale;

	[Range(0f, 10f)]
	public float intensity = intensity;

	public Color tint = tint;
}
