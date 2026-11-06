using System;
using UnityEngine;

namespace Beam.Utilities;

public static class string_safemethods
{
	public static string Format(string str, params object[] args)
	{
		try
		{
			return string.Format(str, args);
		}
		catch (FormatException ex)
		{
			Debug.LogError((object)("Invalid string format '" + str + "'\n\n" + ex.Message));
			return str;
		}
	}
}
