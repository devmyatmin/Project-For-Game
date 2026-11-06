using System.Collections.Generic;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class FarmingManager : MonoBehaviour, IPlantFactory, IFruitFactory, ISerializationCallbackReceiver
{
	[SerializeField]
	private Transform _pooledPlantsParent;

	private Dictionary<PlantType, PlantData> _plantData;

	[SerializeField]
	[HideInInspector]
	private List<PlantType> _keys;

	[SerializeField]
	[HideInInspector]
	private List<PlantData> _values;

	private Dictionary<PlantType, Stack<Plant>> _plantStacks = new Dictionary<PlantType, Stack<Plant>>();

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		_keys.Clear();
		_values.Clear();
		if (_plantData == null)
		{
			return;
		}
		foreach (KeyValuePair<PlantType, PlantData> plantDatum in _plantData)
		{
			_keys.Add(plantDatum.Key);
			_values.Add(plantDatum.Value);
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		_plantData = new Dictionary<PlantType, PlantData>();
		for (int i = 0; i < Mathf.Min(_keys.Count, _values.Count); i++)
		{
			_plantData.Add(_keys[i], _values[i]);
		}
	}

	private void Awake()
	{
		InitializePlantStacks();
	}

	private void InitializePlantStacks()
	{
		foreach (KeyValuePair<PlantType, PlantData> plantDatum in _plantData)
		{
			PlantType key = plantDatum.Key;
			if (!_plantStacks.ContainsKey(key))
			{
				_plantStacks.Add(key, new Stack<Plant>());
			}
		}
	}

	public InteractiveObject_FOOD CreateFruit(PlantType type)
	{
		_plantData.TryGetValue(type, out var value);
		if (value != null)
		{
			return MultiplayerMng.Instantiate<InteractiveObject_FOOD>(value.HarvestedPrefab.PrefabId, MiniGuid.New());
		}
		return null;
	}

	public Plant CreatePlant(PlantType plantType)
	{
		return PopPlant(plantType);
	}

	public void DestroyPlant(Plant plant)
	{
		PushPlant(plant);
	}

	private Plant PopPlant(PlantType plantType)
	{
		Plant plant;
		if (_plantStacks[plantType].Count > 0)
		{
			plant = _plantStacks[plantType].Pop();
		}
		else
		{
			plant = Object.Instantiate<Plant>(_plantData[plantType].PlantPrefab);
			plant.Initialize(this, plantType, _plantData[plantType].HarvestTime);
		}
		plant.Reset();
		return plant;
	}

	private void PushPlant(Plant plant)
	{
		((Component)plant).transform.parent = _pooledPlantsParent;
		_plantStacks[plant.PlantType].Push(plant);
	}
}
