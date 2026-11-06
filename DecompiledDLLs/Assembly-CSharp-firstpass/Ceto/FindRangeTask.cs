using System.Collections;
using System.Collections.Generic;
using Ceto.Common.Containers.Interpolation;
using Ceto.Common.Threading.Tasks;
using UnityEngine;

namespace Ceto;

public class FindRangeTask : ThreadedTask
{
	private IList<InterpolatedArray2f> m_displacements;

	private WaveSpectrum m_spectrum;

	private Vector4 m_max;

	private Vector4 m_choppyness;

	private Vector2 m_gridScale;

	public FindRangeTask(WaveSpectrum spectrum)
		: base(isThreaded: true)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		m_spectrum = spectrum;
		m_choppyness = spectrum.Choppyness;
		m_gridScale = new Vector2(spectrum.gridScale, spectrum.gridScale);
		spectrum.DisplacementBuffer.CopyAndCreateDisplacements(out m_displacements);
	}

	public override void Reset()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.Reset();
		m_choppyness = m_spectrum.Choppyness;
		m_gridScale = new Vector2(m_spectrum.gridScale, m_spectrum.gridScale);
		m_spectrum.DisplacementBuffer.CopyDisplacements(m_displacements);
	}

	public override IEnumerator Run()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		m_max = QueryDisplacements.MaxRange(m_displacements, m_choppyness, m_gridScale, this);
		FinishedRunning();
		return null;
	}

	public override void End()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		m_spectrum.MaxDisplacement = new Vector2(Mathf.Max(m_max.x, m_max.z), m_max.y);
		base.End();
	}
}
