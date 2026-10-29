using System.Collections.Generic;
using Beam.Crafting;
using UnityEngine;

public class EventItemProxies : MonoBehaviour, ISerializationCallbackReceiver
{
	private Dictionary<CraftingType, GameObject> _proxies;

	[SerializeField]
	[HideInInspector]
	private List<CraftingType> _keys;

	[SerializeField]
	[HideInInspector]
	private List<GameObject> _values;

	private List<GameObject> _proxyGameObjects = new List<GameObject>();

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		_keys.Clear();
		_values.Clear();
		if (_proxies == null)
		{
			return;
		}
		foreach (KeyValuePair<CraftingType, GameObject> proxy in _proxies)
		{
			_keys.Add(proxy.Key);
			_values.Add(proxy.Value);
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		_proxies = new Dictionary<CraftingType, GameObject>();
		for (int i = 0; i < Mathf.Min(_keys.Count, _values.Count); i++)
		{
			_proxies.Add(_keys[i], _values[i]);
		}
	}

	private void Awake()
	{
		foreach (KeyValuePair<CraftingType, GameObject> proxy in _proxies)
		{
			_proxyGameObjects.Add(proxy.Value);
		}
	}

	public bool CheckProxiesActive()
	{
		for (int i = 0; i < _proxyGameObjects.Count; i++)
		{
			if (_proxyGameObjects[i] != null && _proxyGameObjects[i].activeSelf)
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetProxy(CraftingType type)
	{
		GameObject value = null;
		_proxies.TryGetValue(type, out value);
		return value;
	}
}
