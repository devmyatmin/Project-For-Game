using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beam.Utilities.Reflection;

public static class Reflection
{
	public static T GetAttribute<T>(this Enum enumerator)
	{
		object obj = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(enumerator.GetType().GetField(enumerator.ToString()).GetCustomAttributes(typeof(T), inherit: false));
		if (obj != null)
		{
			return (T)obj;
		}
		return default;
	}

	public static IEnumerable<T> GetAttributes<T>(this Type type)
	{
		return type.GetFields(BindingFlags.Static | BindingFlags.Public).SelectMany((FieldInfo field) => field.GetCustomAttributes(typeof(T), inherit: false)).Cast<T>();
	}

	public static int GetIndex(this Enum enumerator)
	{
		string value = enumerator.ToString();
		string[] names = Enum.GetNames(enumerator.GetType());
		for (int i = 0; i < names.Length; i++)
		{
			if (names[i].Equals(value, StringComparison.InvariantCultureIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}
}
