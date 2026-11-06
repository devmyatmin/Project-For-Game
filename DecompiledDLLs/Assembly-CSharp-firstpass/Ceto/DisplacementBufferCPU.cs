using System.Collections.Generic;
using Ceto.Common.Containers.Interpolation;
using Ceto.Common.Threading.Scheduling;
using UnityEngine;

namespace Ceto;

public class DisplacementBufferCPU : WaveSpectrumBufferCPU, IDisplacementBuffer
{
	private const int NUM_BUFFERS = 3;

	private IList<InterpolatedArray2f[]> m_displacements;

	public DisplacementBufferCPU(int size, Scheduler scheduler)
		: base(size, 3, scheduler)
	{
		int gRIDS = QueryDisplacements.GRIDS;
		int cHANNELS = QueryDisplacements.CHANNELS;
		m_displacements = new List<InterpolatedArray2f[]>(2);
		m_displacements.Add(new InterpolatedArray2f[gRIDS]);
		m_displacements.Add(new InterpolatedArray2f[gRIDS]);
		for (int i = 0; i < gRIDS; i++)
		{
			m_displacements[0][i] = new InterpolatedArray2f(size, size, cHANNELS, wrap: true);
			m_displacements[1][i] = new InterpolatedArray2f(size, size, cHANNELS, wrap: true);
		}
	}

	protected override void Initilize(WaveSpectrumCondition condition, float time)
	{
		InterpolatedArray2f[] writeDisplacements = GetWriteDisplacements();
		writeDisplacements[0].Clear();
		writeDisplacements[1].Clear();
		writeDisplacements[2].Clear();
		writeDisplacements[3].Clear();
		if (m_initTask == null)
		{
			m_initTask = condition.GetInitSpectrumDisplacementsTask(this, time);
		}
		else if (m_initTask.SpectrumType != condition.Key.SpectrumType || m_initTask.NumGrids != condition.Key.NumGrids)
		{
			m_initTask = condition.GetInitSpectrumDisplacementsTask(this, time);
		}
		else
		{
			m_initTask.Reset(condition, time);
		}
	}

	public InterpolatedArray2f[] GetWriteDisplacements()
	{
		return m_displacements[0];
	}

	public InterpolatedArray2f[] GetReadDisplacements()
	{
		return m_displacements[1];
	}

	public override void Run(WaveSpectrumCondition condition, float time)
	{
		SwapDisplacements();
		base.Run(condition, time);
	}

	public void CopyAndCreateDisplacements(out IList<InterpolatedArray2f> displacements)
	{
		QueryDisplacements.CopyAndCreateDisplacements(GetReadDisplacements(), out displacements);
	}

	public void CopyDisplacements(IList<InterpolatedArray2f> displacements)
	{
		QueryDisplacements.CopyDisplacements(GetReadDisplacements(), displacements);
	}

	private void SwapDisplacements()
	{
		InterpolatedArray2f[] value = m_displacements[0];
		m_displacements[0] = m_displacements[1];
		m_displacements[1] = value;
	}

	public Vector4 MaxRange(Vector4 choppyness, Vector2 gridScale)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return QueryDisplacements.MaxRange(GetReadDisplacements(), choppyness, gridScale, null);
	}

	public void QueryWaves(WaveQuery query, QueryGridScaling scaling)
	{
		int num = EnabledBuffers();
		if (num != 0)
		{
			InterpolatedArray2f[] readDisplacements = GetReadDisplacements();
			QueryDisplacements.QueryWaves(query, num, readDisplacements, scaling);
		}
	}
}
