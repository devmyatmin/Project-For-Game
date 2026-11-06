using System;
using UnityEngine;

namespace kode80.Clouds;

[Serializable]
public class TimeOfDayKeyFrame
{
	public float angle;

	public float sunSize;

	public float atmosphereThickness;

	public Color fogColor;

	public Color sunColor;

	public Color bottomColor;

	public Color topColor;

	public float SunScalar;

	public float AmbientScalar;
}
