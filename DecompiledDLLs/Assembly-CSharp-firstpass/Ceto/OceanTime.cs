using Beam;
using Funlabs;
using Photon.Bolt;
using UnityEngine;

namespace Ceto;

public class OceanTime : IOceanTime
{
	public float Now
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (!MPExtensions.IsMultiplayer(Game.Mode))
			{
				return Time.time;
			}
			return BoltNetwork.ServerTime;
		}
	}
}
