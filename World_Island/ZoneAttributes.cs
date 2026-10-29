using System;
using System.Collections.Generic;
using Beam;
using Beam.Serialization.Json;

public class ZoneAttributes : ISaveable
{
	private int _rain;

	private int _campFires;

	private readonly Dictionary<PlantType, int> _crops;

	public int Rain => _rain;

	public int CampFires
	{
		get
		{
			return _campFires;
		}
		set
		{
			_campFires = value;
		}
	}

	public IDictionary<PlantType, int> Crops => _crops;

	public ZoneAttributes()
	{
		_crops = new Dictionary<PlantType, int>
		{
			{
				PlantType.Ajuga,
				0
			},
			{
				PlantType.Aloe,
				0
			},
			{
				PlantType.Kura,
				0
			},
			{
				PlantType.Pipi,
				0
			},
			{
				PlantType.Potato,
				0
			},
			{
				PlantType.Quwawa,
				0
			},
			{
				PlantType.Yucca,
				0
			}
		};
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		_rain = Singleton<AtmosphereStorm>.Instance.Rain;
		jObject.AddField("Rain", JSerializer.Serialize(_rain));
		jObject.AddField("CampFires", JSerializer.Serialize(_campFires));
		JObject jObject2 = new JObject();
		foreach (KeyValuePair<PlantType, int> crop in _crops)
		{
			JObject jObject3 = new JObject();
			jObject3.AddField("K", JSerializer.Serialize(crop.Key.ToString()));
			jObject3.AddField("V", JSerializer.Serialize(crop.Value.ToString()));
			jObject2.Add(jObject3);
		}
		jObject.AddField("Crops", jObject2);
		return jObject;
	}

	public void Load(JObject data)
	{
		_rain = data.GetField("Rain").GetValue<int>();
		_campFires = data.GetField("CampFires").GetValue<int>();
		JObject field = data.GetField("Crops");
		for (int i = 0; i < field.Children.Count; i++)
		{
			JObject jObject = field.Children[i];
			JObject field2 = jObject.GetField("K");
			JObject field3 = jObject.GetField("V");
			if (Enum.TryParse<PlantType>(field2.GetValue<string>(), ignoreCase: true, out var result) && _crops.ContainsKey(result))
			{
				_crops[result] = field3.GetValue<int>();
			}
		}
	}
}
