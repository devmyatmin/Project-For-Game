using System;
using System.Collections;
using System.Collections.Generic;

namespace Ceto;

public class CoroutineWaveQueryTask : WaveQueryTask
{
	public override bool SupportsOverlays => true;

	public int QuerysPerIteration { get; set; }

	public CoroutineWaveQueryTask(IEnumerable<WaveQuery> querys, Action<IEnumerable<WaveQuery>> callBack, int querysPerIteration = 16)
		: base(querys, callBack, isThreaded: false)
	{
		QuerysPerIteration = querysPerIteration;
	}

	public override IEnumerator Run()
	{
		int count = 0;
		int querysPerIteration = Math.Max(1, QuerysPerIteration);
		IEnumerator<WaveQuery> e = Querys.GetEnumerator();
		while (e.MoveNext())
		{
			if (Cancelled)
			{
				yield break;
			}
			WaveQuery current = e.Current;
			current.result.Clear();
			if (Displacements != null)
			{
				QueryDisplacements.QueryWaves(current, EnabledBuffers, Displacements, Scaling);
			}
			if (OverlaySampler != null)
			{
				OverlaySampler.QueryWaves(current);
			}
			current.result.height += OceanLevel;
			if (count % querysPerIteration == querysPerIteration - 1)
			{
				yield return null;
			}
			count++;
		}
		FinishedRunning();
	}
}
