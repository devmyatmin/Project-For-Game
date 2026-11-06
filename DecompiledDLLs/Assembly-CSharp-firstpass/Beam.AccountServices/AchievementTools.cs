using UnityEngine;

namespace Beam.AccountServices;

public static class AchievementTools
{
	public static bool IsCheating(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = StrandedWorld.GetZone(position);
		if ((Object)(object)zone == (Object)null)
		{
			return false;
		}
		return zone.IsUserMap;
	}
}
