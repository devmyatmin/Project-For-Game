using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Funlabs;
using UnityEngine;

namespace Beam;

public class StateMachine<T> : MultiplayerBehaviour, IFixedTick where T : MonoBehaviour
{
	private Dictionary<object, Dictionary<string, Delegate>> _methodCache = new Dictionary<object, Dictionary<string, Delegate>>();

	private object _currentState;

	private object _previousState;

	private Action DoUpdate = DoNothing;

	private Action DoLateUpdate = DoNothing;

	private Action DoFixedUpdate = DoNothing;

	private Action<Collider> DoOnTriggerEnter = DoNothingCollider;

	private Action<Collider> DoOnTriggerExit = DoNothingCollider;

	private Action<Collision> DoOnCollisionEnter = DoNothingCollision;

	private Action<Collision> DoOnCollisionExit = DoNothingCollision;

	private Func<IEnumerator> EnterState = DoNothingCoroutine;

	private Func<IEnumerator> ExitState = DoNothingCoroutine;

	private IEnumerator EnterStateEnumerator;

	private IEnumerator ExitStateEnumerator;

	public object CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			if (_currentState == null || !_currentState.Equals(value))
			{
				_previousState = _currentState;
				_currentState = value;
				ApplyStateChange();
			}
		}
	}

	public FixedTickRate Rate => FixedTickRate.Quarter;

	bool IFixedTick.Active => ((Behaviour)this).isActiveAndEnabled;

	private static void DoNothing()
	{
	}

	private static void DoNothingCollider(Collider collider)
	{
	}

	private static void DoNothingCollision(Collision collider)
	{
	}

	private static IEnumerator DoNothingCoroutine()
	{
		yield break;
	}

	private void Update()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (LevelLoader.IsLoading(((Component)this).gameObject) || MPExtensions.IsClient(Game.Mode))
		{
			if (MPExtensions.IsClient(Game.Mode))
			{
				ReplicantUpdate();
			}
		}
		else
		{
			DoUpdate();
		}
	}

	public virtual void FixedTick()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (LevelLoader.IsLoading(((Component)this).gameObject) || MPExtensions.IsClient(Game.Mode))
		{
			if (MPExtensions.IsClient(Game.Mode))
			{
				ReplicantFixedUpdate();
			}
		}
		else
		{
			DoFixedUpdate();
		}
	}

	protected virtual void OnEnable()
	{
		FixedTicker.Instance?.Add(this);
	}

	protected virtual void OnDisable()
	{
		FixedTicker.Instance?.Remove(this);
	}

	private void GetCurrentStateMethods()
	{
		DoUpdate = GetDelegate<Action>("Update", DoNothing);
		DoLateUpdate = GetDelegate<Action>("LateUpdate", DoNothing);
		DoFixedUpdate = GetDelegate<Action>("FixedUpdate", DoNothing);
		DoOnTriggerEnter = GetDelegate<Action<Collider>>("OnTriggerEnter", DoNothingCollider);
		DoOnTriggerExit = GetDelegate<Action<Collider>>("OnTriggerExit", DoNothingCollider);
		DoOnCollisionEnter = GetDelegate<Action<Collision>>("OnCollisionEnter", DoNothingCollision);
		DoOnCollisionExit = GetDelegate<Action<Collision>>("OnCollisionExit", DoNothingCollision);
		EnterState = GetDelegate<Func<IEnumerator>>("EnterState", DoNothingCoroutine);
		ExitState = GetDelegate<Func<IEnumerator>>("ExitState", DoNothingCoroutine);
	}

	private TDelegate GetDelegate<TDelegate>(string mth, TDelegate def) where TDelegate : class
	{
		if (!_methodCache.TryGetValue(_currentState, out var value))
		{
			_methodCache[_currentState] = new Dictionary<string, Delegate>();
			value = new Dictionary<string, Delegate>();
		}
		if (!value.TryGetValue(mth, out var value2))
		{
			MethodInfo method = ((object)this).GetType().GetMethod(_currentState.ToString() + "_" + mth, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (method != null)
			{
				if (typeof(TDelegate) == typeof(Func<IEnumerator>) && method.ReturnType != typeof(IEnumerator))
				{
					Action action = Delegate.CreateDelegate(typeof(Action), this, method) as Action;
					value2 = (Func<IEnumerator>)(() =>
					{
						action();
						return (IEnumerator)null;
					});
				}
				else
				{
					value2 = Delegate.CreateDelegate(typeof(TDelegate), this, method);
				}
			}
			else
			{
				value2 = def as Delegate;
			}
			value[mth] = value2;
		}
		return value2 as TDelegate;
	}

	private void ApplyStateChange()
	{
		if (ExitState != null && ((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(ExitState());
		}
		GetCurrentStateMethods();
		if (EnterState != null && ((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(EnterState());
		}
	}

	protected virtual void PlayAnimation(byte animationId)
	{
	}

	protected virtual void ReplicantUpdate()
	{
	}

	protected virtual void ReplicantFixedUpdate()
	{
	}
}
