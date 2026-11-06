using System;
using UnityEngine;

namespace Ceto;

[Serializable]
public struct InscatterModifier(float scale, float intensity, Color color, INSCATTER_MODE mode)
{
	[Range(0f, 5000f)]
	public float scale = scale;

	[Range(0f, 2f)]
	public float intensity = intensity;

	public INSCATTER_MODE mode = mode;

	public Color color = color;
}
