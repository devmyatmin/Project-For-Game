using System.Collections.Generic;
using Ceto;

namespace Beam;

public interface IQueryWaves
{
	bool Active { get; }

	IEnumerable<WaveQuery> WaveQueries { get; }

	void InRange();

	void OutOfRange();
}
