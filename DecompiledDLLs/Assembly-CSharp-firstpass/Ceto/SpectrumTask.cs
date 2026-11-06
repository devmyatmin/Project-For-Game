using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Ceto.Common.Threading.Tasks;
using UnityEngine;

namespace Ceto;

public class SpectrumTask : ThreadedTask
{
	public const float GRAVITY = 9.818286f;

	public const float WAVE_CM = 0.23f;

	public const float WAVE_KM = 370f;

	private Color[] m_spectrum01;

	private Color[] m_spectrum23;

	private Color[] m_wtable;

	private ISpectrum[] m_spectrums;

	private Random m_rnd;

	private SPECTRUM_DISTRIBUTION m_distibution;

	public int Size { get; private set; }

	public int NumGrids { get; private set; }

	public Vector4 GridSizes
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public Vector4 InverseGridSizes
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public Vector4 WaveAmps
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	protected WaveSpectrumCondition Condition { get; private set; }

	public SpectrumTask(WaveSpectrumCondition condition, bool multiThreadTask, ISpectrum[] spectrums)
		: base(multiThreadTask)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (spectrums == null || spectrums.Length != 4)
		{
			throw new ArgumentException("Spectrums array must have a length of 4");
		}
		m_spectrums = spectrums;
		Condition = condition;
		Size = condition.Key.Size;
		GridSizes = condition.GridSizes;
		WaveAmps = condition.WaveAmps;
		NumGrids = condition.Key.NumGrids;
		m_rnd = new Random(0);
		m_distibution = SPECTRUM_DISTRIBUTION.LINEAR;
		float num = (float)Math.PI * 2f * (float)Size;
		InverseGridSizes = new Vector4(num / GridSizes.x, num / GridSizes.y, num / GridSizes.z, num / GridSizes.w);
		m_spectrum01 = new Color[Size * Size];
		if (NumGrids > 2)
		{
			m_spectrum23 = new Color[Size * Size];
		}
		m_wtable = new Color[Size * Size];
	}

	protected float RandomNumber()
	{
		return m_distibution switch
		{
			SPECTRUM_DISTRIBUTION.LINEAR => (float)m_rnd.NextDouble(), 
			SPECTRUM_DISTRIBUTION.GAUSSIAN => GaussianRandomNumber(), 
			_ => (float)m_rnd.NextDouble(), 
		};
	}

	private float GaussianRandomNumber()
	{
		float num;
		float num3;
		do
		{
			num = 2f * (float)m_rnd.NextDouble() - 1f;
			float num2 = 2f * (float)m_rnd.NextDouble() - 1f;
			num3 = num * num + num2 * num2;
		}
		while (num3 >= 1f);
		num3 = Mathf.Sqrt(-2f * Mathf.Log(num3) / num3);
		return num * num3;
	}

	public override void Start()
	{
		base.Start();
		Condition.CreateTextures();
		Condition.Done = false;
	}

	public override IEnumerator Run()
	{
		CreateWTable();
		GenerateWavesSpectrum();
		FinishedRunning();
		return null;
	}

	public override void End()
	{
		base.End();
		Condition.Apply(m_spectrum01, m_spectrum23, m_wtable);
		Condition.LastUpdated = Time.frameCount;
		Condition.Done = true;
	}

	private float GetSpectrumSample(float i, float j, float dk, float kMin, float amp, ISpectrum spectrum)
	{
		if (spectrum == null)
		{
			return 0f;
		}
		float num = i * dk;
		float num2 = j * dk;
		float num3 = 0f;
		if (Math.Abs(num) >= kMin || Math.Abs(num2) >= kMin)
		{
			num3 = Mathf.Sqrt(spectrum.Spectrum(num, num2) * amp * 0.5f) * dk;
			if (float.IsNaN(num3) || float.IsInfinity(num3))
			{
				num3 = 0f;
			}
		}
		return num3;
	}

	private void GenerateWavesSpectrum()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		int size = Size;
		int num = size / 2;
		float num2 = size;
		int numGrids = NumGrids;
		float num3 = (float)Math.PI * 2f;
		float kMin = (float)Math.PI / GridSizes.x;
		float kMin2 = (float)Math.PI * num2 / GridSizes.x;
		float kMin3 = (float)Math.PI * num2 / GridSizes.y;
		float kMin4 = (float)Math.PI * num2 / GridSizes.z;
		float dk = num3 / GridSizes.x;
		float dk2 = num3 / GridSizes.y;
		float dk3 = num3 / GridSizes.z;
		float dk4 = num3 / GridSizes.w;
		float x = WaveAmps.x;
		float y = WaveAmps.y;
		float z = WaveAmps.z;
		float w = WaveAmps.w;
		Vector4 val = default;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				if (Cancelled)
				{
					return;
				}
				int num4 = j + i * size;
				float i2 = ((j >= num) ? ((float)j - num2) : ((float)j));
				float j2 = ((i >= num) ? ((float)i - num2) : ((float)i));
				if (numGrids > 0)
				{
					float num5 = RandomNumber() * num3;
					val.x = GetSpectrumSample(i2, j2, dk, kMin, x, m_spectrums[0]);
					m_spectrum01[num4].r = val.x * Mathf.Cos(num5) * 1.4142135f;
					m_spectrum01[num4].g = val.x * Mathf.Sin(num5) * 1.4142135f;
				}
				if (numGrids > 1)
				{
					float num5 = RandomNumber() * num3;
					val.y = GetSpectrumSample(i2, j2, dk2, kMin2, y, m_spectrums[1]);
					m_spectrum01[num4].b = val.y * Mathf.Cos(num5) * 1.4142135f;
					m_spectrum01[num4].a = val.y * Mathf.Sin(num5) * 1.4142135f;
				}
				if (numGrids > 2)
				{
					float num5 = RandomNumber() * num3;
					val.z = GetSpectrumSample(i2, j2, dk3, kMin3, z, m_spectrums[2]);
					m_spectrum23[num4].r = val.z * Mathf.Cos(num5) * 1.4142135f;
					m_spectrum23[num4].g = val.z * Mathf.Sin(num5) * 1.4142135f;
				}
				if (numGrids > 3)
				{
					float num5 = RandomNumber() * num3;
					val.w = GetSpectrumSample(i2, j2, dk4, kMin4, w, m_spectrums[3]);
					m_spectrum23[num4].b = val.w * Mathf.Cos(num5) * 1.4142135f;
					m_spectrum23[num4].a = val.w * Mathf.Sin(num5) * 1.4142135f;
				}
			}
		}
	}

	private void CreateWTable()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		int size = Size;
		float num = size;
		float num2 = 1f / num;
		int numGrids = NumGrids;
		Vector4 val = default;
		val.x = InverseGridSizes.x * InverseGridSizes.x;
		val.y = InverseGridSizes.y * InverseGridSizes.y;
		val.z = InverseGridSizes.z * InverseGridSizes.z;
		val.w = InverseGridSizes.w * InverseGridSizes.w;
		float num3 = 136900f;
		Vector2 val2 = default;
		Vector2 val3 = default;
		Vector2 val4 = default;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				if (Cancelled)
				{
					return;
				}
				int num4 = j + i * size;
				val2.x = (float)j * num2;
				val2.y = (float)i * num2;
				val3.x = ((val2.x > 0.5f) ? (val2.x - 1f) : val2.x);
				val3.y = ((val2.y > 0.5f) ? (val2.y - 1f) : val2.y);
				val4.x = val3.x * val3.x;
				val4.y = val3.y * val3.y;
				if (numGrids > 0)
				{
					float num5 = Mathf.Sqrt(val4.x * val.x + val4.y * val.x);
					float r = Mathf.Sqrt(9.818286f * num5 * (1f + num5 * num5 / num3));
					m_wtable[num4].r = r;
				}
				if (numGrids > 1)
				{
					float num6 = Mathf.Sqrt(val4.x * val.y + val4.y * val.y);
					float g = Mathf.Sqrt(9.818286f * num6 * (1f + num6 * num6 / num3));
					m_wtable[num4].g = g;
				}
				if (numGrids > 2)
				{
					float num7 = Mathf.Sqrt(val4.x * val.z + val4.y * val.z);
					float b = Mathf.Sqrt(9.818286f * num7 * (1f + num7 * num7 / num3));
					m_wtable[num4].b = b;
				}
				if (numGrids > 3)
				{
					float num8 = Mathf.Sqrt(val4.x * val.w + val4.y * val.w);
					float a = Mathf.Sqrt(9.818286f * num8 * (1f + num8 * num8 / num3));
					m_wtable[num4].a = a;
				}
			}
		}
	}
}
