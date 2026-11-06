using System;
using UnityEngine;

namespace Beam.Crafting;

public class PlantModel
{
	private readonly GameObject[] _plantStages;

	private readonly GameObject _deadStage;

	private readonly float _growthTime;

	private DateTime _startedGrowingTime;

	private bool _dead;

	private int _stage;

	public DateTime StartedGrowingTime
	{
		get
		{
			return _startedGrowingTime;
		}
		set
		{
			_startedGrowingTime = value;
		}
	}

	public bool Dead
	{
		get
		{
			return _dead;
		}
		set
		{
			_dead = value;
		}
	}

	public PlantModel(GameObject[] plantStages, GameObject deadStage, float growthTime)
	{
		_plantStages = plantStages;
		_deadStage = deadStage;
		_growthTime = growthTime;
	}

	public void Poll()
	{
		UpdateStage();
	}

	public void UpdateStage()
	{
		if (_dead && !_deadStage.activeSelf)
		{
			for (int i = 0; i < _plantStages.Length; i++)
			{
				_plantStages[i].SetActive(false);
			}
			_deadStage.SetActive(true);
		}
		else if (!_dead && _deadStage.activeSelf)
		{
			_deadStage.SetActive(false);
		}
		if (!_dead)
		{
			float hoursSince = GameTime.GetHoursSince(_startedGrowingTime);
			_stage = Mathf.Clamp((int)(hoursSince / _growthTime * 4f), 0, 4);
			for (int j = 0; j < _plantStages.Length; j++)
			{
				_plantStages[j].SetActive(_stage == j);
			}
		}
	}
}
