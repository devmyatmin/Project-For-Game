using Beam.Serialization.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class InteractiveObject_FUELCAN : InteractiveObject, IHaveFuel
{
	[SerializeField]
	private float _fuel;

	[HideInInspector]
	public UnityEvent Refilled;

	[HideInInspector]
	public UnityEvent Filled;

	public float FuelCapacity => 4f;

	public float Fuel => _fuel;

	public override string GetHealthTerm()
	{
		return "ITEM_DISPLAY_HEALTH_FUEL_LABEL";
	}

	public override float GetHealthUnits()
	{
		if (!Mathf.Approximately(_fuel, 0f))
		{
			return Mathf.Clamp01(_fuel / FuelCapacity);
		}
		return 0f;
	}

	public override void OnDestroy()
	{
		((UnityEventBase)Refilled).RemoveAllListeners();
		((UnityEventBase)Filled).RemoveAllListeners();
		Refilled = null;
		Filled = null;
		base.OnDestroy();
	}

	public float Refill(float availableFuel)
	{
		float num = Mathf.Min(FuelCapacity - _fuel, availableFuel);
		if (num > 0f)
		{
			_fuel += num;
			_fuel = Mathf.Clamp(_fuel, 0f, FuelCapacity);
			UnityEvent refilled = Refilled;
			if (refilled != null)
			{
				refilled.Invoke();
			}
		}
		return num;
	}

	public float Fill(IHaveFuel fuelable)
	{
		float num = fuelable.Refill(_fuel);
		if (num > 0f)
		{
			_fuel -= num;
			_fuel = Mathf.Clamp(_fuel, 0f, FuelCapacity);
			UnityEvent filled = Filled;
			if (filled != null)
			{
				filled.Invoke();
			}
		}
		return num;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("Fuel", _fuel);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("Fuel");
		if (field != null)
		{
			_fuel = field.GetValue<float>();
		}
	}
}
