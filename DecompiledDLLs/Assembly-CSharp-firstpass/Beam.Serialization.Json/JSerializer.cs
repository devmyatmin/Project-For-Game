using System;
using System.Collections.Generic;
using System.Reflection;
using Beam.Utilities.Reflection;
using UnityEngine;

namespace Beam.Serialization.Json;

public static class JSerializer
{
	private static Dictionary<Type, ISerializer> _serializers;

	private static Dictionary<Type, IComponentSerializer> _componentSerializers;

	static JSerializer()
	{
		_serializers = new Dictionary<Type, ISerializer>();
		_componentSerializers = new Dictionary<Type, IComponentSerializer>();
		GetSerializers();
	}

	private static void GetSerializers()
	{
		try
		{
			Type[] types = Assembly.GetCallingAssembly().GetTypes();
			foreach (Type type in types)
			{
				Attribute[] customAttributes = Attribute.GetCustomAttributes(type, typeof(CustomSerializerFor), inherit: false);
				foreach (Attribute attribute in customAttributes)
				{
					_serializers[((CustomSerializerFor)attribute).type] = Activator.CreateInstance(type) as ISerializer;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Error getting attribute from assembly.\n" + ex));
		}
		try
		{
			Type[] types = Assembly.GetCallingAssembly().GetTypes();
			foreach (Type type2 in types)
			{
				Attribute[] customAttributes = Attribute.GetCustomAttributes(type2, typeof(ComponentSerializerFor), inherit: false);
				foreach (Attribute attribute2 in customAttributes)
				{
					_componentSerializers[((ComponentSerializerFor)attribute2).type] = Activator.CreateInstance(type2) as IComponentSerializer;
				}
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)("Error getting attribute from assembly.\n" + ex2));
		}
	}

	public static JObject Serialize(object instance)
	{
		if (instance == null)
		{
			Debug.LogWarning((object)"Object was null.");
			return null;
		}
		Type type = instance.GetType();
		if (Beam.Utilities.Reflection.ExtensionMethods.IsJNative(type))
		{
			return new JObject(instance);
		}
		if (_serializers.ContainsKey(type))
		{
			return _serializers[type].Serialize(instance);
		}
		throw new JSerializerException("No serializer found for object type of '" + instance.GetType().Name + "'. Object will not be serialized.");
	}

	public static JObject Serialize(Component instance)
	{
		if (!Object.op_Implicit((Object)(object)instance))
		{
			Debug.LogWarning((object)"Component was null.");
			return null;
		}
		Type type = ((object)instance).GetType();
		if (_componentSerializers.ContainsKey(type))
		{
			return _componentSerializers[type].Serialize(instance);
		}
		throw new JSerializerException("No component serializer found for component type of '" + ((object)instance).GetType().Name + "'. Component will not be serialized.");
	}

	public static T Deserialize<T>(JObject data)
	{
		Type typeFromHandle = typeof(T);
		if (data == null || data.Type == JObject.EToken.NULL)
		{
			throw new JSerializerException("JSON Data is null or empty for type of '" + typeFromHandle.Name + "'. Data will not be deserialized.");
		}
		if (Beam.Utilities.Reflection.ExtensionMethods.IsJNative(typeFromHandle))
		{
			if (typeFromHandle.IsEnum)
			{
				return (T)Enum.Parse(typeFromHandle, data.GetValue<string>(), ignoreCase: true);
			}
			return data.GetValue<T>();
		}
		if (_serializers.ContainsKey(typeFromHandle))
		{
			return (T)_serializers[typeFromHandle].Deserialize(data);
		}
		throw new JSerializerException("No serializer found for object type of '" + typeFromHandle.Name + "' \nThe default value for type of '" + typeFromHandle.Name + "' has been returned.");
	}

	public static void DeserializeInto(JObject data, Component instance)
	{
		if (data == null || data.Type == JObject.EToken.NULL || data.Children == null)
		{
			throw new JSerializerException("JSON Data is null or empty for component type of '" + ((object)instance).GetType().Name + "'. Component will not be deserialized.");
		}
		Type type = ((object)instance).GetType();
		if (_componentSerializers.ContainsKey(type))
		{
			_componentSerializers[type].Deserialize(data, instance);
			return;
		}
		throw new JSerializerException("No component serializer found for component type of '" + ((object)instance).GetType().Name + "'. Component will not be deserialized.");
	}
}
