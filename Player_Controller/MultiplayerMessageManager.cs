using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Funlabs;

public static class MultiplayerMessageManager
{
	private static Dictionary<Type, int> _messageIds = new Dictionary<Type, int>();

	private static Dictionary<int, ConstructorInfo> _messageConstructors = new Dictionary<int, ConstructorInfo>();

	public static int Id(Type type)
	{
		return _messageIds[type];
	}

	public static IMultiplayerMessage New(int id)
	{
		return (IMultiplayerMessage)_messageConstructors[id].Invoke(null);
	}

	public static void Register()
	{
		if (_messageIds.Count != 0)
		{
			return;
		}
		int num = 0;
		foreach (Type item in from x in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly x) => x.GetTypes())
			where typeof(IMultiplayerMessage).IsAssignableFrom(x) && !x.IsAbstract
			orderby x.ToStringSafe()
			select x)
		{
			Debug.Log($"<color=yellow>Register multiplayer message class ${item}</color>");
			_messageIds[item] = num;
			_messageConstructors[num] = item.GetConstructor(Type.EmptyTypes);
			num++;
		}
		if (num < MultiplayerTokens.MessageEnvelope.MAX_MESSAGES_TYPES)
		{
			return;
		}
		throw new Exception($"Too many replicating messages class. Only {MultiplayerTokens.MessageEnvelope.MAX_MESSAGES_TYPES} are supported");
	}
}
