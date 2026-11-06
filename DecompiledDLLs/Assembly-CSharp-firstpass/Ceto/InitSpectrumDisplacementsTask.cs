using System;
using System.Collections;
using Ceto.Common.Threading.Tasks;
using UnityEngine;

namespace Ceto;

public class InitSpectrumDisplacementsTask : ThreadedTask
{
	private Color[] m_spectrum01;

	private Color[] m_spectrum23;

	private Color[] m_wtable;

	private Vector3[] m_ktable1;

	private Vector3[] m_ktable2;

	private Vector3[] m_ktable3;

	private Vector3[] m_ktable4;

	public int NumGrids { get; private set; }

	public int Size { get; private set; }

	public int LastUpdated { get; protected set; }

	public float TimeValue { get; protected set; }

	public SPECTRUM_TYPE SpectrumType { get; private set; }

	protected DisplacementBufferCPU Buffer { get; private set; }

	protected Vector4[] Data0 { get; set; }

	protected Vector4[] Data1 { get; set; }

	protected Vector4[] Data2 { get; set; }

	public InitSpectrumDisplacementsTask(DisplacementBufferCPU buffer, WaveSpectrumCondition condition, float time)
		: base(isThreaded: true)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Buffer = buffer;
		NumGrids = condition.Key.NumGrids;
		Size = condition.Key.Size;
		SpectrumType = condition.Key.SpectrumType;
		TimeValue = time;
		Reset(condition, time);
		CreateKTables(condition.InverseGridSizes());
	}

	public void Reset(WaveSpectrumCondition condition, float time)
	{
		if (condition.Key.SpectrumType != SpectrumType)
		{
			throw new InvalidOperationException("Trying to reset a Unified InitSpectrum task with wrong condition type = " + condition.Key.SpectrumType);
		}
		if (condition.Key.Size != Size)
		{
			throw new InvalidOperationException("Trying to reset a Unified InitSpectrum task with wrong condition size = " + condition.Key.Size);
		}
		base.Reset();
		int num = Size * Size;
		if (m_spectrum01 == null)
		{
			m_spectrum01 = new Color[num];
		}
		if (m_spectrum23 == null && NumGrids > 2)
		{
			m_spectrum23 = new Color[num];
		}
		if (m_wtable == null)
		{
			m_wtable = new Color[num];
		}
		TimeValue = time;
		Data0 = Buffer.GetReadBuffer(0);
		Data1 = Buffer.GetReadBuffer(1);
		Data2 = Buffer.GetReadBuffer(2);
		WaveSpectrumBufferCPU.Buffer buffer = Buffer.GetBuffer(0);
		WaveSpectrumBufferCPU.Buffer buffer2 = Buffer.GetBuffer(1);
		WaveSpectrumBufferCPU.Buffer buffer3 = Buffer.GetBuffer(2);
		if (buffer != null)
		{
			if (NumGrids > 2)
			{
				buffer.doublePacked = true;
			}
			else
			{
				buffer.doublePacked = false;
			}
		}
		if (buffer2 != null)
		{
			if (NumGrids > 1)
			{
				buffer2.doublePacked = true;
			}
			else
			{
				buffer2.doublePacked = false;
			}
		}
		if (buffer3 != null)
		{
			if (NumGrids > 3)
			{
				buffer3.doublePacked = true;
			}
			else
			{
				buffer3.doublePacked = false;
			}
		}
		if (LastUpdated != condition.LastUpdated)
		{
			LastUpdated = condition.LastUpdated;
			if (m_spectrum01 != null && condition.SpectrumData01 != null)
			{
				Array.Copy(condition.SpectrumData01, m_spectrum01, num);
			}
			if (m_spectrum23 != null && condition.SpectrumData23 != null)
			{
				Array.Copy(condition.SpectrumData23, m_spectrum23, num);
			}
			if (m_wtable != null && condition.WTableData != null)
			{
				Array.Copy(condition.WTableData, m_wtable, num);
			}
		}
	}

	public override IEnumerator Run()
	{
		if (NumGrids == 1)
		{
			InitilizeGrids1();
		}
		else if (NumGrids == 2)
		{
			InitilizeGrids2();
		}
		else if (NumGrids == 3)
		{
			InitilizeGrids3();
		}
		else if (NumGrids == 4)
		{
			InitilizeGrids4();
		}
		FinishedRunning();
		return null;
	}

	private void InitilizeGrids1()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		int size = Size;
		float num = 1f / (float)size;
		Vector2 val = default;
		Vector2 val2 = default;
		Vector2 val5 = default;
		Vector2 val7 = default;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				if (Cancelled)
				{
					return;
				}
				val.x = (float)j * num;
				val.y = (float)i * num;
				val2.x = ((val.x > 0.5f) ? (val.x - 1f) : val.x);
				val2.y = ((val.y > 0.5f) ? (val.y - 1f) : val.y);
				int num2 = j + i * size;
				int num3 = (size - j) % size + (size - i) % size * size;
				Color val3 = m_spectrum01[num2];
				Color val4 = m_spectrum01[num3];
				float num4 = m_wtable[num2].r * TimeValue;
				float num5 = Mathf.Cos(num4);
				float num6 = Mathf.Sin(num4);
				val5.x = (val3.r + val4.r) * num5 - (val3.g + val4.g) * num6;
				val5.y = (val3.r - val4.r) * num6 + (val3.g - val4.g) * num5;
				if (Data0 != null)
				{
					Data0[num2].x = val5.x;
					Data0[num2].y = val5.y;
					Data0[num2].z = 0f;
					Data0[num2].w = 0f;
				}
				if (Data1 != null)
				{
					Vector3 val6 = m_ktable1[num2];
					val7.x = 0f - val6.x * val5.y - val6.y * val5.x;
					val7.y = val6.x * val5.x - val6.y * val5.y;
					Data1[num2].x = val7.x * val6.z;
					Data1[num2].y = val7.y * val6.z;
					Data1[num2].z = 0f;
					Data1[num2].w = 0f;
				}
			}
		}
	}

	private void InitilizeGrids2()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		int size = Size;
		float num = 1f / (float)size;
		Vector2 val = default;
		Vector2 val2 = default;
		Vector2 val6 = default;
		Vector2 val7 = default;
		Vector2 val10 = default;
		Vector2 val11 = default;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				if (Cancelled)
				{
					return;
				}
				val.x = (float)j * num;
				val.y = (float)i * num;
				val2.x = ((val.x > 0.5f) ? (val.x - 1f) : val.x);
				val2.y = ((val.y > 0.5f) ? (val.y - 1f) : val.y);
				int num2 = j + i * size;
				int num3 = (size - j) % size + (size - i) % size * size;
				Color val3 = m_spectrum01[num2];
				Color val4 = m_spectrum01[num3];
				Color val5 = m_wtable[num2];
				val5.r *= TimeValue;
				val5.g *= TimeValue;
				float num4 = Mathf.Cos(val5.r);
				float num5 = Mathf.Sin(val5.r);
				val6.x = (val3.r + val4.r) * num4 - (val3.g + val4.g) * num5;
				val6.y = (val3.r - val4.r) * num5 + (val3.g - val4.g) * num4;
				num4 = Mathf.Cos(val5.g);
				num5 = Mathf.Sin(val5.g);
				val7.x = (val3.b + val4.b) * num4 - (val3.a + val4.a) * num5;
				val7.y = (val3.b - val4.b) * num5 + (val3.a - val4.a) * num4;
				if (Data0 != null)
				{
					Data0[num2].x = val6.x + (0f - val7.y);
					Data0[num2].y = val6.y + val7.x;
					Data0[num2].z = 0f;
					Data0[num2].w = 0f;
				}
				if (Data1 != null)
				{
					Vector3 val8 = m_ktable1[num2];
					Vector3 val9 = m_ktable2[num2];
					val10.x = 0f - val8.x * val6.y - val8.y * val6.x;
					val10.y = val8.x * val6.x - val8.y * val6.y;
					val11.x = 0f - val9.x * val7.y - val9.y * val7.x;
					val11.y = val9.x * val7.x - val9.y * val7.y;
					Data1[num2].x = val10.x * val8.z;
					Data1[num2].y = val10.y * val8.z;
					Data1[num2].z = val11.x * val9.z;
					Data1[num2].w = val11.y * val9.z;
				}
			}
		}
	}

	private void InitilizeGrids3()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		int size = Size;
		float num = 1f / (float)size;
		Vector2 val = default;
		Vector2 val2 = default;
		Vector2 val8 = default;
		Vector2 val9 = default;
		Vector2 val10 = default;
		Vector2 val13 = default;
		Vector2 val14 = default;
		Vector2 val16 = default;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				if (Cancelled)
				{
					return;
				}
				val.x = (float)j * num;
				val.y = (float)i * num;
				val2.x = ((val.x > 0.5f) ? (val.x - 1f) : val.x);
				val2.y = ((val.y > 0.5f) ? (val.y - 1f) : val.y);
				int num2 = j + i * size;
				int num3 = (size - j) % size + (size - i) % size * size;
				Color val3 = m_spectrum01[num2];
				Color val4 = m_spectrum23[num2];
				Color val5 = m_spectrum01[num3];
				Color val6 = m_spectrum23[num3];
				Color val7 = m_wtable[num2];
				val7.r *= TimeValue;
				val7.g *= TimeValue;
				val7.b *= TimeValue;
				val7.a *= TimeValue;
				float num4 = Mathf.Cos(val7.r);
				float num5 = Mathf.Sin(val7.r);
				val8.x = (val3.r + val5.r) * num4 - (val3.g + val5.g) * num5;
				val8.y = (val3.r - val5.r) * num5 + (val3.g - val5.g) * num4;
				num4 = Mathf.Cos(val7.g);
				num5 = Mathf.Sin(val7.g);
				val9.x = (val3.b + val5.b) * num4 - (val3.a + val5.a) * num5;
				val9.y = (val3.b - val5.b) * num5 + (val3.a - val5.a) * num4;
				num4 = Mathf.Cos(val7.b);
				num5 = Mathf.Sin(val7.b);
				val10.x = (val4.r + val6.r) * num4 - (val4.g + val6.g) * num5;
				val10.y = (val4.r - val6.r) * num5 + (val4.g - val6.g) * num4;
				if (Data0 != null)
				{
					Data0[num2].x = val8.x + (0f - val9.y);
					Data0[num2].y = val8.y + val9.x;
					Data0[num2].z = val10.x;
					Data0[num2].w = val10.y;
				}
				if (Data1 != null)
				{
					Vector3 val11 = m_ktable1[num2];
					Vector3 val12 = m_ktable2[num2];
					val13.x = 0f - val11.x * val8.y - val11.y * val8.x;
					val13.y = val11.x * val8.x - val11.y * val8.y;
					val14.x = 0f - val12.x * val9.y - val12.y * val9.x;
					val14.y = val12.x * val9.x - val12.y * val9.y;
					Data1[num2].x = val13.x * val11.z;
					Data1[num2].y = val13.y * val11.z;
					Data1[num2].z = val14.x * val12.z;
					Data1[num2].w = val14.y * val12.z;
				}
				if (Data2 != null)
				{
					Vector3 val15 = m_ktable3[num2];
					val16.x = 0f - val15.x * val10.y - val15.y * val10.x;
					val16.y = val15.x * val10.x - val15.y * val10.y;
					Data2[num2].x = val16.x * val15.z;
					Data2[num2].y = val16.y * val15.z;
					Data2[num2].z = 0f;
					Data2[num2].w = 0f;
				}
			}
		}
	}

	private void InitilizeGrids4()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_057e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		int size = Size;
		float num = 1f / (float)size;
		Vector2 val = default;
		Vector2 val2 = default;
		Vector2 val8 = default;
		Vector2 val9 = default;
		Vector2 val10 = default;
		Vector2 val11 = default;
		Vector2 val14 = default;
		Vector2 val15 = default;
		Vector2 val18 = default;
		Vector2 val19 = default;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				if (Cancelled)
				{
					return;
				}
				val.x = (float)j * num;
				val.y = (float)i * num;
				val2.x = ((val.x > 0.5f) ? (val.x - 1f) : val.x);
				val2.y = ((val.y > 0.5f) ? (val.y - 1f) : val.y);
				int num2 = j + i * size;
				int num3 = (size - j) % size + (size - i) % size * size;
				Color val3 = m_spectrum01[num2];
				Color val4 = m_spectrum23[num2];
				Color val5 = m_spectrum01[num3];
				Color val6 = m_spectrum23[num3];
				Color val7 = m_wtable[num2];
				val7.r *= TimeValue;
				val7.g *= TimeValue;
				val7.b *= TimeValue;
				val7.a *= TimeValue;
				float num4 = Mathf.Cos(val7.r);
				float num5 = Mathf.Sin(val7.r);
				val8.x = (val3.r + val5.r) * num4 - (val3.g + val5.g) * num5;
				val8.y = (val3.r - val5.r) * num5 + (val3.g - val5.g) * num4;
				num4 = Mathf.Cos(val7.g);
				num5 = Mathf.Sin(val7.g);
				val9.x = (val3.b + val5.b) * num4 - (val3.a + val5.a) * num5;
				val9.y = (val3.b - val5.b) * num5 + (val3.a - val5.a) * num4;
				num4 = Mathf.Cos(val7.b);
				num5 = Mathf.Sin(val7.b);
				val10.x = (val4.r + val6.r) * num4 - (val4.g + val6.g) * num5;
				val10.y = (val4.r - val6.r) * num5 + (val4.g - val6.g) * num4;
				num4 = Mathf.Cos(val7.a);
				num5 = Mathf.Sin(val7.a);
				val11.x = (val4.b + val6.b) * num4 - (val4.a + val6.a) * num5;
				val11.y = (val4.b - val6.b) * num5 + (val4.a - val6.a) * num4;
				if (Data0 != null)
				{
					Data0[num2].x = val8.x + (0f - val9.y);
					Data0[num2].y = val8.y + val9.x;
					Data0[num2].z = val10.x + (0f - val11.y);
					Data0[num2].w = val10.y + val11.x;
				}
				if (Data1 != null)
				{
					Vector3 val12 = m_ktable1[num2];
					Vector3 val13 = m_ktable2[num2];
					val14.x = 0f - val12.x * val8.y - val12.y * val8.x;
					val14.y = val12.x * val8.x - val12.y * val8.y;
					val15.x = 0f - val13.x * val9.y - val13.y * val9.x;
					val15.y = val13.x * val9.x - val13.y * val9.y;
					Data1[num2].x = val14.x * val12.z;
					Data1[num2].y = val14.y * val12.z;
					Data1[num2].z = val15.x * val13.z;
					Data1[num2].w = val15.y * val13.z;
				}
				if (Data2 != null)
				{
					Vector3 val16 = m_ktable3[num2];
					Vector3 val17 = m_ktable4[num2];
					val18.x = 0f - val16.x * val10.y - val16.y * val10.x;
					val18.y = val16.x * val10.x - val16.y * val10.y;
					val19.x = 0f - val17.x * val11.y - val17.y * val11.x;
					val19.y = val17.x * val11.x - val17.y * val11.y;
					Data2[num2].x = val18.x * val16.z;
					Data2[num2].y = val18.y * val16.z;
					Data2[num2].z = val19.x * val17.z;
					Data2[num2].w = val19.y * val17.z;
				}
			}
		}
	}

	private void CreateKTables(Vector4 inverseGridSizes)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		int size = Size;
		float num = 1f / (float)size;
		int numGrids = NumGrids;
		if (numGrids > 0)
		{
			m_ktable1 = new Vector3[size * size];
		}
		if (numGrids > 1)
		{
			m_ktable2 = new Vector3[size * size];
		}
		if (numGrids > 2)
		{
			m_ktable3 = new Vector3[size * size];
		}
		if (numGrids > 3)
		{
			m_ktable4 = new Vector3[size * size];
		}
		Vector2 val = default;
		Vector2 val2 = default;
		Vector2 val3 = default;
		Vector2 val4 = default;
		Vector2 val5 = default;
		Vector2 val6 = default;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				val.x = (float)j * num;
				val.y = (float)i * num;
				val2.x = ((val.x > 0.5f) ? (val.x - 1f) : val.x);
				val2.y = ((val.y > 0.5f) ? (val.y - 1f) : val.y);
				int num2 = j + i * size;
				if (numGrids > 0)
				{
					val3.x = val2.x * inverseGridSizes.x;
					val3.y = val2.y * inverseGridSizes.x;
					float num3 = Mathf.Sqrt(val3.x * val3.x + val3.y * val3.y);
					float z = ((num3 == 0f) ? 0f : (1f / num3));
					m_ktable1[num2].x = val3.x;
					m_ktable1[num2].y = val3.y;
					m_ktable1[num2].z = z;
				}
				if (numGrids > 1)
				{
					val4.x = val2.x * inverseGridSizes.y;
					val4.y = val2.y * inverseGridSizes.y;
					float num4 = Mathf.Sqrt(val4.x * val4.x + val4.y * val4.y);
					float z2 = ((num4 == 0f) ? 0f : (1f / num4));
					m_ktable2[num2].x = val4.x;
					m_ktable2[num2].y = val4.y;
					m_ktable2[num2].z = z2;
				}
				if (numGrids > 2)
				{
					val5.x = val2.x * inverseGridSizes.z;
					val5.y = val2.y * inverseGridSizes.z;
					float num5 = Mathf.Sqrt(val5.x * val5.x + val5.y * val5.y);
					float z3 = ((num5 == 0f) ? 0f : (1f / num5));
					m_ktable3[num2].x = val5.x;
					m_ktable3[num2].y = val5.y;
					m_ktable3[num2].z = z3;
				}
				if (numGrids > 3)
				{
					val6.x = val2.x * inverseGridSizes.w;
					val6.y = val2.y * inverseGridSizes.w;
					float num6 = Mathf.Sqrt(val6.x * val6.x + val6.y * val6.y);
					float z4 = ((num6 == 0f) ? 0f : (1f / num6));
					m_ktable4[num2].x = val6.x;
					m_ktable4[num2].y = val6.y;
					m_ktable4[num2].z = z4;
				}
			}
		}
	}
}
