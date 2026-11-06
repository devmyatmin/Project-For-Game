using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public class FixedTicker : MonoBehaviour
{
	private static FixedTicker _Instance;

	private readonly List<IFixedTick> _tickables = new List<IFixedTick>();

	private int _ticks;

	public static FixedTicker Instance
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_Instance == (Object)null && !Game.IsQuitting)
			{
				_Instance = new GameObject("~FixedTicker").AddComponent<FixedTicker>();
				((Component)_Instance).transform.SetAsFirstSibling();
				((Component)_Instance).transform.position = Vector3.zero;
				((Component)_Instance).transform.rotation = Quaternion.identity;
			}
			return _Instance;
		}
	}

	private void Awake()
	{
		if ((Object)(object)_Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnDestroy()
	{
		if (!((Object)(object)_Instance != (Object)(object)this))
		{
			_Instance = null;
		}
	}

	public void Add(IFixedTick tickable)
	{
		_tickables.Add(tickable);
	}

	public void Remove(IFixedTick tickable)
	{
		_tickables.Remove(tickable);
	}

	private void FixedUpdate()
	{
		if (LevelLoader.IsLoadingScene())
		{
			return;
		}
		_ticks++;
		if (_ticks > 3)
		{
			_ticks = 0;
		}
		bool flag = _ticks % 2 == 0;
		bool flag2 = _ticks % 4 == 0;
		for (int i = 0; i < _tickables.Count; i++)
		{
			IFixedTick fixedTick = _tickables[i];
			if ((fixedTick.Rate != FixedTickRate.Half || flag) && (fixedTick.Rate != FixedTickRate.Quarter || flag2) && fixedTick.Active)
			{
				try
				{
					fixedTick.FixedTick();
				}
				catch (Exception ex)
				{
					_tickables.RemoveAt(i);
					Debug.LogException(ex);
					break;
				}
			}
		}
	}
}
