using System;
using Ceto.Common.Unity.Utility;
using UnityEngine;

namespace Ceto;

public class FourierGPU
{
	private const int PASS_X_1 = 0;

	private const int PASS_Y_1 = 1;

	private const int PASS_X_2 = 2;

	private const int PASS_Y_2 = 3;

	private const int PASS_X_3 = 4;

	private const int PASS_Y_3 = 5;

	private const int PASS_X_4 = 6;

	private const int PASS_Y_4 = 7;

	private int m_size;

	private float m_fsize;

	private int m_passes;

	private Texture2D[] m_butterflyLookupTable;

	private Material m_fourier;

	private RenderBuffer[] m_pass0RT2;

	private RenderBuffer[] m_pass1RT2;

	private RenderBuffer[] m_pass0RT3;

	private RenderBuffer[] m_pass1RT3;

	private RenderBuffer[] m_pass0RT4;

	private RenderBuffer[] m_pass1RT4;

	public int size => m_size;

	public int passes => m_passes;

	public FourierGPU(int size, Shader sdr)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected Obj, but got Unknown
		if (!Mathf.IsPowerOfTwo(size))
		{
			throw new ArgumentException("Fourier grid size must be pow2 number");
		}
		m_fourier = new Material(sdr);
		m_size = size;
		m_fsize = m_size;
		m_passes = (int)(Mathf.Log(m_fsize) / Mathf.Log(2f));
		m_butterflyLookupTable = new Texture2D[m_passes];
		ComputeButterflyLookupTable();
		m_fourier.SetFloat("Ceto_FourierSize", m_fsize);
		m_pass0RT2 = new RenderBuffer[2];
		m_pass1RT2 = new RenderBuffer[2];
		m_pass0RT3 = new RenderBuffer[3];
		m_pass1RT3 = new RenderBuffer[3];
		m_pass0RT4 = new RenderBuffer[4];
		m_pass1RT4 = new RenderBuffer[4];
	}

	public void Release()
	{
		int num = m_butterflyLookupTable.Length;
		for (int i = 0; i < num; i++)
		{
			Object.Destroy((Object)(object)m_butterflyLookupTable[i]);
		}
		m_butterflyLookupTable = null;
	}

	private int BitReverse(int i)
	{
		int num = i;
		int num2 = 0;
		int num3 = 1;
		for (int num4 = m_size / 2; num4 != 0; num4 /= 2)
		{
			num = (((i & num4) > num4 - 1) ? 1 : 0);
			num2 += num * num3;
			num3 *= 2;
		}
		return num2;
	}

	private Texture2D Make1DTex(int i)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected Obj, but got Unknown
		return new Texture2D(m_size, 1, (TextureFormat)20, false, true)
		{
			filterMode = (FilterMode)0,
			wrapMode = (TextureWrapMode)1,
			hideFlags = (HideFlags)61,
			name = "Ceto Fouier GPU Butterfly Lookup"
		};
	}

	private void ComputeButterflyLookupTable()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		float num = m_size;
		float num2 = m_size - 1;
		for (int i = 0; i < m_passes; i++)
		{
			int num3 = (int)Mathf.Pow(2f, (float)(m_passes - 1 - i));
			int num4 = (int)Mathf.Pow(2f, (float)i);
			m_butterflyLookupTable[i] = Make1DTex(i);
			for (int j = 0; j < num3; j++)
			{
				for (int k = 0; k < num4; k++)
				{
					int num5;
					int num6;
					int num7;
					int num8;
					if (i == 0)
					{
						num5 = j * num4 * 2 + k;
						num6 = j * num4 * 2 + num4 + k;
						num7 = BitReverse(num5);
						num8 = BitReverse(num6);
					}
					else
					{
						num5 = j * num4 * 2 + k;
						num6 = j * num4 * 2 + num4 + k;
						num7 = num5;
						num8 = num6;
					}
					float num9 = Mathf.Cos((float)Math.PI * 2f * (float)(k * num3) / num);
					float num10 = Mathf.Sin((float)Math.PI * 2f * (float)(k * num3) / num);
					m_butterflyLookupTable[i].SetPixel(num5, 0, new Color((float)num7 / num2, (float)num8 / num2, num9, num10));
					m_butterflyLookupTable[i].SetPixel(num6, 0, new Color((float)num7 / num2, (float)num8 / num2, 0f - num9, 0f - num10));
				}
			}
			m_butterflyLookupTable[i].Apply();
		}
	}

	public int PeformFFT(RenderTexture[] data0)
	{
		if (m_butterflyLookupTable == null)
		{
			return -1;
		}
		RenderTexture val = data0[0];
		RenderTexture val2 = data0[1];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int num4 = (num2 + 1) % 2;
			m_fourier.SetTexture("Ceto_ButterFlyLookUp", (Texture)(object)m_butterflyLookupTable[num3]);
			m_fourier.SetTexture("Ceto_ReadBuffer0", (Texture)(object)data0[num4]);
			if (num == 0)
			{
				Graphics.Blit((Texture)null, val, m_fourier, 0);
			}
			else
			{
				Graphics.Blit((Texture)null, val2, m_fourier, 0);
			}
			num3++;
			num2++;
		}
		num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int num4 = (num2 + 1) % 2;
			m_fourier.SetTexture("Ceto_ButterFlyLookUp", (Texture)(object)m_butterflyLookupTable[num3]);
			m_fourier.SetTexture("Ceto_ReadBuffer0", (Texture)(object)data0[num4]);
			if (num == 0)
			{
				Graphics.Blit((Texture)null, val, m_fourier, 1);
			}
			else
			{
				Graphics.Blit((Texture)null, val2, m_fourier, 1);
			}
			num3++;
			num2++;
		}
		return num;
	}

	public int PeformFFT(RenderTexture[] data0, RenderTexture[] data1)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		if (m_butterflyLookupTable == null)
		{
			return -1;
		}
		if (SystemInfo.supportedRenderTargetCount < 2)
		{
			throw new InvalidOperationException("System does not support at least 2 render targets");
		}
		m_pass0RT2[0] = data0[0].colorBuffer;
		m_pass0RT2[1] = data1[0].colorBuffer;
		m_pass1RT2[0] = data0[1].colorBuffer;
		m_pass1RT2[1] = data1[1].colorBuffer;
		RenderBuffer depthBuffer = data0[0].depthBuffer;
		RenderBuffer depthBuffer2 = data0[1].depthBuffer;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int num4 = (num2 + 1) % 2;
			m_fourier.SetTexture("Ceto_ButterFlyLookUp", (Texture)(object)m_butterflyLookupTable[num3]);
			m_fourier.SetTexture("Ceto_ReadBuffer0", (Texture)(object)data0[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer1", (Texture)(object)data1[num4]);
			if (num == 0)
			{
				RTUtility.MultiTargetBlit(m_pass0RT2, depthBuffer, m_fourier, 2);
			}
			else
			{
				RTUtility.MultiTargetBlit(m_pass1RT2, depthBuffer2, m_fourier, 2);
			}
			num3++;
			num2++;
		}
		num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int num4 = (num2 + 1) % 2;
			m_fourier.SetTexture("Ceto_ButterFlyLookUp", (Texture)(object)m_butterflyLookupTable[num3]);
			m_fourier.SetTexture("Ceto_ReadBuffer0", (Texture)(object)data0[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer1", (Texture)(object)data1[num4]);
			if (num == 0)
			{
				RTUtility.MultiTargetBlit(m_pass0RT2, depthBuffer, m_fourier, 3);
			}
			else
			{
				RTUtility.MultiTargetBlit(m_pass1RT2, depthBuffer2, m_fourier, 3);
			}
			num3++;
			num2++;
		}
		return num;
	}

	public int PeformFFT(RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		if (m_butterflyLookupTable == null)
		{
			return -1;
		}
		if (SystemInfo.supportedRenderTargetCount < 3)
		{
			throw new InvalidOperationException("System does not support at least 3 render targets");
		}
		m_pass0RT3[0] = data0[0].colorBuffer;
		m_pass0RT3[1] = data1[0].colorBuffer;
		m_pass0RT3[2] = data2[0].colorBuffer;
		m_pass1RT3[0] = data0[1].colorBuffer;
		m_pass1RT3[1] = data1[1].colorBuffer;
		m_pass1RT3[2] = data2[1].colorBuffer;
		RenderBuffer depthBuffer = data0[0].depthBuffer;
		RenderBuffer depthBuffer2 = data0[1].depthBuffer;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int num4 = (num2 + 1) % 2;
			m_fourier.SetTexture("Ceto_ButterFlyLookUp", (Texture)(object)m_butterflyLookupTable[num3]);
			m_fourier.SetTexture("Ceto_ReadBuffer0", (Texture)(object)data0[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer1", (Texture)(object)data1[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer2", (Texture)(object)data2[num4]);
			if (num == 0)
			{
				RTUtility.MultiTargetBlit(m_pass0RT3, depthBuffer, m_fourier, 4);
			}
			else
			{
				RTUtility.MultiTargetBlit(m_pass1RT3, depthBuffer2, m_fourier, 4);
			}
			num3++;
			num2++;
		}
		num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int num4 = (num2 + 1) % 2;
			m_fourier.SetTexture("Ceto_ButterFlyLookUp", (Texture)(object)m_butterflyLookupTable[num3]);
			m_fourier.SetTexture("Ceto_ReadBuffer0", (Texture)(object)data0[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer1", (Texture)(object)data1[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer2", (Texture)(object)data2[num4]);
			if (num == 0)
			{
				RTUtility.MultiTargetBlit(m_pass0RT3, depthBuffer, m_fourier, 5);
			}
			else
			{
				RTUtility.MultiTargetBlit(m_pass1RT3, depthBuffer2, m_fourier, 5);
			}
			num3++;
			num2++;
		}
		return num;
	}

	public int PeformFFT(RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2, RenderTexture[] data3)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		if (m_butterflyLookupTable == null)
		{
			return -1;
		}
		if (SystemInfo.supportedRenderTargetCount < 4)
		{
			throw new InvalidOperationException("System does not support at least 4 render targets");
		}
		m_pass0RT4[0] = data0[0].colorBuffer;
		m_pass0RT4[1] = data1[0].colorBuffer;
		m_pass0RT4[2] = data2[0].colorBuffer;
		m_pass0RT4[3] = data3[0].colorBuffer;
		m_pass1RT4[0] = data0[1].colorBuffer;
		m_pass1RT4[1] = data1[1].colorBuffer;
		m_pass1RT4[2] = data2[1].colorBuffer;
		m_pass1RT4[3] = data3[1].colorBuffer;
		RenderBuffer depthBuffer = data0[0].depthBuffer;
		RenderBuffer depthBuffer2 = data0[1].depthBuffer;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int num4 = (num2 + 1) % 2;
			m_fourier.SetTexture("Ceto_ButterFlyLookUp", (Texture)(object)m_butterflyLookupTable[num3]);
			m_fourier.SetTexture("Ceto_ReadBuffer0", (Texture)(object)data0[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer1", (Texture)(object)data1[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer2", (Texture)(object)data2[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer3", (Texture)(object)data3[num4]);
			if (num == 0)
			{
				RTUtility.MultiTargetBlit(m_pass0RT4, depthBuffer, m_fourier, 6);
			}
			else
			{
				RTUtility.MultiTargetBlit(m_pass1RT4, depthBuffer2, m_fourier, 6);
			}
			num3++;
			num2++;
		}
		num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int num4 = (num2 + 1) % 2;
			m_fourier.SetTexture("Ceto_ButterFlyLookUp", (Texture)(object)m_butterflyLookupTable[num3]);
			m_fourier.SetTexture("Ceto_ReadBuffer0", (Texture)(object)data0[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer1", (Texture)(object)data1[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer2", (Texture)(object)data2[num4]);
			m_fourier.SetTexture("Ceto_ReadBuffer3", (Texture)(object)data3[num4]);
			if (num == 0)
			{
				RTUtility.MultiTargetBlit(m_pass0RT4, depthBuffer, m_fourier, 7);
			}
			else
			{
				RTUtility.MultiTargetBlit(m_pass1RT4, depthBuffer2, m_fourier, 7);
			}
			num3++;
			num2++;
		}
		return num;
	}
}
