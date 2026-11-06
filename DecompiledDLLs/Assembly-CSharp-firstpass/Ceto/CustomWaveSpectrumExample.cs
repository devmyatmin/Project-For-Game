using System;
using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Components/CustomWaveSpectrumExample")]
[DisallowMultipleComponent]
[RequireComponent(typeof(Ocean))]
[RequireComponent(typeof(WaveSpectrum))]
public class CustomWaveSpectrumExample : MonoBehaviour, ICustomWaveSpectrum
{
	public class CustomSpectrumConditionKey : WaveSpectrumConditionKey
	{
		public float WindSpeed { get; private set; }

		public CustomSpectrumConditionKey(float windSpeed, int size, float windDir, SPECTRUM_TYPE spectrumType, int numGrids)
			: base(size, windDir, spectrumType, numGrids)
		{
			WindSpeed = windSpeed;
		}

		protected override bool Matches(WaveSpectrumConditionKey k)
		{
			CustomSpectrumConditionKey customSpectrumConditionKey = k as CustomSpectrumConditionKey;
			if (customSpectrumConditionKey == null)
			{
				return false;
			}
			if (WindSpeed != customSpectrumConditionKey.WindSpeed)
			{
				return false;
			}
			return true;
		}

		protected override int AddToHashCode(int hashcode)
		{
			hashcode = hashcode * 37 + WindSpeed.GetHashCode();
			return hashcode;
		}
	}

	public class CustomSpectrum : ISpectrum
	{
		private readonly float GRAVITY = 9.818286f;

		private readonly float AMP = 0.02f;

		private readonly float WindSpeed;

		private readonly Vector2 WindDir;

		private readonly float length2;

		private readonly float dampedLength2;

		public CustomSpectrum(float windSpeed, float windDir)
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

	[Range(0f, 30f)]
	public float windSpeed = 10f;

	public bool MultiThreadTask => true;

	private void Awake()
	{
		((Component)this).GetComponent<WaveSpectrum>().CustomWaveSpectrum = this;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public WaveSpectrumConditionKey CreateKey(int size, float windDir, SPECTRUM_TYPE spectrumType, int numGrids)
	{
		return new CustomSpectrumConditionKey(windSpeed, size, windDir, spectrumType, numGrids);
	}

	public ISpectrum CreateSpectrum(WaveSpectrumConditionKey key)
	{
		CustomSpectrumConditionKey obj = key as CustomSpectrumConditionKey;
		if (obj == null)
		{
			throw new InvalidCastException("Spectrum condition key is null or not the correct type");
		}
		float num = obj.WindSpeed;
		float windDir = obj.WindDir;
		return new CustomSpectrum(num, windDir);
	}

	public Vector4 GetGridSizes(int numGrids)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (numGrids == 4)
		{
			return new Vector4(1372f, 217f, 97f, 31f);
		}
		return new Vector4(217f, 97f, 31f, 1f);
	}

	public Vector4 GetChoppyness(int numGrids)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new Vector4(1.5f, 1.2f, 1f, 1f);
	}

	public Vector4 GetWaveAmps(int numGrids)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (numGrids == 4)
		{
			return new Vector4(0.5f, 1f, 1f, 1f);
		}
		return new Vector4(0.25f, 0.5f, 1f, 1f);
	}
}
