using System;
using System.Globalization;
using Beam.Utilities.Reflection;

namespace Beam.Serialization.Json;

public static class ExtensionMethods
{
	public static CultureInfo EnglishCulture = new CultureInfo("en-US");

	public static void Add(this JObject jo, object instance)
	{
		jo.Add(JSerializer.Serialize(instance));
	}

	public static T GetValue<T>(this JObject jo)
	{
		if (jo == null || jo.Type == JObject.EToken.NULL)
		{
			throw new JSerializerException("JObject::GetValue<T>:: Error getting value. JObject is null or empty.");
		}
		return Convert<T>(jo);
	}

	private static T Convert<T>(JObject jo)
	{
		if (jo.Data is T)
		{
			return (T)jo.Data;
		}
		Type type = typeof(T);
		if (Beam.Utilities.Reflection.ExtensionMethods.IsNullable(type))
		{
			if (jo.Data == null)
			{
				return default;
			}
			type = Nullable.GetUnderlyingType(type);
		}
		try
		{
			if (Beam.ExtensionMethods.IsIntegralNumeric(type))
			{
				return (T)System.Convert.ChangeType(jo.Data.ToString(), type);
			}
			if (Beam.ExtensionMethods.IsFloatingPointNumeric(type))
			{
				return (T)System.Convert.ChangeType(jo.Data.ToString().Replace(",", "."), type, EnglishCulture);
			}
			return (T)System.Convert.ChangeType(jo.Data, type);
		}
		catch (Exception ex)
		{
			throw new JSerializerException("JObject::Convert<T>:: Error converting value of type [" + jo.Data.GetType().Name + "] to type [" + type.Name + "]. \n\n" + ex);
		}
	}
}
