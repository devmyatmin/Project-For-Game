using System.Collections.Generic;
using System.Linq;
using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class GameUI : MonoBehaviour
{
	private static GameUI _Instance;

	[SerializeField]
	private List<MonoBehaviour> _components;

	private void Awake()
	{
		if ((Object)(object)_Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
		else
		{
			_Instance = this;
		}
	}

	private void OnDestroy()
	{
		if (!((Object)(object)_Instance != (Object)(object)this))
		{
			_Instance = null;
		}
	}

	public static T GetUIComponent<T>() where T : Component
	{
		MonoBehaviour? obj = _Instance?._components.FirstOrDefault((MonoBehaviour thing) => ((object)thing).GetType() == typeof(T));
		return (T)(object)((obj is T) ? obj : null);
	}
}
