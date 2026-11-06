using System;

namespace Beam.Utilities;

public static class BeamMath
{
	public static float Frac(float value)
	{
		return (float)((double)value - Math.Truncate(value));
	}

	public static float Sqr(float value)
	{
		return value * value;
	}

	public static float Normalize(float value, float min, float max)
	{
		return (value - min) / (max - min);
	}

	public static int IntWrap(int value, int min, int max)
	{
		if (value < min)
		{
			return max;
		}
		if (value > max)
		{
			return min;
		}
		return value;
	}
}
