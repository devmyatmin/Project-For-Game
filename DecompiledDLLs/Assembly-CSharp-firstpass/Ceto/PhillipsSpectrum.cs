using System;
using UnityEngine;

namespace Ceto;

public class PhillipsSpectrum : ISpectrum
{
	private readonly float GRAVITY = 9.818286f;

	private readonly float AMP = 0.02f;

	private readonly float WindSpeed;

	private readonly Vector2 WindDir;

	private readonly float length2;

	private readonly float dampedLength2;

	public PhillipsSpectrum(float windSpeed, float windDir)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		WindSpeed = windSpeed;
		float num = windDir * (float)Math.PI / 180f;
		WindDir = new Vector2(Mathf.Cos(num), Mathf.Sin(num));
		float num2 = WindSpeed * WindSpeed / GRAVITY;
		length2 = num2 * num2;
		float num3 = 0.001f;
		dampedLength2 = length2 * num3 * num3;
	}

	public float Spectrum(float kx, float kz)
	{
		float num = kx * WindDir.x - kz * WindDir.y;
		float num2 = kx * WindDir.y + kz * WindDir.x;
		kx = num;
		kz = num2;
		float num3 = Mathf.Sqrt(kx * kx + kz * kz);
		if (num3 < 1E-06f)
		{
			return 0f;
		}
		float num4 = num3 * num3;
		float num5 = num4 * num4;
		kx /= num3;
		kz /= num3;
		float num6 = kx * 1f + kz * 0f;
		float num7 = num6 * num6 * num6 * num6 * num6 * num6;
		return AMP * Mathf.Exp(-1f / (num4 * length2)) / num5 * num7 * Mathf.Exp((0f - num4) * dampedLength2);
	}
}
