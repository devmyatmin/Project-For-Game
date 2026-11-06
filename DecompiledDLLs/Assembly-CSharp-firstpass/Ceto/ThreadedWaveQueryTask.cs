using System;
using System.Collections;
using System.Collections.Generic;

namespace Ceto;

public class ThreadedWaveQueryTask : WaveQueryTask
{
	public override bool SupportsOverlays => false;

	public ThreadedWaveQueryTask(IEnumerable<WaveQuery> querys, Action<IEnumerable<WaveQuery>> callBack)
		: base(querys, callBack, isThreaded: true)
	{
	}

	public override IEnumerator Run()
	{
		IEnumerator<WaveQuery> enumerator = Querys.GetEnumerator();
		while (enumerator.MoveNext() && !Cancelled)
		{
			WaveQuery current = enumerator.Current;
			current.result.Clear();
			if (Displacements != null)
			{
				QueryDisplacements.QueryWaves(current, EnabledBuffers, Displacements, Scaling);
			}
			current.result.height += OceanLevel;
		}
		FinishedRunning();
		return null;
	}
}
