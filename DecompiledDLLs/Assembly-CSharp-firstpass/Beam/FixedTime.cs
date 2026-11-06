using UnityEngine;

namespace Beam;

public class FixedTime
{
	public static float TimeStep(IFixedTick tickable)
	{
		if (!Time.inFixedTimeStep)
		{
			return Time.deltaTime;
		}
		return Time.fixedDeltaTime * (float)tickable.Rate;
	}
}
