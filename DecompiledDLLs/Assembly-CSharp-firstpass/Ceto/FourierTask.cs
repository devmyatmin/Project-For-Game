using System;
using System.Collections;
using System.Collections.Generic;
using Ceto.Common.Containers.Interpolation;
using Ceto.Common.Threading.Tasks;
using UnityEngine;

namespace Ceto;

public class FourierTask : ThreadedTask
{
	private FourierCPU m_fourier;

	private DisplacementBufferCPU m_buffer;

	private int m_numGrids;

	private int m_index;

	private IList<Vector4[]> m_data;

	private Color[] m_results;

	private Texture2D m_map;

	private bool m_doublePacked;

	public FourierTask(WaveSpectrumBufferCPU buffer, FourierCPU fourier, int index, int numGrids)
		: base(isThreaded: true)
	{
		if (m_index == -1)
		{
			throw new InvalidOperationException("Index can be -1. Fourier for multiple buffers is not being used");
		}
		if (!(buffer is DisplacementBufferCPU))
		{
			throw new InvalidOperationException("Fourier task currently only designed for displacement buffers");
		}
		m_buffer = buffer as DisplacementBufferCPU;
		m_fourier = fourier;
		m_index = index;
		m_numGrids = numGrids;
		WaveSpectrumBufferCPU.Buffer buffer2 = m_buffer.GetBuffer(m_index);
		m_data = buffer2.data;
		m_results = buffer2.results;
		m_map = buffer2.map;
		m_doublePacked = buffer2.doublePacked;
	}

	public void Reset(int index, int numGrids)
	{
		base.Reset();
		if (m_index == -1)
		{
			throw new InvalidOperationException("Index can be -1. Fourier for multiple buffers is not being used");
		}
		m_index = index;
		m_numGrids = numGrids;
		WaveSpectrumBufferCPU.Buffer buffer = m_buffer.GetBuffer(m_index);
		m_data = buffer.data;
		m_results = buffer.results;
		m_map = buffer.map;
		m_doublePacked = buffer.doublePacked;
	}

	public override void Start()
	{
		base.Start();
	}

	public override IEnumerator Run()
	{
		PerformSingleFourier();
		FinishedRunning();
		return null;
	}

	public override void End()
	{
		base.End();
		m_map.SetPixels(m_results);
		m_map.Apply();
	}

	private void PerformSingleFourier()
	{
		int num = -1;
		num = ((!m_doublePacked) ? m_fourier.PeformFFT_SinglePacked(0, m_data, this) : m_fourier.PeformFFT_DoublePacked(0, m_data, this));
		if (!Cancelled)
		{
			if (num != 1)
			{
				throw new InvalidOperationException("Fourier transform did not result in the read buffer at index " + 1);
			}
			ProcessData(m_index, m_results, m_data[num], m_numGrids);
		}
	}

	private void ProcessData(int index, Color[] result, Vector4[] data, int numGrids)
	{
		int cHANNELS = QueryDisplacements.CHANNELS;
		int size = m_buffer.Size;
		InterpolatedArray2f[] writeDisplacements = m_buffer.GetWriteDisplacements();
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				int num = j + i * size;
				int num2 = num * cHANNELS;
				switch (numGrids)
				{
				case 1:
					result[num].r = data[num].x;
					result[num].g = data[num].y;
					result[num].b = 0f;
					result[num].a = 0f;
					switch (index)
					{
					case 0:
						writeDisplacements[0].Data[num2 + 1] = result[num].r;
						break;
					case 1:
						writeDisplacements[0].Data[num2] += result[num].r;
						writeDisplacements[0].Data[num2 + 2] += result[num].g;
						break;
					}
					break;
				case 2:
					result[num].r = data[num].x;
					result[num].g = data[num].y;
					result[num].b = data[num].z;
					result[num].a = data[num].w;
					switch (index)
					{
					case 0:
						writeDisplacements[0].Data[num2 + 1] = result[num].r;
						writeDisplacements[1].Data[num2 + 1] = result[num].g;
						break;
					case 1:
						writeDisplacements[0].Data[num2] += result[num].r;
						writeDisplacements[0].Data[num2 + 2] += result[num].g;
						writeDisplacements[1].Data[num2] += result[num].b;
						writeDisplacements[1].Data[num2 + 2] += result[num].a;
						break;
					}
					break;
				case 3:
					result[num].r = data[num].x;
					result[num].g = data[num].y;
					result[num].b = data[num].z;
					result[num].a = data[num].w;
					switch (index)
					{
					case 0:
						writeDisplacements[0].Data[num2 + 1] = result[num].r;
						writeDisplacements[1].Data[num2 + 1] = result[num].g;
						writeDisplacements[2].Data[num2 + 1] = result[num].b;
						writeDisplacements[3].Data[num2 + 1] = result[num].a;
						break;
					case 1:
						writeDisplacements[0].Data[num2] += result[num].r;
						writeDisplacements[0].Data[num2 + 2] += result[num].g;
						writeDisplacements[1].Data[num2] += result[num].b;
						writeDisplacements[1].Data[num2 + 2] += result[num].a;
						break;
					case 2:
						writeDisplacements[2].Data[num2] += result[num].r;
						writeDisplacements[2].Data[num2 + 2] += result[num].g;
						break;
					}
					break;
				case 4:
					result[num].r = data[num].x;
					result[num].g = data[num].y;
					result[num].b = data[num].z;
					result[num].a = data[num].w;
					switch (index)
					{
					case 0:
						writeDisplacements[0].Data[num2 + 1] = result[num].r;
						writeDisplacements[1].Data[num2 + 1] = result[num].g;
						writeDisplacements[2].Data[num2 + 1] = result[num].b;
						writeDisplacements[3].Data[num2 + 1] = result[num].a;
						break;
					case 1:
						writeDisplacements[0].Data[num2] += result[num].r;
						writeDisplacements[0].Data[num2 + 2] += result[num].g;
						writeDisplacements[1].Data[num2] += result[num].b;
						writeDisplacements[1].Data[num2 + 2] += result[num].a;
						break;
					case 2:
						writeDisplacements[2].Data[num2] += result[num].r;
						writeDisplacements[2].Data[num2 + 2] += result[num].g;
						writeDisplacements[3].Data[num2] += result[num].b;
						writeDisplacements[3].Data[num2 + 2] += result[num].a;
						break;
					}
					break;
				default:
					result[num].r = 0f;
					result[num].g = 0f;
					result[num].b = 0f;
					result[num].a = 0f;
					break;
				}
			}
		}
	}
}
