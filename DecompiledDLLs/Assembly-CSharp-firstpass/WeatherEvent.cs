using System;
using System.Collections.Generic;
using Beam;
using Beam.Serialization.Json;
using MEC;
using UnityEngine;

public class WeatherEvent : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private int _percentage;

	[SerializeField]
	[HideInInspector]
	private float _buildTime;

	[SerializeField]
	[HideInInspector]
	private float _duration;

	[SerializeField]
	[HideInInspector]
	private float _dissipateTime;

	[SerializeField]
	[HideInInspector]
	private float _temperature;

	[SerializeField]
	[HideInInspector]
	private float _humidity;

	[SerializeField]
	[HideInInspector]
	private bool _affectSpecular;

	[SerializeField]
	[HideInInspector]
	private float _specularPower = 1f;

	[SerializeField]
	[HideInInspector]
	private bool _affectLights;

	[SerializeField]
	[HideInInspector]
	private Gradient _sunColorGradient;

	private Color _sunColor;

	[SerializeField]
	[HideInInspector]
	private Gradient _moonColorGradient;

	private Color _moonColor;

	[SerializeField]
	[HideInInspector]
	private Gradient _ambientSkyColorGradient;

	private Color _ambientSkyColor;

	[SerializeField]
	[HideInInspector]
	private Gradient _ambientGroundColorGradient;

	private Color _ambientGroundColor;

	[SerializeField]
	private List<WeatherEventEffectBase> _effects;

	private float _startTime;

	private DateTime _startedTime;

	private bool _isActive;

	private bool _hasStarted;

	private float _normalizedBuildTime;

	private float _normalizedDissipateTime;

	private bool _fading;

	private CoroutineHandle _startSequence;

	private CoroutineHandle _stopSequence;

	public int Percentage => _percentage;

	public float BuildTime => _buildTime;

	public float Duration => _duration;

	public float DissipateTime => _dissipateTime;

	public float TotalEventMinutes => _buildTime + _duration + _dissipateTime;

	public float TotalEventHours => TotalEventMinutes / 60f;

	public float Temperature => _temperature;

	public float TemperatureC => (_temperature - 32f) * 0.555555f;

	public float Humidity => _humidity;

	public bool AffectSpecular => _affectSpecular;

	public float SpecularPower => _specularPower;

	public bool AffectLights => _affectLights;

	public float StartTime
	{
		get
		{
			return _startTime;
		}
		set
		{
			_startTime = value;
		}
	}

	public DateTime StartedTime
	{
		get
		{
			return _startedTime;
		}
		set
		{
			_startedTime = value;
		}
	}

	public float NormalizedFadeValue
	{
		get
		{
			if (!_fading)
			{
				return _normalizedDissipateTime;
			}
			return _normalizedBuildTime;
		}
	}

	public bool IsActive => _isActive;

	public bool HasStarted => _hasStarted;

	public event WeatherEventHandler Started;

	public event EmptyEventHandler Stopped;

	protected virtual void OnStarted(WeatherEvent weather)
	{
		Started?.Invoke(weather);
	}

	protected virtual void OnStopped()
	{
		Stopped?.Invoke();
	}

	private void OnDestroy()
	{
		Started = null;
		Stopped = null;
	}

	public void StartEvent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_startSequence);
		Timing.KillCoroutines(_stopSequence);
		_startedTime = GameTime.Now;
		_startSequence = Timing.RunCoroutine(StartEvent_Sequence());
		((MonoBehaviour)this).Invoke("DelayedStartEvent", _buildTime * GameTime.OneGameMinute);
	}

	public void StopEvent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_startSequence);
		Timing.KillCoroutines(_stopSequence);
		_stopSequence = Timing.RunCoroutine(StopEvent_Sequence());
	}

	private void DelayedStartEvent()
	{
		OnStarted(this);
	}

	private IEnumerator<float> StartEvent_Sequence()
	{
		InitializeEvent();
		float elapsedMinutes = 0f;
		while (elapsedMinutes < _buildTime + _duration)
		{
			elapsedMinutes = GameTime.GetMinutesSince(_startedTime);
			_isActive = elapsedMinutes > _buildTime;
			_normalizedBuildTime = Mathf.Clamp01(elapsedMinutes / _buildTime);
			float normalizedTime = Singleton<GameTime>.Instance.NormalizedTime;
			UpdateLights(normalizedTime);
			UpdateAmbient(normalizedTime);
			UpdateSpecular();
			UpdateEffects();
			yield return 0f;
		}
		StopEvent();
	}

	private IEnumerator<float> StopEvent_Sequence()
	{
		CeaseEvent();
		float elapsedMinutes = 0f;
		while (elapsedMinutes < TotalEventMinutes)
		{
			elapsedMinutes = GameTime.GetMinutesSince(_startedTime);
			_normalizedDissipateTime = 1f - Mathf.Clamp01((elapsedMinutes - _buildTime - _duration) / _dissipateTime);
			float normalizedTime = Singleton<GameTime>.Instance.NormalizedTime;
			UpdateLights(normalizedTime);
			UpdateAmbient(normalizedTime);
			UpdateSpecular();
			UpdateEffects();
			yield return 0f;
		}
		CeaseSpecular();
		CeaseEffects();
		Singleton<AtmosphereStorm>.Instance?.ClearWeatherEvent();
		((Component)this).gameObject.SetActive(false);
		_hasStarted = false;
	}

	private void InitializeEvent()
	{
		Singleton<AtmosphereStorm>.Instance?.SetWeatherEvent(this);
		((Component)this).gameObject.SetActive(true);
		_hasStarted = true;
		_fading = true;
		_normalizedBuildTime = 0f;
		_normalizedDissipateTime = 1f;
		InitializeEffects();
	}

	private void CeaseEvent()
	{
		_normalizedBuildTime = 0f;
		_normalizedDissipateTime = 1f;
		_isActive = false;
		_fading = false;
		OnStopped();
	}

	public void ResetEvent()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (_hasStarted)
		{
			Timing.KillCoroutines(_startSequence);
			Timing.KillCoroutines(_stopSequence);
			((MonoBehaviour)this).CancelInvoke("DelayedStartEvent");
			CeaseEvent();
			CeaseSpecular();
			CeaseEffects();
		}
		Singleton<AtmosphereStorm>.Instance?.ClearWeatherEvent();
		((Component)this).gameObject.SetActive(false);
		_hasStarted = false;
	}

	public void ResumeEvent()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		float minutesSince = GameTime.GetMinutesSince(_startedTime);
		if (minutesSince < _buildTime + _duration)
		{
			_startSequence = Timing.RunCoroutine(StartEvent_Sequence());
		}
		else if (minutesSince >= _buildTime + _duration)
		{
			InitializeEvent();
			_stopSequence = Timing.RunCoroutine(StopEvent_Sequence());
		}
	}

	private void InitializeEffects()
	{
		for (int i = 0; i < _effects.Count; i++)
		{
			_effects[i].InitializeEffect(this);
		}
	}

	private void UpdateEffects()
	{
		for (int i = 0; i < _effects.Count; i++)
		{
			_effects[i].UpdateEffect(NormalizedFadeValue);
		}
	}

	private void CeaseEffects()
	{
		for (int i = 0; i < _effects.Count; i++)
		{
			_effects[i].Cease();
		}
	}

	private void UpdateLights(float normalizedTimeOfDay)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		_sunColor = _sunColorGradient.Evaluate(normalizedTimeOfDay);
		_moonColor = _moonColorGradient.Evaluate(normalizedTimeOfDay);
	}

	private void UpdateAmbient(float normalizedTimeOfDay)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		_sunColor = _sunColorGradient.Evaluate(normalizedTimeOfDay);
		_moonColor = _moonColorGradient.Evaluate(normalizedTimeOfDay);
		_ambientSkyColor = _ambientSkyColorGradient.Evaluate(normalizedTimeOfDay);
		_ambientGroundColor = _ambientGroundColorGradient.Evaluate(normalizedTimeOfDay);
	}

	private void UpdateSpecular()
	{
		if (_affectSpecular)
		{
			Shader.SetGlobalFloat("_AfsRainamount", 1f * NormalizedFadeValue);
			Shader.SetGlobalFloat("_AfsSpecPower", _specularPower);
		}
	}

	private void CeaseSpecular()
	{
		if (_affectSpecular)
		{
			Shader.SetGlobalFloat("_AfsRainamount", 0f);
			Shader.SetGlobalFloat("_AfsSpecPower", 1f);
		}
	}

	public Color GetSunColor(Color sunColor)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!_affectLights || !_hasStarted)
		{
			return sunColor;
		}
		return Color.Lerp(sunColor, _sunColor, NormalizedFadeValue);
	}

	public Color GetMoonColor(Color moonColor)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!_affectLights || !_hasStarted)
		{
			return moonColor;
		}
		return Color.Lerp(moonColor, _moonColor, NormalizedFadeValue);
	}

	public Color GetAmbientSkyColor(Color skyColor)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasStarted)
		{
			return skyColor;
		}
		return Color.Lerp(skyColor, _ambientSkyColor, NormalizedFadeValue);
	}

	public Color GetAmbientGroundColor(Color groundColor)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasStarted)
		{
			return groundColor;
		}
		return Color.Lerp(groundColor, _ambientGroundColor, NormalizedFadeValue);
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("HasStarted", JSerializer.Serialize((object)_hasStarted));
		jObject.AddField("StartTime", JSerializer.Serialize((object)_startTime));
		jObject.AddField("StartedTime", JSerializer.Serialize((object)_startedTime));
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("HasStarted");
		JObject field2 = data.GetField("StartedTime");
		JObject field3 = data.GetField("StartTime");
		if (field != null && field2 != null && field3 != null)
		{
			_hasStarted = field.GetValue<bool>();
			_startTime = field3.GetValue<float>();
			if (_hasStarted)
			{
				_startedTime = JSerializer.Deserialize<DateTime>(field2);
				ResumeEvent();
			}
			else
			{
				Singleton<AtmosphereStorm>.Instance?.NewWeatherEvent(this);
			}
		}
	}
}
