using System;

namespace Beam.Utilities.Reflection;

public static class ExtensionMethods
{
	public static bool IsJNative(this Type type)
	{
		if (!type.IsPrimitive && !type.IsEnum && !(type == typeof(string)))
		{
			return type == typeof(decimal);
		}
		return true;
	}

	public static bool IsNullable(this Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		return false;
	}

	public static object GetDefaultValue(this Type type)
	{
		if (!type.IsValueType)
		{
			return null;
		}
		return Activator.CreateInstance(type);
	}
}
