using System;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class PlantFruit
{
	private readonly IFruitFactory _fruitFactory;

	private readonly PlantType _plantType;

	private readonly float _growthTime;

	private readonly Transform _fruitPosition;

	private InteractiveObject_FOOD _fruit;

	private DateTime _lastHarvestedTime;

	public InteractiveObject_FOOD Fruit => _fruit;

	public DateTime LastHarvestedTime { get; set; }

	public PlantFruit(IFruitFactory fruitFactory, PlantType plantType, Transform fruitPosition, float growthTime)
	{
		_fruitFactory = fruitFactory;
		_plantType = plantType;
		_fruitPosition = fruitPosition;
		_growthTime = growthTime;
	}

	public void Poll()
	{
		CheckFruit();
	}

	public void CheckFruit()
	{
		if ((Object)(object)_fruit != (Object)null)
		{
			_fruit.IsGrowing = true;
		}
		else if (GameTime.GetHoursSince(LastHarvestedTime) > _growthTime)
		{
			CreateFruit();
		}
	}

	private void CreateFruit()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsClient(Game.Mode))
		{
			InteractiveObject_FOOD interactiveObject_FOOD = _fruitFactory.CreateFruit(_plantType);
			interactiveObject_FOOD.transform.position = _fruitPosition.position;
			interactiveObject_FOOD.transform.rotation = _fruitPosition.rotation;
			interactiveObject_FOOD.IsGrowing = true;
			AddFruit(interactiveObject_FOOD, _fruitPosition);
		}
	}

	public void AddFruit(InteractiveObject_FOOD fruit, Transform position)
	{
		_fruit = fruit;
		_fruit.Attach(new StaticAttachingAnchor(position));
		_fruit.DetachedEvent += Fruit_Detached;
		fruit.IsGrowing = true;
	}

	public void ClearFruit(bool spoil = false)
	{
		if (!((Object)(object)_fruit == (Object)null))
		{
			_fruit.IsGrowing = false;
			_fruit.DetachedEvent -= Fruit_Detached;
			_fruit.DetachFromImpact();
			if (spoil)
			{
				_fruit.Spoil();
			}
			if (_fruit.IsDamageable)
			{
				BeamTeam.Destroy<GameObject>(((Component)_fruit).gameObject);
			}
			_fruit = null;
		}
	}

	private void Fruit_Detached(IAttachable sender)
	{
		if ((Object)(object)(sender as InteractiveObject_FOOD) != (Object)null)
		{
			LastHarvestedTime = GameTime.Now;
			_fruit.IsGrowing = false;
			_fruit = null;
		}
	}

	public void UpdateFruitGrowing(bool isGrowing)
	{
		_fruit.IsGrowing = isGrowing;
	}
}
