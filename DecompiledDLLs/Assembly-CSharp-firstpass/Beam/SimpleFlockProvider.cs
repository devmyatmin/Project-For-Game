using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public class SimpleFlockProvider : FlockProvider
{
	[SerializeField]
	private List<FlockData> _flocks;

	public override IList<FlockData> GetFlocks()
	{
		return _flocks;
	}
}
