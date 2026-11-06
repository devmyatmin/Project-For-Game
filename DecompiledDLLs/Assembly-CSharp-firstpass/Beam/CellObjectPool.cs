using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beam;

[Serializable]
public class CellObjectPool<TCellObject> : ICreateCellObjects<TCellObject>, IFreeCellObjects<TCellObject> where TCellObject : MonoBehaviour, ICellObject
{
	[SerializeField]
	private Transform _pooledContainer;

	[SerializeField]
	[HideInInspector]
	private Dictionary<int, Stack<TCellObject>> _pools = new Dictionary<int, Stack<TCellObject>>();

	TCellObject ICreateCellObjects<TCellObject>.CreateCellObject(TCellObject prefab)
	{
		return PopCellObject(prefab);
	}

	void IFreeCellObjects<TCellObject>.FreeCellObject(TCellObject obj)
	{
		PoolCellObject(obj);
	}

	private TCellObject PopCellObject(TCellObject prefab)
	{
		_pools.TryGetValue(prefab.PrefabId, out var value);
		TCellObject val;
		if (value != null && value.Count > 0)
		{
			val = value.Pop();
			((Component)(object)val).transform.SetParent((Transform)null);
		}
		else
		{
			val = Object.Instantiate<TCellObject>(prefab);
		}
		return val;
	}

	private void PoolCellObject(TCellObject obj)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (!_pools.TryGetValue(obj.PrefabId, out var value))
		{
			value = new Stack<TCellObject>();
			_pools.Add(obj.PrefabId, value);
		}
		((Component)(object)obj).transform.SetParent(_pooledContainer, false);
		((Component)(object)obj).transform.localPosition = Vector3.zero;
		((Component)(object)obj).transform.localRotation = Quaternion.identity;
		value.Push(obj);
	}
}
