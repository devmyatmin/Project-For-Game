using System.Text.RegularExpressions;
using UnityEngine;

namespace MyUtility;

public static class UtilityStrings
{
	private const int MAX_LENGTH = 75;

	public static string CleanUpString(string str)
	{
		return CleanUpString(str, 75);
	}

	public static string CleanUpString(string str, int max_length)
	{
		string text = Regex.Replace(str, "[^\\w\\.@_*+$<>\\- ]", "");
		return text.Substring(0, Mathf.Min(max_length, text.Length));
	}

	public static string InsertSpacesIntoCamelCase(string p_camelCaseString)
	{
		return Regex.Replace(p_camelCaseString, "(?=[A-Z][a-z][0-9])|(?<=[a-z])(?=[A-Z])|(?<=[a-z])(?=[0-9])", " ");
	}
}
