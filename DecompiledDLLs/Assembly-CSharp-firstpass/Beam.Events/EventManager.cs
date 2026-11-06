using System;
using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Events;

public class EventManager : MonoBehaviour
{
	public delegate void EventDelegate<T>(T e) where T : IEvent;

	private delegate void EventDelegate(IEvent e);

	private Dictionary<Type, EventDelegate> delegates = new Dictionary<Type, EventDelegate>();

	private Dictionary<Delegate, EventDelegate> delegateLookup = new Dictionary<Delegate, EventDelegate>();

	public static EventManager Instance { get; private set; }

	public static void AddListener<T>(EventDelegate<T> del) where T : IEvent
	{
		Instance?.AddListener_(del);
	}

	public static void RemoveListener<T>(EventDelegate<T> del) where T : IEvent
	{
		Instance?.RemoveListener_(del);
	}

	public static void RaiseEvent<T>(T e) where T : IEvent
	{
		if (Object.op_Implicit((Object)(object)Instance))
		{
			Instance.RaiseEvent_(e);
		}
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	private void AddListener_<T>(EventDelegate<T> del) where T : IEvent
	{
		if (!delegateLookup.ContainsKey(del))
		{
			EventDelegate eventDelegate = (IEvent e) =>
			{
				del((T)e);
			};
			delegateLookup[del] = eventDelegate;
			if (delegates.TryGetValue(typeof(T), out var value))
			{
				value = (delegates[typeof(T)] = (EventDelegate)Delegate.Combine(value, eventDelegate));
			}
			else
			{
				delegates[typeof(T)] = eventDelegate;
			}
		}
	}

	public void RemoveListener_<T>(EventDelegate<T> del) where T : IEvent
	{
		if (!delegateLookup.TryGetValue(del, out var value))
		{
			return;
		}
		if (delegates.TryGetValue(typeof(T), out var value2))
		{
			value2 = (EventDelegate)Delegate.Remove(value2, value);
			if (value2 == null)
			{
				delegates.Remove(typeof(T));
			}
			else
			{
				delegates[typeof(T)] = value2;
			}
		}
		delegateLookup.Remove(del);
	}

	private void RaiseEvent_<T>(T e) where T : IEvent
	{
		if (delegates.TryGetValue(e.GetType(), out var value))
		{
			try
			{
				value(e);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Error raising event T\n" + ex.Message + "\n" + ex.StackTrace));
			}
		}
	}
}
