using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public class BoidFlock
{
	private readonly List<Boid> _boids = new List<Boid>();

	private readonly int _cellCount;

	private readonly float _cellWidth;

	private readonly List<Boid>[] _cells;

	private readonly List<Boid> _tempCellBoids = new List<Boid>();

	public IList<Boid> Boids => _boids;

	public BoidFlock(float boundsSize, int cells)
	{
		_cellCount = cells;
		_cellWidth = boundsSize / (float)_cellCount;
		_cells = new List<Boid>[_cellCount * _cellCount];
		for (int i = 0; i < _cellCount; i++)
		{
			for (int j = 0; j < _cellCount; j++)
			{
				_cells[j * _cellCount + i] = new List<Boid>();
			}
		}
	}

	private IList<Boid> FindCellContaining(float posX, float posY)
	{
		int num = Mathf.FloorToInt(posX / _cellWidth);
		int num2 = Mathf.FloorToInt(posY / _cellWidth);
		if (num < 0)
		{
			num = 0;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num >= _cellCount)
		{
			num = _cellCount - 1;
		}
		if (num2 >= _cellCount)
		{
			num2 = _cellCount - 1;
		}
		return _cells[num2 * _cellCount + num];
	}

	private IList<Boid> FindNeighbourCells(float posX, float posY)
	{
		return FindCellContaining(posX, posY);
	}

	public IList<Boid> FindNeighborsNear(float posX, float posY, float influenceRange)
	{
		if (influenceRange == 0f)
		{
			return FindCellContaining(posX, posY);
		}
		if (influenceRange < _cellWidth)
		{
			return FindNeighbourCells(posX, posY);
		}
		int index = Mathf.CeilToInt(influenceRange / _cellWidth);
		return FindExtended(posX, posY, index);
	}

	private IList<Boid> FindExtended(float posX, float posY, int index)
	{
		int num = Mathf.FloorToInt(posX / _cellWidth);
		int num2 = Mathf.FloorToInt(posY / _cellWidth);
		if (num < 0)
		{
			num = 0;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num >= _cellCount)
		{
			num = _cellCount - 1;
		}
		if (num2 >= _cellCount)
		{
			num2 = _cellCount - 1;
		}
		_tempCellBoids.Clear();
		for (int i = num - index; i < num + index; i++)
		{
			if (i < 0 || i >= _cellCount)
			{
				continue;
			}
			for (int j = num2 - index; j < num2 + index; j++)
			{
				if (j >= 0 && j < _cellCount)
				{
					List<Boid> collection = _cells[j * _cellCount + i];
					_tempCellBoids.AddRange(collection);
				}
			}
		}
		return _tempCellBoids;
	}

	public void Rebuild()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _cells.Length; i++)
		{
			_cells[i].Clear();
		}
		for (int j = 0; j < _boids.Count; j++)
		{
			Boid boid = _boids[j];
			FindCellContaining(boid.Position.x, boid.Position.y).Add(boid);
		}
	}
}
