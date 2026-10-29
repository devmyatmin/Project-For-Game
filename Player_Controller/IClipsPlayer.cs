using System;
using System.Collections.Generic;

namespace Beam.UI;

public interface IClipsPlayer : IDisposable
{
	IEnumerator<float> Play_Sequence();

	void Skip();
}
