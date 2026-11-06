using System.Collections.Generic;
using System.Linq;
using Beam.Events;
using Beam.Utilities;
using MEC;
using UnityEngine;

namespace Beam;

[RequireComponent(typeof(AudioSource))]
public class SoundtrackManager : Singleton<SoundtrackManager>
{
	private const float POLL_INTERVAL_SECONDS = 10f;

	private const float FADE_IN_RATE = 1f / 3f;

	private const float FADE_OUT_RATE = 1f / 3f;

	private const float AMBIENCE_DELAY_MINUTES = 2f;

	private const float AMBIENCE_DELAY_SECONDS = 120f;

	[SerializeField]
	private List<Track> _tracks = new List<Track>();

	private TrackType _currentTrackType;

	private float _ambienceTimer;

	private AudioSource _audio;

	private CoroutineHandle _startCoroutine;

	private CoroutineHandle _transitionCoroutine;

	private CoroutineHandle _fadeOutCoroutine;

	private bool _isRaining;

	private bool _passiveWildlife;

	private void Awake()
	{
		_audio = ((Component)this).GetComponent<AudioSource>();
		_audio.volume = 0f;
	}

	private void Start()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Invalid comparison between Unknown and I4
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Invalid comparison between Unknown and I4
		EventManager.AddListener<TrackEvent>(EventManager_TrackEvent);
		if ((Object)(object)Singleton<AtmosphereStorm>.Instance != (Object)null)
		{
			Singleton<AtmosphereStorm>.Instance.WeatherStarted += AtmosphereStorm_WeatherStarted;
			Singleton<AtmosphereStorm>.Instance.WeatherStopped += AtmosphereStorm_WeatherStopped;
			if ((Object)(object)Singleton<AtmosphereStorm>.Instance.CurrentWeatherEvent != (Object)null)
			{
				AtmosphereStorm_WeatherStarted(Singleton<AtmosphereStorm>.Instance.CurrentWeatherEvent);
			}
		}
		if ((int)Game.State != 1)
		{
			if ((int)Game.State == 0)
			{
				PlayTrack(TrackType.MainMenu);
			}
			else if ((int)Game.State != 4)
			{
				if ((int)Game.State == 2)
				{
					PlayTrack(TrackType.Ambience_First_Arrival);
				}
				else if ((int)Game.State == 3)
				{
					_ambienceTimer = 120f;
				}
				((MonoBehaviour)this).InvokeRepeating("PollSoundtrack_InvokeRepeating", 1f, 10f);
			}
		}
		SetDifficultyParams();
	}

	private void SetDifficultyParams()
	{
		bool flag = PlayerUtilities.GetServerWildlifeMode() == EGameWildlifeMode.Passive;
		if (PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Creative)
		{
			flag = true;
		}
		if (flag)
		{
			_passiveWildlife = true;
		}
	}

	protected override void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_startCoroutine);
		Timing.KillCoroutines(_transitionCoroutine);
		Timing.KillCoroutines(_fadeOutCoroutine);
		if ((Object)(object)Singleton<AtmosphereStorm>.Instance != (Object)null)
		{
			Singleton<AtmosphereStorm>.Instance.WeatherStarted -= AtmosphereStorm_WeatherStarted;
			Singleton<AtmosphereStorm>.Instance.WeatherStopped -= AtmosphereStorm_WeatherStopped;
		}
		EventManager.RemoveListener<TrackEvent>(EventManager_TrackEvent);
		base.OnDestroy();
	}

	private void EventManager_TrackEvent(TrackEvent ev)
	{
		if (!_passiveWildlife || ev.TrackType != TrackType.Ambience_Shark)
		{
			if (ev.EventType == TrackEventType.Play)
			{
				PlayTrack(ev.TrackType);
			}
			else if (ev.EventType == TrackEventType.Play_Instantly)
			{
				PlayTrackInstantly(ev.TrackType);
			}
			else
			{
				StopTrack(ev.TrackType);
			}
		}
	}

	private void PollSoundtrack_InvokeRepeating()
	{
		if (LevelLoader.IsServerJoinInProgress)
		{
			return;
		}
		if (_currentTrackType != TrackType.None && _audio.isPlaying)
		{
			if (_currentTrackType != TrackType.Ambience_Shark)
			{
				_ambienceTimer = 0f;
			}
			return;
		}
		_ambienceTimer += 10f;
		if (!(_ambienceTimer > 120f))
		{
			return;
		}
		if (Beam.Utilities.ExtensionMethods.Any_NonAlloc(PlayerRegistry.AllPlayers, IsPlayerDying))
		{
			PlayTrack(TrackType.Ambience_Life_Threatening);
		}
		else
		{
			if (_isRaining)
			{
				return;
			}
			if (Beam.Utilities.ExtensionMethods.Any_NonAlloc(PlayerRegistry.AllPlayers, IsPlayerOperating))
			{
				if (Random.value < 0.5f)
				{
					PlayTrack(TrackType.Ambience_Sailing_1);
				}
				else
				{
					PlayTrack(TrackType.Ambience_Sailing_2);
				}
			}
			else if (Singleton<Atmosphere>.Instance.DayTime)
			{
				if (Random.value < 0.5f)
				{
					PlayTrack(TrackType.Ambience_Sunny_1);
				}
				else
				{
					PlayTrack(TrackType.Ambience_Sunny_2);
				}
			}
			else if (!Singleton<Atmosphere>.Instance.DayTime)
			{
				PlayTrack(TrackType.Ambience_Night);
			}
		}
	}

	public void PlayTrack(TrackType trackType)
	{
		if (_currentTrackType == TrackType.None)
		{
			StartTrack(trackType);
		}
		else
		{
			TransitionTrack(trackType);
		}
	}

	public void PlayTrackInstantly(TrackType trackType)
	{
		StartTrack(trackType, 100f);
	}

	public void PauseTracks()
	{
		if (_audio.isPlaying)
		{
			_audio.Pause();
		}
	}

	public void ResumeTracks()
	{
		_audio.UnPause();
	}

	private void StopTrack(TrackType trackType)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTrackType == trackType)
		{
			Timing.KillCoroutines(_fadeOutCoroutine);
			Timing.KillCoroutines(_transitionCoroutine);
			_fadeOutCoroutine = Timing.RunCoroutine(FadeOutTrack_Sequence());
		}
	}

	private void StartTrack(TrackType trackType, float fadeInRate = 1f / 3f)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTrackType != trackType && (!_audio.isPlaying || GetTrackPriority(_currentTrackType) <= GetTrackPriority(trackType)))
		{
			Track track = GetTrack(trackType);
			if (track != null)
			{
				Timing.KillCoroutines(_startCoroutine);
				_startCoroutine = Timing.RunCoroutine(StartTrack_Sequence(track, fadeInRate));
			}
		}
	}

	private IEnumerator<float> StartTrack_Sequence(Track track, float fadeInRate)
	{
		_audio.volume = 0f;
		_audio.clip = track.Clip;
		_audio.loop = track.Loop;
		_audio.Play();
		_currentTrackType = track.Type;
		do
		{
			AudioSource audio = _audio;
			audio.volume += fadeInRate * Time.deltaTime;
		}
		while (!(_audio.volume >= 1f));
		_audio.volume = 1f;
		yield break;
	}

	private void TransitionTrack(TrackType trackType)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTrackType != trackType && (!_audio.isPlaying || GetTrackPriority(_currentTrackType) <= GetTrackPriority(trackType)))
		{
			Track track = GetTrack(trackType);
			if (track != null)
			{
				Timing.KillCoroutines(_transitionCoroutine);
				_transitionCoroutine = Timing.RunCoroutine(TransitionTrack_Sequence(track));
			}
		}
	}

	private IEnumerator<float> TransitionTrack_Sequence(Track track)
	{
		while (true)
		{
			if (_currentTrackType != track.Type)
			{
				AudioSource audio = _audio;
				audio.volume -= 1f / 3f * Time.deltaTime;
				if (_audio.volume <= 0f)
				{
					_audio.volume = 0f;
					_audio.clip = track.Clip;
					_audio.loop = track.Loop;
					_audio.Play();
					_currentTrackType = track.Type;
				}
			}
			else
			{
				AudioSource audio2 = _audio;
				audio2.volume += 1f / 3f * Time.deltaTime;
				if (_audio.volume >= 1f)
				{
					break;
				}
			}
			yield return 0f;
		}
		_audio.volume = 1f;
	}

	private IEnumerator<float> FadeOutTrack_Sequence()
	{
		while (true)
		{
			AudioSource audio = _audio;
			audio.volume -= 1f / 3f * Time.deltaTime;
			if (_audio.volume <= 0f)
			{
				break;
			}
			yield return 0f;
		}
		_audio.Stop();
		_audio.volume = 0f;
		_audio.clip = null;
		_audio.loop = false;
		_currentTrackType = TrackType.None;
	}

	private Track GetTrack(TrackType trackType)
	{
		return _tracks.FirstOrDefault((Track t) => t.Type == trackType);
	}

	private int GetTrackPriority(TrackType trackType)
	{
		return trackType switch
		{
			TrackType.Player_Death => 100, 
			TrackType.Ambience_Cockpit => 10, 
			TrackType.Cockpit_Crash => 10, 
			TrackType.Cockpit_TakeOff => 10, 
			TrackType.Boss_Victory => 2, 
			TrackType.Boss_Failed => 2, 
			TrackType.Boss_Shark => 2, 
			TrackType.Boss_Eel => 2, 
			TrackType.Boss_Squid => 2, 
			TrackType.Boss_Shark_Intro => 2, 
			TrackType.Boss_Eel_Intro => 2, 
			TrackType.Boss_Squid_Intro => 2, 
			TrackType.Ambience_Shark => 1, 
			_ => 0, 
		};
	}

	private void AtmosphereStorm_WeatherStarted(WeatherEvent weather)
	{
		if (Mathf.Approximately(weather.Humidity, 100f))
		{
			_isRaining = true;
		}
	}

	private void AtmosphereStorm_WeatherStopped()
	{
		_isRaining = false;
	}

	private bool IsPlayerDying(IPlayer player)
	{
		if (player.IsInitialized)
		{
			return player.Statistics.HealthPercent < 0.15f;
		}
		return false;
	}

	private bool IsPlayerOperating(IPlayer player)
	{
		if (player.IsInitialized)
		{
			return player.Movement.Vehicle != null;
		}
		return false;
	}
}
