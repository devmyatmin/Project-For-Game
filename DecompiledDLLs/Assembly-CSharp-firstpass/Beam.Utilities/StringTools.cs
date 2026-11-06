using System;
using System.Globalization;

namespace Beam.Utilities;

public static class StringTools
{
	public static int MAX_LENGTH = 500;

	public static void SetTruncateLength(int length)
	{
		MAX_LENGTH = length;
	}

	public static string Truncate(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return string.Empty;
		}
		if (input.Length > MAX_LENGTH)
		{
			return input.Substring(0, MAX_LENGTH) + "\n...";
		}
		return input;
	}

	public static string ToTitleCase(this string s)
	{
		return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());
	}

	public static string ReplaceFirst(this string str, string oldValue, string newValue)
	{
		if (string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		if (string.IsNullOrEmpty(oldValue))
		{
			return str;
		}
		if (string.IsNullOrEmpty(newValue))
		{
			newValue = string.Empty;
		}
		int startIndex = str.IndexOf(oldValue, StringComparison.InvariantCulture);
		return str.Remove(startIndex, oldValue.Length).Insert(startIndex, newValue);
	}
}
