using System;
using UnityEngine;

namespace Ceto;

public class UnifiedSpectrum : ISpectrum
{
	private readonly float GRAVITY = 9.818286f;

	private readonly float WAVE_CM = 0.23f;

	private readonly float WAVE_KM = 370f;

	private readonly float U10;

	private readonly float PI_2;

	private readonly float SQRT_10;

	private readonly float G_SQ_OMEGA_U10;

	private readonly float Z_SQ_U10_G;

	private readonly float LOG_OMEGA_6;

	private readonly float SIGMA;

	private readonly float SQ_SIGMA_2;

	private readonly float ALPHA_P;

	private readonly float LOG_2_4;

	private readonly float kp;

	private readonly float cp;

	private readonly float z0;

	private readonly float u_star;

	private readonly float gamma;

	private readonly float HALF_ALPHA_P_CP;

	private readonly float alpham;

	private readonly float HALF_ALPHAM_WAVE_CM;

	private readonly float am;

	private readonly float WindSpeed;

	private readonly float WaveAge;

	private readonly Vector2 WindDir;

	public UnifiedSpectrum(float windSpeed, float windDir, float waveAge)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		WindSpeed = windSpeed;
		WaveAge = waveAge;
		float num = windDir * (float)Math.PI / 180f;
		WindDir = new Vector2(Mathf.Cos(num), Mathf.Sin(num));
		U10 = WindSpeed;
		PI_2 = (float)Math.PI * 2f;
		SQRT_10 = Mathf.Sqrt(10f);
		G_SQ_OMEGA_U10 = GRAVITY * sqr(WaveAge / U10);
		Z_SQ_U10_G = 3.7E-05f * sqr(U10) / 9.81f;
		LOG_OMEGA_6 = Mathf.Log(WaveAge) * 6f;
		SIGMA = 0.08f * (1f + 4f / Mathf.Pow(WaveAge, 3f));
		SQ_SIGMA_2 = sqr(SIGMA) * 2f;
		ALPHA_P = 0.006f * Mathf.Sqrt(WaveAge);
		LOG_2_4 = Mathf.Log(2f) / 4f;
		kp = G_SQ_OMEGA_U10;
		cp = omega(kp) / kp;
		z0 = Z_SQ_U10_G * Mathf.Pow(U10 / cp, 0.9f);
		u_star = 0.41f * U10 / Mathf.Log(10f / z0);
		gamma = ((WaveAge < 1f) ? 1.7f : (1.7f + LOG_OMEGA_6));
		HALF_ALPHA_P_CP = 0.5f * ALPHA_P * cp;
		alpham = 0.01f * ((u_star < WAVE_CM) ? (1f + Mathf.Log(u_star / WAVE_CM)) : (1f + 3f * Mathf.Log(u_star / WAVE_CM)));
		HALF_ALPHAM_WAVE_CM = 0.5f * alpham * WAVE_CM;
		am = 0.13f * u_star / WAVE_CM;
	}

	private float sqr(float x)
	{
		return x * x;
	}

	private float omega(float k)
	{
		return Mathf.Sqrt(GRAVITY * k * (1f + sqr(k / WAVE_KM)));
	}

	public float Spectrum(float kx, float ky)
	{
		float num = kx * WindDir.x - ky * WindDir.y;
		float num2 = kx * WindDir.y + ky * WindDir.x;
		kx = num;
		ky = num2;
		float num3 = Mathf.Sqrt(kx * kx + ky * ky);
		float num4 = omega(num3) / num3;
		float num5 = Mathf.Exp(-1.25f * sqr(kp / num3));
		float num6 = Mathf.Exp(-1f / SQ_SIGMA_2 * sqr(Mathf.Sqrt(num3 / kp) - 1f));
		float num7 = Mathf.Pow(gamma, num6);
		float num8 = num5 * num7 * Mathf.Exp((0f - WaveAge) / SQRT_10 * (Mathf.Sqrt(num3 / kp) - 1f));
		float num9 = HALF_ALPHA_P_CP / num4 * num8;
		float num10 = Mathf.Exp(-0.25f * sqr(num3 / WAVE_KM - 1f));
		float num11 = HALF_ALPHAM_WAVE_CM / num4 * num10 * num5;
		float num12 = (float)Math.Tanh(LOG_2_4 + 4f * Mathf.Pow(num4 / cp, 2.5f) + am * Mathf.Pow(WAVE_CM / num4, 2.5f));
		float num13 = Mathf.Atan2(ky, kx);
		if (kx < 0f)
		{
			return 0f;
		}
		num9 *= 2f;
		num11 *= 2f;
		float num14 = Mathf.Sqrt(Mathf.Max(kx / num3, 0f));
		return (num9 + num11) * (1f + num12 * Mathf.Cos(2f * num13)) / (PI_2 * num3 * num3 * num3 * num3) * num14;
	}
}
