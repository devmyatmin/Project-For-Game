using System.Collections.Generic;
using Beam;
using Beam.Events;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class AtmosphereStorm : Singleton<AtmosphereStorm>, IPersistentSaveable, ISaveable
{
	private class ReplicateCheckWeatherMsg : MultiplayerMessage
	{
		[Replicate]
		public int Seed;

		[Replicate]
		public int Iter;

		public override void OnPeer()
		{
			Singleton<AtmosphereStorm>.Instance.SetState(Seed, Iter);
			Singleton<AtmosphereStorm>.Instance.CheckWeather();
		}
	}

	[Range(0f, 100f)]
	[SerializeField]
	private int _percentage = 70;

	private Atmosphere.Illuminary _sunIlluminary;

	private Atmosphere.Illuminary _moonIlluminary;

	[SerializeField]
	private WeatherEvent _currentWeatherEvent;

	[SerializeField]
	private List<WeatherEvent> _weatherEvents;

	private DeterministicFastRandom _random = new DeterministicFastRandom();

	private bool _canDoWeather = true;

	private int _lastRainyDay = -1;

	private int _lastDayChecked = -1;

	private int _rain;

	public int Percentage => _percentage;

	public WeatherEvent CurrentWeatherEvent => _currentWeatherEvent;

	public IList<WeatherEvent> WeatherEvents => _weatherEvents;

	public int Rain => _rain;

	public event WeatherEventHandler WeatherStarted;

	public event EmptyEventHandler WeatherStopped;

	protected virtual void OnWeatherStarted(WeatherEvent weather)
	{
		WeatherStarted?.Invoke(weather);
	}

	protected virtual void OnWeatherStopped()
	{
		WeatherStopped?.Invoke();
	}

	private void Start()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalFloat("_SheetLightning", 0f);
		Singleton<GameTime>.Instance.NewDayTick += GameTime_NewDayTick;
		Singleton<GameTime>.Instance.HourChanged += GameTime_HourChanged;
		for (int i = 0; i < _weatherEvents.Count; i++)
		{
			_weatherEvents[i].Started += WeatherEvent_Started;
			_weatherEvents[i].Stopped += WeatherEvent_Stopped;
		}
		EventManager.AddListener<EndGameSequenceEvent>(EventManager_EndGameSequenceEvent);
		if (MPExtensions.IsMaster(Game.Mode))
		{
			SetState(StrandedWorld.WORLD_SEED, 0);
		}
	}

	protected override void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)Singleton<GameTime>.Instance))
		{
			Singleton<GameTime>.Instance.NewDayTick -= GameTime_NewDayTick;
			Singleton<GameTime>.Instance.HourChanged -= GameTime_HourChanged;
		}
		for (int i = 0; i < _weatherEvents.Count; i++)
		{
			_weatherEvents[i].Started -= WeatherEvent_Started;
			_weatherEvents[i].Stopped -= WeatherEvent_Stopped;
		}
		EventManager.RemoveListener<EndGameSequenceEvent>(EventManager_EndGameSequenceEvent);
		WeatherStarted = null;
		WeatherStopped = null;
		base.OnDestroy();
	}

	private void SetState(int seed, int iter)
	{
		_random.RestoreState(seed, iter);
	}

	private void EventManager_EndGameSequenceEvent(EndGameSequenceEvent ev)
	{
		ClearWeatherEvent();
		_canDoWeather = false;
	}

	private void GameTime_NewDayTick()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsClient(Game.Mode))
		{
			ReplicatedCheckWeather();
		}
	}

	private void GameTime_HourChanged(float hours)
	{
		if (!((Object)(object)_currentWeatherEvent != (Object)null))
		{
			return;
		}
		if (_currentWeatherEvent.HasStarted)
		{
			if (GameTime.GetMinutesSince(_currentWeatherEvent.StartedTime) > _currentWeatherEvent.TotalEventMinutes)
			{
				_currentWeatherEvent.ResetEvent();
			}
		}
		else if (Singleton<GameTime>.Instance.MilitaryTime > _currentWeatherEvent.StartTime + _currentWeatherEvent.TotalEventHours)
		{
			ClearWeatherEvent();
		}
		else if (Singleton<GameTime>.Instance.MilitaryTime > _currentWeatherEvent.StartTime)
		{
			float hours2 = Singleton<GameTime>.Instance.MilitaryTime - _currentWeatherEvent.StartTime;
			_currentWeatherEvent.StartedTime = DateTimeTools.RemoveHours(GameTime.Now, hours2);
			_currentWeatherEvent.ResumeEvent();
			((MonoBehaviour)this).CancelInvoke("PollTimeForWeatherEvent");
		}
	}

	private void CheckWeather()
	{
		if (_lastDayChecked == Singleton<GameCalendar>.Instance.Day)
		{
			return;
		}
		if (_lastRainyDay != Singleton<GameCalendar>.Instance.Day)
		{
			if ((Object)(object)_currentWeatherEvent != (Object)null)
			{
				_lastRainyDay = Singleton<GameCalendar>.Instance.Day;
			}
			else if ((float)_random.NextDouble() * 100f <= (float)_percentage)
			{
				_lastRainyDay = Singleton<GameCalendar>.Instance.Day;
				CreateWeatherEvent();
			}
		}
		_lastDayChecked = Singleton<GameCalendar>.Instance.Day;
	}

	private void CreateWeatherEvent()
	{
		List<WeatherEvent> list = new List<WeatherEvent>();
		for (int i = 0; i < _weatherEvents.Count; i++)
		{
			WeatherEvent weatherEvent = _weatherEvents[i];
			for (int j = 0; j < weatherEvent.Percentage / 10; j++)
			{
				list.Add(weatherEvent);
			}
		}
		int index = _random.Next(0, list.Count);
		WeatherEvent weatherEvent2 = list[index];
		float startTime = 10f + 6f * (float)_random.NextDouble();
		weatherEvent2.StartTime = startTime;
		NewWeatherEvent(weatherEvent2);
	}

	private void PollTimeForWeatherEvent()
	{
		if (_canDoWeather && Singleton<GameTime>.Instance.MilitaryTime >= _currentWeatherEvent.StartTime)
		{
			((MonoBehaviour)this).CancelInvoke("PollTimeForWeatherEvent");
			StartWeatherEvent();
		}
	}

	public void StartWeatherEvent()
	{
		if ((Object)(object)_currentWeatherEvent != (Object)null)
		{
			_currentWeatherEvent.StartEvent();
			if (Mathf.Approximately(_currentWeatherEvent.Humidity, 100f))
			{
				_rain++;
			}
		}
	}

	private void WeatherEvent_Started(WeatherEvent weather)
	{
		OnWeatherStarted(weather);
	}

	private void WeatherEvent_Stopped()
	{
		OnWeatherStopped();
	}

	public void NewWeatherEvent(WeatherEvent weather)
	{
		SetWeatherEvent(weather);
		((MonoBehaviour)this).InvokeRepeating("PollTimeForWeatherEvent", 0f, 2f);
	}

	public void SetWeatherEvent(WeatherEvent weather)
	{
		_currentWeatherEvent = weather;
	}

	public void ClearWeatherEvent()
	{
		_currentWeatherEvent = null;
		((MonoBehaviour)this).CancelInvoke("PollTimeForWeatherEvent");
	}

	private void ReplicatedCheckWeather()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateCheckWeatherMsg replicateCheckWeatherMsg = new ReplicateCheckWeatherMsg();
			replicateCheckWeatherMsg.Seed = _random.GetSeed();
			replicateCheckWeatherMsg.Iter = _random.GetIter();
			replicateCheckWeatherMsg.Post();
		}
		CheckWeather();
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Rain", _rain);
		JObject data = SavingUtilities.Save(_random);
		jObject.AddField("State", data);
		if ((Object)(object)CurrentWeatherEvent != (Object)null)
		{
			JObject data2 = CurrentWeatherEvent.Save();
			jObject.AddField(((Object)((Component)CurrentWeatherEvent).gameObject).name, data2);
		}
		return jObject;
	}

	public void Load(JObject data)
	{
		_rain = data.GetField("Rain").GetValue<int>();
		JObject field = data.GetField("State");
		if ((bool)field)
		{
			SavingUtilities.Load(_random, field);
		}
		for (int i = 0; i < _weatherEvents.Count; i++)
		{
			WeatherEvent weatherEvent = _weatherEvents[i];
			JObject field2 = data.GetField(((Object)weatherEvent).name);
			if (field2 != null)
			{
				weatherEvent.Load(field2);
				break;
			}
		}
	}
}
