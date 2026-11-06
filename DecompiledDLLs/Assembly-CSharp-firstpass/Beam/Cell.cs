using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public class Cell<TCellObject> : ICell where TCellObject : class, ICellObject
{
	private readonly Transform _transform;

	private readonly Vector3 _size;

	private int _lod = -1;

	private List<TCellObject> _objects = new List<TCellObject>();

	public int Id
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Atan2(Position.z, Position.x) * 180f / (float)Math.PI;
			return (int)(Position.x + Position.z + num);
		}
	}

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _transform.position;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_transform.position = value;
		}
	}

	public Vector3 Size
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _size;
		}
	}

	public int Lod
	{
		get
		{
			return _lod;
		}
		set
		{
			if (_lod != value)
			{
				_lod = value;
				LodChanged?.Invoke(_lod);
			}
		}
	}

	public IList<TCellObject> Objects => _objects;

	public Transform ObjectContainer { get; set; }

	public bool Generated { get; set; }

	public bool Loaded { get; set; }

	public event Action<int> LodChanged;

	public event Action<TCellObject> CellObjectFreed;

	public event Action<TCellObject> CellObjectDestroyed;

	public Cell(Transform transform, Vector3 size)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		_transform = transform;
		_size = size;
	}

	public void ResetLod()
	{
		_lod = -1;
	}

	public void AddCellObject(TCellObject cellObject)
	{
		_objects.Add(cellObject);
		cellObject.Subscribe(this);
		cellObject.Freed += CellObject_Freed;
		cellObject.Destroyed += CellObject_Destroyed;
	}

	public void RemoveCellObject(TCellObject cellObject)
	{
		if (_objects.Remove(cellObject))
		{
			cellObject.Unsubscribe(this);
			cellObject.Freed -= CellObject_Freed;
			cellObject.Destroyed -= CellObject_Destroyed;
		}
	}

	private void CellObject_Freed(ICellObject obj)
	{
		TCellObject val = (TCellObject)obj;
		RemoveCellObject(val);
		CellObjectFreed?.Invoke(val);
	}

	private void CellObject_Destroyed(ICellObject obj)
	{
		TCellObject val = (TCellObject)obj;
		RemoveCellObject(val);
		CellObjectDestroyed?.Invoke(val);
	}
}
