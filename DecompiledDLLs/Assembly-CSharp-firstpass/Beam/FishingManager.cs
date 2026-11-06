using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public class FishingManager : ISceneClassInstance
{
	private static FishingManager _Instance;

	private readonly List<FishBase> _fishes = new List<FishBase>();

	public static FishingManager Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = Game.CreateSceneClassInstance<FishingManager>();
			}
			return _Instance;
		}
	}

	void ISceneClassInstance.OnSceneDestroy()
	{
		_fishes.Clear();
		_Instance = null;
	}

	public void Add(FishBase fish)
	{
		_fishes.Add(fish);
	}

	public void Remove(FishBase fish)
	{
		_fishes.Remove(fish);
	}

	public IFishable GetInterestedFishableAt(Vector3 position)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _fishes.Count; i++)
		{
			FishBase fishBase = _fishes[i];
			position.y = ((Component)fishBase.FishRenderer).transform.position.y;
			if (fishBase.CheckPositionInInterestRadius(position))
			{
				return fishBase;
			}
		}
		return null;
	}
}
