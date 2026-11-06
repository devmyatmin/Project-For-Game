using UnityEngine;

namespace Beam;

public interface ICheckBounds
{
	bool InBounds(Vector3 position);
}
