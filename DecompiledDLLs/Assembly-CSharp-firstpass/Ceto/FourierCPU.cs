using System;
using System.Collections.Generic;
using Ceto.Common.Threading.Tasks;
using UnityEngine;

namespace Ceto;

public class FourierCPU
{
	private struct LookUp
	{
		public int j1;

		public int j2;

		public float wr;

		public float wi;
	}

	private int m_size;

	private float m_fsize;

	private int m_passes;

	private LookUp[] m_butterflyLookupTable;

	public int size => m_size;

	public int passes => m_passes;

	public FourierCPU(int size)
	{
		if (!Mathf.IsPowerOfTwo(size))
		{
			throw new ArgumentException("Fourier grid size must be pow2 number");
		}
		m_size = size;
		m_fsize = m_size;
		m_passes = (int)(Mathf.Log(m_fsize) / Mathf.Log(2f));
		ComputeButterflyLookupTable();
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

	private void ComputeButterflyLookupTable()
	{
		m_butterflyLookupTable = new LookUp[m_size * m_passes];
		for (int i = 0; i < m_passes; i++)
		{
			int num = (int)Mathf.Pow(2f, (float)(m_passes - 1 - i));
			int num2 = (int)Mathf.Pow(2f, (float)i);
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num2; k++)
				{
					int num3;
					int num4;
					int j2;
					int j3;
					if (i == 0)
					{
						num3 = j * num2 * 2 + k;
						num4 = j * num2 * 2 + num2 + k;
						j2 = BitReverse(num3);
						j3 = BitReverse(num4);
					}
					else
					{
						num3 = j * num2 * 2 + k;
						num4 = j * num2 * 2 + num2 + k;
						j2 = num3;
						j3 = num4;
					}
					float num5 = Mathf.Cos((float)Math.PI * 2f * (float)(k * num) / m_fsize);
					float num6 = Mathf.Sin((float)Math.PI * 2f * (float)(k * num) / m_fsize);
					int num7 = num3 + i * m_size;
					m_butterflyLookupTable[num7].j1 = j2;
					m_butterflyLookupTable[num7].j2 = j3;
					m_butterflyLookupTable[num7].wr = num5;
					m_butterflyLookupTable[num7].wi = num6;
					int num8 = num4 + i * m_size;
					m_butterflyLookupTable[num8].j1 = j2;
					m_butterflyLookupTable[num8].j2 = j3;
					m_butterflyLookupTable[num8].wr = 0f - num5;
					m_butterflyLookupTable[num8].wi = 0f - num6;
				}
			}
		}
	}

	private Vector4 FFT(Vector2 w, Vector4 input1, Vector4 input2)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		input1.x += w.x * input2.x - w.y * input2.y;
		input1.y += w.y * input2.x + w.x * input2.y;
		input1.z += w.x * input2.z - w.y * input2.w;
		input1.w += w.y * input2.z + w.x * input2.w;
		return input1;
	}

	private Vector2 FFT(Vector2 w, Vector2 input1, Vector2 input2)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		input1.x += w.x * input2.x - w.y * input2.y;
		input1.y += w.y * input2.x + w.x * input2.y;
		return input1;
	}

	public int PeformFFT_SinglePacked(int startIdx, IList<Vector4[]> data0, ICancelToken token)
	{
		int num = 0;
		int num2 = startIdx;
		int num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int index = (num2 + 1) % 2;
			Vector4[] array = data0[num];
			Vector4[] array2 = data0[index];
			int num4 = num3 * m_size;
			for (int i = 0; i < m_size; i++)
			{
				int num5 = i + num4;
				int j = m_butterflyLookupTable[num5].j1;
				int j2 = m_butterflyLookupTable[num5].j2;
				float wr = m_butterflyLookupTable[num5].wr;
				float wi = m_butterflyLookupTable[num5].wi;
				for (int k = 0; k < m_size; k++)
				{
					if (token.Cancelled)
					{
						return -1;
					}
					int num6 = k * m_size;
					int num7 = i + num6;
					int num8 = j + num6;
					int num9 = j2 + num6;
					array[num7].x = array2[num8].x + wr * array2[num9].x - wi * array2[num9].y;
					array[num7].y = array2[num8].y + wi * array2[num9].x + wr * array2[num9].y;
				}
			}
			num3++;
			num2++;
		}
		num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int index = (num2 + 1) % 2;
			Vector4[] array3 = data0[num];
			Vector4[] array4 = data0[index];
			int num4 = num3 * m_size;
			for (int k = 0; k < m_size; k++)
			{
				int num5 = k + num4;
				int j = m_butterflyLookupTable[num5].j1 * m_size;
				int j2 = m_butterflyLookupTable[num5].j2 * m_size;
				float wr = m_butterflyLookupTable[num5].wr;
				float wi = m_butterflyLookupTable[num5].wi;
				for (int i = 0; i < m_size; i++)
				{
					if (token.Cancelled)
					{
						return -1;
					}
					int num7 = i + k * m_size;
					int num8 = i + j;
					int num9 = i + j2;
					array3[num7].x = array4[num8].x + wr * array4[num9].x - wi * array4[num9].y;
					array3[num7].y = array4[num8].y + wi * array4[num9].x + wr * array4[num9].y;
				}
			}
			num3++;
			num2++;
		}
		return num;
	}

	public int PeformFFT_DoublePacked(int startIdx, IList<Vector4[]> data0, ICancelToken token)
	{
		int num = 0;
		int num2 = startIdx;
		int num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int index = (num2 + 1) % 2;
			Vector4[] array = data0[num];
			Vector4[] array2 = data0[index];
			int num4 = num3 * m_size;
			for (int i = 0; i < m_size; i++)
			{
				int num5 = i + num4;
				int j = m_butterflyLookupTable[num5].j1;
				int j2 = m_butterflyLookupTable[num5].j2;
				float wr = m_butterflyLookupTable[num5].wr;
				float wi = m_butterflyLookupTable[num5].wi;
				for (int k = 0; k < m_size; k++)
				{
					if (token.Cancelled)
					{
						return -1;
					}
					int num6 = k * m_size;
					int num7 = i + num6;
					int num8 = j + num6;
					int num9 = j2 + num6;
					array[num7].x = array2[num8].x + wr * array2[num9].x - wi * array2[num9].y;
					array[num7].y = array2[num8].y + wi * array2[num9].x + wr * array2[num9].y;
					array[num7].z = array2[num8].z + wr * array2[num9].z - wi * array2[num9].w;
					array[num7].w = array2[num8].w + wi * array2[num9].z + wr * array2[num9].w;
				}
			}
			num3++;
			num2++;
		}
		num3 = 0;
		while (num3 < m_passes)
		{
			num = num2 % 2;
			int index = (num2 + 1) % 2;
			Vector4[] array3 = data0[num];
			Vector4[] array4 = data0[index];
			int num4 = num3 * m_size;
			for (int k = 0; k < m_size; k++)
			{
				int num5 = k + num4;
				int j = m_butterflyLookupTable[num5].j1 * m_size;
				int j2 = m_butterflyLookupTable[num5].j2 * m_size;
				float wr = m_butterflyLookupTable[num5].wr;
				float wi = m_butterflyLookupTable[num5].wi;
				for (int i = 0; i < m_size; i++)
				{
					if (token.Cancelled)
					{
						return -1;
					}
					int num7 = i + k * m_size;
					int num8 = i + j;
					int num9 = i + j2;
					array3[num7].x = array4[num8].x + wr * array4[num9].x - wi * array4[num9].y;
					array3[num7].y = array4[num8].y + wi * array4[num9].x + wr * array4[num9].y;
					array3[num7].z = array4[num8].z + wr * array4[num9].z - wi * array4[num9].w;
					array3[num7].w = array4[num8].w + wi * array4[num9].z + wr * array4[num9].w;
				}
			}
			num3++;
			num2++;
		}
		return num;
	}
}
