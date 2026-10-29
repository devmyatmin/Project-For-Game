using System;
using Beam.Serialization.Json;
using UnityEngine;

namespace Beam;

[Serializable]
public class PlayerEffect : ISaveable
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private string _displayName;

	[SerializeField]
	private float _healthPerHour;

	[SerializeField]
	private float _fluidsPerHour;

	[SerializeField]
	private float _caloriesPerHour;

	private float _temperaturePerHour;

	[SerializeField]
	private float _duration = -1f;

	[SerializeField]
	private bool _positiveEffect;

	public string Name => _name;

	public string DisplayName
	{
		get
		{
			if (string.IsNullOrEmpty(_displayName))
			{
				_displayName = _name;
			}
			return _displayName;
		}
	}

	public float HealthPerHour => _healthPerHour;

	public float FluidsPerHour => _fluidsPerHour;

	public float CaloriesPerHour => _caloriesPerHour;

	public float TemperaturePerHour => _temperaturePerHour;

	public float Duration
	{
		get
		{
			return _duration;
		}
		set
		{
			_duration = value;
		}
	}

	public bool PositiveEffect
	{
		get
		{
			return _positiveEffect;
		}
		set
		{
			_positiveEffect = value;
		}
	}

	public DateTime StartedTime { get; set; }

	public PlayerEffect()
	{
	}

	public PlayerEffect(string name, string displayName, bool positiveEffect, float fluidsPerHour, float caloriesPerHour, float temperaturePerHour, float healthPerHour, float duration = -1f)
	{
		_name = name;
		_displayName = displayName;
		_positiveEffect = positiveEffect;
		_fluidsPerHour = fluidsPerHour;
		_caloriesPerHour = caloriesPerHour;
		_temperaturePerHour = temperaturePerHour;
		_healthPerHour = healthPerHour;
		_duration = duration;
	}

	public static bool operator ==(PlayerEffect a, PlayerEffect b)
	{
		return a.Name == b.Name;
	}

	public static bool operator !=(PlayerEffect a, PlayerEffect b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		return (obj as PlayerEffect)?.Name == Name;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("EffectName", JSerializer.Serialize(Name));
		jObject.AddField("DisplayName", JSerializer.Serialize(DisplayName));
		jObject.AddField("WaterEffectRate", JSerializer.Serialize(FluidsPerHour));
		jObject.AddField("CalorieEffectRate", JSerializer.Serialize(CaloriesPerHour));
		jObject.AddField("TemperatureEffectRate", JSerializer.Serialize(TemperaturePerHour));
		jObject.AddField("HealthEffectRate", JSerializer.Serialize(HealthPerHour));
		jObject.AddField("Duration", JSerializer.Serialize(Duration));
		jObject.AddField("StartedTime", JSerializer.Serialize(StartedTime));
		jObject.AddField("PositiveEffect", JSerializer.Serialize(PositiveEffect));
		return jObject;
	}

	public void Load(JObject data)
	{
		string value = data.GetField("EffectName").GetValue<string>();
		_name = value;
		JObject field = data.GetField("DisplayName");
		if (field != null)
		{
			_displayName = field.GetValue<string>();
		}
		float value2 = data.GetField("WaterEffectRate").GetValue<float>();
		_fluidsPerHour = value2;
		float value3 = data.GetField("CalorieEffectRate").GetValue<float>();
		_caloriesPerHour = value3;
		float value4 = data.GetField("TemperatureEffectRate").GetValue<float>();
		_temperaturePerHour = value4;
		float value5 = data.GetField("HealthEffectRate").GetValue<float>();
		_healthPerHour = value5;
		float value6 = data.GetField("Duration").GetValue<float>();
		_duration = value6;
		JObject field2 = data.GetField("StartedTime");
		StartedTime = JSerializer.Deserialize<DateTime>(field2);
		JObject field3 = data.GetField("PositiveEffect");
		if (field3 != null)
		{
			_positiveEffect = field3.GetValue<bool>();
		}
	}
}
