using System;

namespace Beam.Utilities;

public static class DateTimeTools
{
	public static DateTime RemoveHours(this DateTime time, float hours)
	{
		return time.AddHours(0f - hours);
	}
}
