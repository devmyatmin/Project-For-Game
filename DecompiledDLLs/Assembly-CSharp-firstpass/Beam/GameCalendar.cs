using System;
using Beam.AccountServices;
using Beam.Serialization.Json;
using SharpNeatLib.Maths;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class GameCalendar : Singleton<GameCalendar>, IPersistentSaveable, ISaveable
{
	[SerializeField]
	private int _year;

	[SerializeField]
	private int _month;

	[SerializeField]
	private int _day;

	[SerializeField]
	private int _daysElapsed;

	[SerializeField]
	private DayTickEvent _newDayTick;

	public readonly MonthData[] MonthsData = new MonthData[12]
	{
		new MonthData
		{
			Name = "JAN",
			MinTemp = 65.6f,
			MaxTemp = 80.1f,
			RelativeHumidity = 55f,
			OceanTemperature = 76f,
			HeatWaveChance = 0,
			UVIndex = 5
		},
		new MonthData
		{
			Name = "FEB",
			MinTemp = 65.4f,
			MaxTemp = 80.5f,
			RelativeHumidity = 69.5f,
			OceanTemperature = 76f,
			HeatWaveChance = 0,
			UVIndex = 5
		},
		new MonthData
		{
			Name = "MAR",
			MinTemp = 67.2f,
			MaxTemp = 81.6f,
			RelativeHumidity = 66f,
			OceanTemperature = 76f,
			HeatWaveChance = 0,
			UVIndex = 5
		},
		new MonthData
		{
			Name = "APR",
			MinTemp = 68.7f,
			MaxTemp = 82.8f,
			RelativeHumidity = 63.5f,
			OceanTemperature = 76f,
			HeatWaveChance = 0,
			UVIndex = 6
		},
		new MonthData
		{
			Name = "MAY",
			MinTemp = 70.3f,
			MaxTemp = 84.7f,
			RelativeHumidity = 61f,
			OceanTemperature = 78f,
			HeatWaveChance = 0,
			UVIndex = 7
		},
		new MonthData
		{
			Name = "JUN",
			MinTemp = 72.2f,
			MaxTemp = 86.5f,
			RelativeHumidity = 59.5f,
			OceanTemperature = 79f,
			HeatWaveChance = 2,
			UVIndex = 7
		},
		new MonthData
		{
			Name = "JUL",
			MinTemp = 73.5f,
			MaxTemp = 87.5f,
			RelativeHumidity = 60f,
			OceanTemperature = 80f,
			HeatWaveChance = 6,
			UVIndex = 8
		},
		new MonthData
		{
			Name = "AUG",
			MinTemp = 74.2f,
			MaxTemp = 88.7f,
			RelativeHumidity = 59.5f,
			OceanTemperature = 80f,
			HeatWaveChance = 12,
			UVIndex = 8
		},
		new MonthData
		{
			Name = "SEP",
			MinTemp = 73.5f,
			MaxTemp = 88.5f,
			RelativeHumidity = 60.5f,
			OceanTemperature = 81f,
			HeatWaveChance = 11,
			UVIndex = 8
		},
		new MonthData
		{
			Name = "OCT",
			MinTemp = 72.3f,
			MaxTemp = 86.9f,
			RelativeHumidity = 61.5f,
			OceanTemperature = 81f,
			HeatWaveChance = 5,
			UVIndex = 7
		},
		new MonthData
		{
			Name = "NOV",
			MinTemp = 70.3f,
			MaxTemp = 84.1f,
			RelativeHumidity = 65f,
			OceanTemperature = 79f,
			HeatWaveChance = 0,
			UVIndex = 6
		},
		new MonthData
		{
			Name = "DEC",
			MinTemp = 67f,
			MaxTemp = 81.2f,
			RelativeHumidity = 69f,
			OceanTemperature = 77f,
			HeatWaveChance = 0,
			UVIndex = 5
		}
	};

	public int Year => _year;

	public int Month
	{
		get
		{
			return _month;
		}
		set
		{
			_month = value;
			if (_month > 11)
			{
				_month = 0;
				_year++;
			}
		}
	}

	public int Day
	{
		get
		{
			return _day;
		}
		set
		{
			_day = value;
			if (_day > DateTime.DaysInMonth(_year, Month + 1) - 1)
			{
				_day = 0;
				Month++;
			}
		}
	}

	public int DaysElapsed => _daysElapsed;

	public int DaysAlive => DaysElapsed + 1;

	public DayTickEvent NewDayTick => _newDayTick;

	public MonthData CurrentMonthData => MonthsData[Month];

	private void Awake()
	{
		_year = 2022;
		FastRandom fastRandom = new FastRandom(StrandedWorld.WORLD_SEED);
		Month = fastRandom.Next(0, 12);
		Day = fastRandom.Next(0, DateTime.DaysInMonth(_year, Month + 1));
		Singleton<GameTime>.Instance.DayTick += GameTime_DayTick;
		Singleton<GameTime>.Instance.NewDayTick += GameTime_NewDayTick;
	}

	protected override void OnDestroy()
	{
		if ((Object)(object)Singleton<GameTime>.Instance != (Object)null)
		{
			Singleton<GameTime>.Instance.DayTick -= GameTime_DayTick;
			Singleton<GameTime>.Instance.NewDayTick -= GameTime_NewDayTick;
		}
		base.OnDestroy();
	}

	private void GameTime_DayTick()
	{
		Day++;
		_daysElapsed++;
	}

	private void GameTime_NewDayTick()
	{
		((UnityEvent<int>)NewDayTick)?.Invoke(_daysElapsed);
		GAccountService.Get().LocalUser.UserStats.SessionStats.DaysSurvived = DaysElapsed;
	}

	public string GetDate()
	{
		return $"{(Day + 1).ToString().PadLeft(2, '0')}-{CurrentMonthData.Name}";
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Year", _year);
		jObject.AddField("Month", Month);
		jObject.AddField("Day", Day);
		jObject.AddField("DaysElapsed", _daysElapsed);
		JObject jObject2 = new JObject();
		for (int i = 0; i < MonthsData.Length; i++)
		{
			MonthData monthData = MonthsData[i];
			JObject jObject3 = new JObject();
			jObject3.AddField("HeatWaveDays", monthData.HeatWaveDays);
			jObject2.AddField(monthData.Name, jObject3);
		}
		jObject.AddField("MonthsData", jObject2);
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("Year");
		_year = field.GetValue<int>();
		int value = data.GetField("Month").GetValue<int>();
		Month = value;
		int value2 = data.GetField("Day").GetValue<int>();
		Day = value2;
		int value3 = data.GetField("DaysElapsed").GetValue<int>();
		_daysElapsed = value3;
		JObject field2 = data.GetField("MonthsData");
		if (field2 != null)
		{
			for (int i = 0; i < MonthsData.Length; i++)
			{
				MonthData monthData = MonthsData[i];
				int value4 = field2.GetField(monthData.Name).GetField("HeatWaveDays").GetValue<int>();
				monthData.HeatWaveDays = value4;
			}
		}
	}
}
