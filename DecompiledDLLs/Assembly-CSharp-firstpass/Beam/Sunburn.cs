using Beam.Serialization.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class Sunburn : ScenePlayerEffect, IIdleUntilInitialized
{
	private const float SUNSTROKE_REMOVE_TIME_HOURS = 0.5f;

	[SerializeField]
	private Player _player;

	[SerializeField]
	private LayerMask _shelterLayerMask;

	[SerializeField]
	private FloatUnityEvent _onUpdateBurn;

	private float _sunExposureTime;

	private float _lastNotification;

	private bool _burnt;

	private bool _initialized;

	public override string Name { get; } = "Sunburn";

	public bool Protected { get; set; }

	public float SunExposureTime
	{
		get
		{
			if (PlayerUtilities.GetServerGameDifficulty() != EGameDifficultyMode.Creative)
			{
				return _sunExposureTime;
			}
			return 0f;
		}
	}

	public bool IsInitialized => _initialized;

	public void Initialize()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if (!_initialized)
		{
			_initialized = true;
			ResetExposure();
			if ((int)Game.State != 1 && PlayerUtilities.GetServerGameDifficulty() != EGameDifficultyMode.Creative)
			{
				_player.Statistics.PlayerEffectApplied += PlayerStatistics_PlayerEffectApplied;
				_player.Statistics.PlayerEffectRemoved += PlayerStatistics_PlayerEffectRemoved;
				Singleton<GameTime>.Instance.HourChanged += GameTime_HourChanged;
				((MonoBehaviour)this).InvokeRepeating("DoProtectionRaycast", 0f, 1f);
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)_player != (Object)null && (Object)(object)_player.Statistics != (Object)null)
		{
			_player.Statistics.PlayerEffectApplied -= PlayerStatistics_PlayerEffectApplied;
			_player.Statistics.PlayerEffectRemoved -= PlayerStatistics_PlayerEffectRemoved;
		}
		if ((Object)(object)Singleton<GameTime>.Instance != (Object)null)
		{
			Singleton<GameTime>.Instance.HourChanged -= GameTime_HourChanged;
		}
	}

	private void PlayerStatistics_PlayerEffectApplied(PlayerEffect effect)
	{
		if (effect.Name == "SUNBLOCK")
		{
			ResetExposure();
		}
		else if (effect.Name == "SUNSTROKE")
		{
			_burnt = true;
			if (Time.time - _lastNotification > 30f)
			{
				IPlayerSpeech playerSpeech = _player.PlayerSpeech;
				(playerSpeech ?? NullObject<NullPlayerSpeech>.Instance).Play("STATUS_SUNBURN", _player.Gender, _player.Movement.IsUnderwater);
			}
			_lastNotification = Time.time;
		}
	}

	private void PlayerStatistics_PlayerEffectRemoved(PlayerEffect effect)
	{
		if (effect.Name == "SUNSTROKE")
		{
			_burnt = false;
		}
	}

	private void GameTime_HourChanged(float hours)
	{
		BurnManual(hours);
	}

	public void ResetExposure()
	{
		_sunExposureTime = 0f;
		((UnityEvent<float>)_onUpdateBurn)?.Invoke(0f);
	}

	public bool IsDayTimeUV()
	{
		if (Singleton<GameTime>.Instance.MilitaryTime >= 10f)
		{
			return Singleton<GameTime>.Instance.MilitaryTime <= 15f;
		}
		return false;
	}

	private void Update()
	{
		if (PlayerUtilities.GetServerGameDifficulty() != EGameDifficultyMode.Creative && _initialized && !LevelLoader.IsLoadingScene() && !_player.Statistics.Invincible && !_player.Statistics.HasStatusEffect(StatusEffect.SUNBLOCK))
		{
			BurnManual(GameTime.DeltaTimeHour);
			if (!_burnt && _sunExposureTime >= 1f)
			{
				_player.Statistics.ApplyStatusEffect(StatusEffect.SUNSTROKE);
			}
			else if (_burnt && _sunExposureTime < 1f)
			{
				_player.Statistics.RemoveStatusEffect(StatusEffect.SUNSTROKE);
			}
			((UnityEvent<float>)_onUpdateBurn)?.Invoke(_sunExposureTime);
		}
	}

	private void BurnManual(float hours)
	{
		bool flag = IsDayTimeUV();
		if (flag && !Protected)
		{
			float num = Mathf.Clamp01(1f / UVIndexUtility.GetBurnTime(Singleton<AtmosphereTemperature>.Instance.GetUVIndex()));
			_sunExposureTime += num * hours;
		}
		else if (!flag || Protected)
		{
			_sunExposureTime -= 0.5f * hours;
		}
		_sunExposureTime = Mathf.Clamp01(_sunExposureTime);
	}

	private void DoProtectionRaycast()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Singleton<Atmosphere>.Instance.SunIlluminary.Transform.position - ((Component)_player.Movement).transform.position;
		Protected = Physics.Raycast(((Component)_player.Movement).transform.position, val, 100f, _shelterLayerMask.value, (QueryTriggerInteraction)2);
		Protected |= _player.Movement.IsUnderwater;
		bool flag = Object.op_Implicit((Object)(object)Singleton<AtmosphereStorm>.Instance) && Object.op_Implicit((Object)(object)Singleton<AtmosphereStorm>.Instance.CurrentWeatherEvent) && Singleton<AtmosphereStorm>.Instance.CurrentWeatherEvent.Humidity > 90f && Singleton<AtmosphereStorm>.Instance.CurrentWeatherEvent.IsActive;
		Protected |= flag;
	}

	public override JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("BurnTimer", _sunExposureTime);
		return jObject;
	}

	public override void Load(JObject data)
	{
		JObject field = data.GetField("BurnTimer");
		if (field != null)
		{
			float num = field.GetValue<float>();
			if (num > 1f)
			{
				num = 0f;
			}
			_sunExposureTime = num;
		}
	}
}
