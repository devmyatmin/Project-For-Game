using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public abstract class NotifyOnEvent : MonoBehaviour
{
	private interface INotify
	{
	}

	private class Notify : INotify
	{
		public Action<GameObject> action;
	}

	private class NotifyWithArg : INotify
	{
		public Action<GameObject, object> action;

		public object arg;
	}

	public static bool Disable;

	private IList<INotify> m_actions = new List<INotify>();

	protected void OnEvent()
	{
		if (Disable)
		{
			return;
		}
		int count = m_actions.Count;
		for (int i = 0; i < count; i++)
		{
			INotify notify = m_actions[i];
			if (notify is Notify)
			{
				(notify as Notify).action(((Component)this).gameObject);
			}
			else if (notify is NotifyWithArg)
			{
				NotifyWithArg notifyWithArg = notify as NotifyWithArg;
				notifyWithArg.action(((Component)this).gameObject, notifyWithArg.arg);
			}
		}
	}

	public void AddAction(Action<GameObject, object> action, object arg)
	{
		NotifyWithArg notifyWithArg = new NotifyWithArg();
		notifyWithArg.action = action;
		notifyWithArg.arg = arg;
		m_actions.Add(notifyWithArg);
	}

	public void AddAction(Action<GameObject> action)
	{
		Notify notify = new Notify();
		notify.action = action;
		m_actions.Add(notify);
	}
}
