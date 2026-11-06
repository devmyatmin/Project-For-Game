using UnityEngine;

namespace Beam;

public class NullCheckBounds : NullObject<NullCheckBounds>, ICheckBounds
{
	public bool InBounds(Vector3 position)
	{
		Debug.LogError((object)"NullCheckBounds::InBounds:: Not implemented.");
		return false;
	}
}
