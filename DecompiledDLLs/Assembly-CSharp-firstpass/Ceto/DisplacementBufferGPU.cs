using System.Collections.Generic;
using Ceto.Common.Containers.Interpolation;
using UnityEngine;

namespace Ceto;

public class DisplacementBufferGPU : WaveSpectrumBufferGPU, IDisplacementBuffer
{
	private static readonly int NUM_BUFFERS = 3;

	private InterpolatedArray2f[] m_displacements;

	public DisplacementBufferGPU(int size, Shader fourierSdr)
		: base(size, fourierSdr, NUM_BUFFERS)
	{
		int gRIDS = QueryDisplacements.GRIDS;
		int cHANNELS = QueryDisplacements.CHANNELS;
		m_displacements = new InterpolatedArray2f[gRIDS];
		for (int i = 0; i < gRIDS; i++)
		{
			m_displacements[i] = new InterpolatedArray2f(size, size, cHANNELS, wrap: true);
		}
	}

	public InterpolatedArray2f[] GetReadDisplacements()
	{
		return m_displacements;
	}

	public void CopyAndCreateDisplacements(out IList<InterpolatedArray2f> displacements)
	{
		QueryDisplacements.CopyAndCreateDisplacements(m_displacements, out displacements);
	}

	public void CopyDisplacements(IList<InterpolatedArray2f> displacements)
	{
		QueryDisplacements.CopyDisplacements(m_displacements, displacements);
	}

	public Vector4 MaxRange(Vector4 choppyness, Vector2 gridScale)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return QueryDisplacements.MaxRange(m_displacements, choppyness, gridScale, null);
	}

	public void QueryWaves(WaveQuery query, QueryGridScaling scaling)
	{
		int num = EnabledBuffers();
		if (num != 0)
		{
			QueryDisplacements.QueryWaves(query, num, m_displacements, scaling);
		}
	}
}
