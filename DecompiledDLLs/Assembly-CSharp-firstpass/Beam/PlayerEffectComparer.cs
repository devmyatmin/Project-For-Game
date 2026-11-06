using System.Collections.Generic;

namespace Beam;

public class PlayerEffectComparer : IEqualityComparer<PlayerEffect>
{
	public bool Equals(PlayerEffect a, PlayerEffect b)
	{
		return a?.Name == b?.Name;
	}

	public int GetHashCode(PlayerEffect obj)
	{
		return obj.Name.GetHashCode();
	}
}
