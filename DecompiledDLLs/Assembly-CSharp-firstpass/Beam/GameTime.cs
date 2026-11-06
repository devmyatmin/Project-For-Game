using System;
using System.Globalization;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

namespace Beam;

public class GameTime : Singleton<GameTime>, IPersistentSaveable, ISaveable
{
	private class PostReplicateTick_ : MultiplayerMessage
	{
		public override void OnPeer()
		{
			Singleton<GameTime>.Instance.ReplicateTick();
		}
	}

	private class ReplicateTick_ : MultiplayerMessage
	{
		[Replicate]
		public float Tick;

		public override void OnPeer()
		{
			if ((Object)(object)Singleton<GameTime>.Instance != (Object)null)
			{
				Singleton<GameTime>.Instance.Tick = Tick;
			}
		}
	}

	private class ReplicateAddToTick : MultiplayerMessage
	{
		[Replicate]
		public float Delta;

		public override void OnPeer()
		{
			Singleton<GameTime>.Instance.Tick += Delta;
		}
	}

	public static int TIME_SCALE = 36;

	private const int MINUTES_IN_ONE_HOUR = 60;

	private const int SECONDS_IN_ONE_HOUR = 3600;

	private const int SECONDS_IN_ONE_DAY = 86400;

	private const float GAME_TIME_TICK_SYNC_INTERVAL = 12.5f;

	private static float NEW_DAY_START_HOUR = 9f;

	[SerializeField]
	private bool _paused;

	private float _normalizedTime;

	[SerializeField]
	private int _hours;

	[SerializeField]
	private int _minutes;

	[SerializeField]
	private int _seconds;

	private float _sessionDuration;

	private float _tick = (float)GAME_DAY_DURATION / 2f;

	private bool _newDay;

	private static int GAME_DAY_DURATION => 86400 / TIME_SCALE;

	private static float NEW_DAY_START_TICKS => 3600f * NEW_DAY_START_HOUR / (float)TIME_SCALE;

	public bool Paused
	{
		get
		{
			return _paused;
		}
		set
		{
			_paused = value;
		}
	}

	public static float RealDeltaTime
	{
		get
		{
			if (!Time.inFixedTimeStep)
			{
				return Time.deltaTime;
			}
			return Time.fixedDeltaTime;
		}
	}

	public static float DeltaTimeSecond
	{
		get
		{
			if (!Singleton<GameTime>.Instance.Paused)
			{
				return RealDeltaTime * (float)TIME_SCALE;
			}
			return 0f;
		}
	}

	public static float DeltaTimeMinute => DeltaTimeSecond / 60f;

	public static float DeltaTimeHour => DeltaTimeMinute / 60f;

	public static float DeltaTimeDay => DeltaTimeHour / 24f;

	public static float OneGameMinute => 60f / (float)TIME_SCALE;

	public static float OneGameHour => OneGameMinute * 60f;

	public static float OneGameDay => OneGameHour * 24f;

	public static DateTime Now => new DateTime(Singleton<GameCalendar>.Instance.Year, Singleton<GameCalendar>.Instance.Month + 1, Singleton<GameCalendar>.Instance.Day + 1, Singleton<GameTime>.Instance.Hours, Singleton<GameTime>.Instance.Minutes, Singleton<GameTime>.Instance.Seconds, 0, CultureInfo.InvariantCulture.Calendar);

	public float NormalizedTime => _normalizedTime;

	public float NormalizedPingPongTime => Mathf.PingPong(_normalizedTime * 2f, 1f);

	public float MilitaryTime
	{
		get
		{
			return _normalizedTime * 24f;
		}
		set
		{
			float num = _normalizedTime * 24f;
			OnHourChanged(Mathf.Clamp(value - num, 0f, 24f));
			Tick = value / 24f * (float)GAME_DAY_DURATION;
			UpdateProperties();
		}
	}

	public float MilitaryTime_NoEvents
	{
		set
		{
			Tick = value / 24f * (float)GAME_DAY_DURATION;
			UpdateProperties();
		}
	}

	public int Hours
	{
		get
		{
			return _hours;
		}
		set
		{
			if (_hours != value)
			{
				OnHourTick();
			}
			_hours = value;
		}
	}

	public int Minutes => _minutes;

	public int Seconds => _seconds;

	public float SessionDuration => _sessionDuration;

	public float Tick
	{
		get
		{
			return _tick;
		}
		set
		{
			_tick = value;
			if (_tick < 0f)
			{
				_tick = GAME_DAY_DURATION - 1;
			}
			if (_tick > NEW_DAY_START_TICKS && _newDay)
			{
				OnNewDayTick();
				_newDay = false;
			}
			if (_tick >= (float)GAME_DAY_DURATION)
			{
				_tick = 0f;
				OnDayTick();
				_newDay = true;
			}
		}
	}

	public event EmptyEventHandler DayTick;

	public event EmptyEventHandler NewDayTick;

	public event EmptyEventHandler HourTick;

	public event FloatEventHandler HourChanged;

	public static float GetMinutesSince(DateTime a)
	{
		return (float)(Now - a).Duration().TotalMinutes;
	}

	public static float GetHoursSince(DateTime a)
	{
		return (float)(Now - a).Duration().TotalHours;
	}

	public static float GetDaysSince(DateTime a)
	{
		return (float)(Now - a).Duration().TotalDays;
	}

	protected virtual void OnDayTick()
	{
		DayTick?.Invoke();
	}

	protected virtual void OnNewDayTick()
	{
		NewDayTick?.Invoke();
	}

	protected virtual void OnHourTick()
	{
		HourTick?.Invoke();
	}

	protected virtual void OnHourChanged(float hours)
	{
		HourChanged?.Invoke(hours);
	}

	protected override void OnDestroy()
	{
		DayTick = null;
		HourTick = null;
		HourChanged = null;
		NewDayTick = null;
		((MonoBehaviour)this).CancelInvoke("ReplicateTickRepeating_");
		base.OnDestroy();
	}

	private void Update()
	{
		if (!_paused)
		{
			Tick += Time.deltaTime;
			_sessionDuration += Time.deltaTime;
		}
		UpdateProperties();
	}

	private void UpdateProperties()
	{
		_normalizedTime = Tick / (float)GAME_DAY_DURATION;
		int num = Mathf.FloorToInt(_normalizedTime * 86400f);
		Hours = num / 3600;
		_minutes = num % 3600 / 60;
		_seconds = num % 3600 % 60;
	}

	public void ForceUpdateTick(float _tick)
	{
		Tick = _tick;
		UpdateProperties();
	}

	public static string GetFormattedTime()
	{
		return $"{Singleton<GameTime>.Instance.Hours.ToString().PadLeft(2, '0')}:{Singleton<GameTime>.Instance.Minutes.ToString().PadLeft(2, '0')}";
	}

	public void PostReplicateTick()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode))
		{
			new PostReplicateTick_().Post();
		}
	}

	public void ReplicateTickRepeating()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			((MonoBehaviour)this).InvokeRepeating("ReplicateTickRepeating_", 0f, 12.5f);
		}
	}

	private void ReplicateTickRepeating_()
	{
		if (!Singleton<GameTime>.Instance.Paused)
		{
			ReplicateTick();
		}
	}

	public void ReplicateTick()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateTick_ replicateTick_ = new ReplicateTick_();
			replicateTick_.Tick = _tick;
			replicateTick_.Post();
		}
	}

	private void ReplicatedAddToTick(float delta)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateAddToTick replicateAddToTick = new ReplicateAddToTick();
			replicateAddToTick.Delta = delta;
			replicateAddToTick.Post();
		}
		Tick += delta;
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Tick", Tick);
		jObject.AddField("SessionDuration", _sessionDuration);
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("Tick");
		if (field != null)
		{
			Tick = field.GetValue<float>();
			if (Tick < NEW_DAY_START_TICKS)
			{
				_newDay = true;
			}
			UpdateProperties();
		}
		JObject field2 = data.GetField("SessionDuration");
		if (field2 != null)
		{
			_sessionDuration = field2.GetValue<float>();
		}
	}
}
