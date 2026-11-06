using System;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Plant : MonoBehaviour, ISaveable
{
	[SerializeField]
	private Transform _fruitPosition;

	[SerializeField]
	private GameObject[] _plantStages;

	[SerializeField]
	private GameObject _deadStage;

	private PlantModel _plantModel;

	private PlantFruit _plantFruit;

	private PlantType _plantType;

	private float _growthTime;

	public PlantType PlantType => _plantType;

	public float GrowthTime => _growthTime;

	public void Initialize(IFruitFactory fruitFactory, PlantType plantType, float growthTime)
	{
		_plantType = plantType;
		_growthTime = growthTime;
		_plantModel = new PlantModel(_plantStages, _deadStage, _growthTime);
		_plantFruit = new PlantFruit(fruitFactory, _plantType, _fruitPosition, _growthTime);
	}

	public void Reset()
	{
		_plantModel.StartedGrowingTime = GameTime.Now;
		_plantFruit.LastHarvestedTime = GameTime.Now;
		_plantModel.Dead = false;
		_plantModel.UpdateStage();
	}

	public void Poll()
	{
		_plantModel.Poll();
		if (!_plantModel.Dead)
		{
			_plantFruit.Poll();
		}
	}

	public void Kill()
	{
		_plantFruit.ClearFruit(spoil: true);
		_plantModel.Dead = true;
		_plantModel.UpdateStage();
	}

	public void ClearFruit()
	{
		_plantFruit.ClearFruit();
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("StartedGrowingTime", JSerializer.Serialize((object)_plantModel.StartedGrowingTime));
		jObject.AddField("LastHarvestedTime", JSerializer.Serialize((object)_plantFruit.LastHarvestedTime));
		JObject jObject2 = new JObject();
		if (!BeamTeam.IsNullOrDestroyed(_plantFruit.Fruit))
		{
			jObject2.AddField("Fruit", _plantFruit.Fruit.ReferenceId.ToString());
			jObject2.AddField("isGrowing", _plantFruit.Fruit.IsGrowing);
		}
		jObject.AddField("Fruits", jObject2);
		return jObject;
	}

	public void Load(JObject data)
	{
		_plantModel.StartedGrowingTime = JSerializer.Deserialize<DateTime>(data.GetField("StartedGrowingTime"));
		_plantFruit.LastHarvestedTime = JSerializer.Deserialize<DateTime>(data.GetField("LastHarvestedTime"));
		JObject fruitsData = data.GetField("Fruits");
		if (fruitsData != null && !fruitsData.IsNull())
		{
			LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
			{
				MiniGuid referenceId = Beam.Utilities.ExtensionMethods.ToMiniGuid(fruitsData.GetField("Fruit").GetValue<string>());
				bool value = fruitsData.GetField("isGrowing").GetValue<bool>();
				InteractiveObject_FOOD interactiveObject_FOOD = MultiplayerMng.FindObject<InteractiveObject_FOOD>(referenceId);
				_plantFruit.AddFruit(interactiveObject_FOOD, interactiveObject_FOOD.transform);
				_plantFruit.UpdateFruitGrowing(value);
			});
		}
		else
		{
			_plantFruit.CheckFruit();
		}
		_plantModel.Poll();
	}
}
