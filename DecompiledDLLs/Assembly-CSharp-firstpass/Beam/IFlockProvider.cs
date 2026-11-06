using System.Collections.Generic;

namespace Beam;

public interface IFlockProvider
{
	IList<FlockData> GetFlocks();
}
