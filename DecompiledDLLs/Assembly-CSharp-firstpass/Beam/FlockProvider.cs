using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public abstract class FlockProvider : MonoBehaviour, IFlockProvider
{
	public abstract IList<FlockData> GetFlocks();
}
