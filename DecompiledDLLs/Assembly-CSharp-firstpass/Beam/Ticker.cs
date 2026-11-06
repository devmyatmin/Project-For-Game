using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public class Ticker : MonoBehaviour
{
	private static Ticker _Instance;

	private readonly List<ITick> _tickables = new List<ITick>();

	public static Ticker Instance
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_Instance == (Object)null && !Game.IsQuitting)
			{
				_Instance = new GameObject("~Ticker").AddComponent<Ticker>();
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

	public void Add(ITick tickable)
	{
		_tickables.Add(tickable);
	}

	public void Remove(ITick tickable)
	{
		_tickables.Remove(tickable);
	}

	private void Update()
	{
		if (LevelLoader.IsLoadingScene())
		{
			return;
		}
		for (int i = 0; i < _tickables.Count; i++)
		{
			ITick tick = _tickables[i];
			if (tick.Active)
			{
				try
				{
					tick.Tick();
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
