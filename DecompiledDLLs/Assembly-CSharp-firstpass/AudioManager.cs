using System.Collections.Generic;
using System.Linq;
using Beam;
using Beam.Events;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour, IAudioPlayer
{
	private const int AUDIO_PRIORITY_2D = 126;

	private const int AUDIO_PRIORITY_3D = 127;

	private static AudioManager _Instance;

	[SerializeField]
	private Transform _pooledParent;

	[SerializeField]
	private Transform _activeParent;

	[Header("MIXER")]
	[SerializeField]
	private AudioMixer _audioMixer;

	[SerializeField]
	private AudioMixerGroup _musicMixerGroup;

	[SerializeField]
	private AudioMixerGroup _environmentMixerGroup;

	[SerializeField]
	private AudioMixerGroup _fxMixerGroup;

	[SerializeField]
	private AudioMixerGroup _voiceMixerGroup;

	[SerializeField]
	private AudioMixerGroup _uiMixerGroup;

	[SerializeField]
	private AudioMixerSnapshot _regularMixerSnapshot;

	[SerializeField]
	private AudioMixerSnapshot _underwaterMixerSnapshot;

	[Header("ROLLOFF PRESETS")]
	[SerializeField]
	private AudioRollOff _veryNearRollOff;

	[SerializeField]
	private AudioRollOff _nearRollOff;

	[SerializeField]
	private AudioRollOff _farRollOff;

	[SerializeField]
	private AudioRollOff _veryFarRollOff;

	private IPlayer[] _listeners;

	private CoroutineHandle _updateSequence;

	private UAudioMixer _uAudioMixer;

	public static IAudioPlayer GetAudioPlayer()
	{
		IAudioPlayer instance = _Instance;
		return instance ?? NullAudioPlayer.Instance;
	}

	private void Awake()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		_Instance = this;
		if ((int)Game.State != 4)
		{
			_listeners = new IPlayer[MPExtensions.GetLocalPlayersCount(Game.Mode)];
			_updateSequence = Timing.RunCoroutine(Update_Sequence(), (Segment)3);
		}
	}

	private void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)Game.State == 4)
		{
			return;
		}
		if (LevelLoader.IsLoading())
		{
			LevelLoader.CurrentLoader.PostLoading.Add(() =>
			{
				PlayerRegistry.LocalPlayer.PlayerCamera.ToggleUnderwater += Cameras_ToggleUnderwater;
			});
		}
		else
		{
			PlayerRegistry.LocalPlayer.PlayerCamera.ToggleUnderwater += Cameras_ToggleUnderwater;
		}
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
		LoadOptions();
	}

	protected void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_updateSequence);
		AudioActorFactory<AudioActor>.Dispose();
		AudioActorFactory<SpatialAudioActor>.Dispose();
		Cameras_ToggleUnderwater(underwater: false);
		if (PlayerRegistry.LocalPlayer != null && (Object)(object)PlayerRegistry.LocalPlayer.PlayerCamera != (Object)null)
		{
			PlayerRegistry.LocalPlayer.PlayerCamera.ToggleUnderwater -= Cameras_ToggleUnderwater;
		}
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
		if ((Object)(object)_Instance == (Object)(object)this)
		{
			_Instance = null;
		}
	}

	private void LoadOptions()
	{
		GetAudioMixer().GetChannel(AudioMixerChannels.Master).Volume = Options.AudioSettings.MasterVolume;
		GetAudioMixer().GetChannel(AudioMixerChannels.Music).Volume = Options.AudioSettings.MusicVolume;
		GetAudioMixer().GetChannel(AudioMixerChannels.UI).Volume = Options.AudioSettings.UIVolume;
		GetAudioMixer().GetChannel(AudioMixerChannels.Environment).Volume = Options.AudioSettings.EnvironmentVolume;
		GetAudioMixer().GetChannel(AudioMixerChannels.FX).Volume = Options.AudioSettings.EffectsVolume;
		GetAudioMixer().GetChannel(AudioMixerChannels.Voice).Volume = Options.AudioSettings.VoiceVolume;
	}

	private void EventManager_OptionsAppliedEvent(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void Cameras_ToggleUnderwater(bool underwater)
	{
		(underwater ? _underwaterMixerSnapshot : _regularMixerSnapshot).TransitionTo(0f);
	}

	public AudioActorHandle Play2D(AudioClip clip, AudioMixerChannels channel, AudioPlayMode mode = AudioPlayMode.Single)
	{
		return Play2D_(clip, channel, mode)?.Handle ?? AudioActorHandle.Empty;
	}

	private AudioActor Play2D_(AudioClip clip, AudioMixerChannels channel, AudioPlayMode mode = AudioPlayMode.Single)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)clip == (Object)null)
		{
			return null;
		}
		AudioActor audioActor = Pop<AudioActor>();
		audioActor.Position = Vector3.zero;
		audioActor.Source.Clip = clip;
		audioActor.Source.Loop = mode == AudioPlayMode.Persistent;
		audioActor.Source.Priority = 126;
		audioActor.Source.OutputAudioMixerGroup = GetAudioMixerGroup(channel);
		audioActor.Source.Play();
		return audioActor;
	}

	public AudioActorHandle Play3D(AudioClip clip, Vector3 position, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Play3D_(clip, position, channel, distance, mode)?.Handle ?? AudioActorHandle.Empty;
	}

	public AudioActorHandle Play3D(AudioClip[] clips, Vector3 position, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (clips == null || clips.Length == 0)
		{
			return AudioActorHandle.Empty;
		}
		AudioClip clip = clips[Random.Range(0, clips.Length)];
		SpatialAudioActor spatialAudioActor = Play3D_(clip, position, channel, distance, mode);
		if (spatialAudioActor != null)
		{
			spatialAudioActor.Target = null;
			return spatialAudioActor.Handle;
		}
		return AudioActorHandle.Empty;
	}

	public AudioActorHandle Play3D(AudioClip clip, Transform target, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		SpatialAudioActor spatialAudioActor = Play3D_(clip, target.position, channel, distance, mode);
		if (spatialAudioActor != null)
		{
			spatialAudioActor.Target = target;
			return spatialAudioActor.Handle;
		}
		return AudioActorHandle.Empty;
	}

	private SpatialAudioActor Play3D_(AudioClip clip, Vector3 position, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)clip == (Object)null)
		{
			return null;
		}
		AudioRollOff audioRollOff = GetAudioRollOff(distance);
		int listeners = GetListeners(position, audioRollOff.MaxDistance);
		if (listeners == 0 && mode == AudioPlayMode.Single)
		{
			return null;
		}
		SpatialAudioActor spatialAudioActor = Pop<SpatialAudioActor>();
		spatialAudioActor.Position = position;
		spatialAudioActor.Source.Clip = clip;
		spatialAudioActor.Source.Loop = mode == AudioPlayMode.Persistent;
		spatialAudioActor.Source.OutputAudioMixerGroup = GetAudioMixerGroup(channel);
		spatialAudioActor.Source.RollOff = audioRollOff;
		spatialAudioActor.Source.Priority = 127;
		spatialAudioActor.Target = null;
		if (listeners > 0)
		{
			spatialAudioActor.Source.Play();
		}
		return spatialAudioActor;
	}

	private TAudioActor Pop<TAudioActor>() where TAudioActor : AudioActor
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		TAudioActor val = AudioActorFactory<TAudioActor>.Create();
		val.Source.transform.SetParent(_activeParent, false);
		val.Source.transform.position = Vector3.zero;
		return val;
	}

	private void Pool<TAudioActor>(TAudioActor audioActor) where TAudioActor : AudioActor
	{
		audioActor.Source.transform.SetParent(_pooledParent, false);
		AudioActorFactory<TAudioActor>.Pool(audioActor);
	}

	public void Stop(AudioActorHandle handle)
	{
		if (!handle.IsValid())
		{
			return;
		}
		AudioActor audioActor = AudioActorFactory<AudioActor>.GetActiveActors().FirstOrDefault((AudioActor s) => s.Handle == handle);
		if (audioActor != null)
		{
			Pool(audioActor);
			return;
		}
		SpatialAudioActor spatialAudioActor = AudioActorFactory<SpatialAudioActor>.GetActiveActors().FirstOrDefault((SpatialAudioActor s) => s.Handle == handle);
		if (spatialAudioActor != null)
		{
			Pool(spatialAudioActor);
		}
	}

	public IAudioSource GetAudioSource(AudioActorHandle handle)
	{
		if (!handle.IsValid())
		{
			return NullAudioSource.Instance;
		}
		AudioActor audioActor = AudioActorFactory<AudioActor>.GetActiveActors().FirstOrDefault((AudioActor s) => s.Handle == handle);
		if (audioActor != null)
		{
			return audioActor.Source ?? NullAudioSource.Instance;
		}
		SpatialAudioActor spatialAudioActor = AudioActorFactory<SpatialAudioActor>.GetActiveActors().FirstOrDefault((SpatialAudioActor s) => s.Handle == handle);
		if (spatialAudioActor != null)
		{
			return spatialAudioActor.Source ?? NullAudioSource.Instance;
		}
		return NullAudioSource.Instance;
	}

	public IAudioMixer GetAudioMixer()
	{
		return _uAudioMixer ?? (_uAudioMixer = new UAudioMixer(_audioMixer));
	}

	private int GetListeners(Vector3 position, float maxDistance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return PlayerUtilities.FindPlayers_NonAlloc(_listeners, position, maxDistance, MPExtensions.IsMultiplayer(Game.Mode) ? PlayerUtilities.LocalPlayers() : PlayerUtilities.AllPlayers());
	}

	private IEnumerator<float> Update_Sequence()
	{
		while (true)
		{
			IList<AudioActor> activeActors = AudioActorFactory<AudioActor>.GetActiveActors();
			for (int num = activeActors.Count - 1; num >= 0; num--)
			{
				AudioActor audioActor = activeActors[num];
				if (!audioActor.Source.Loop && !audioActor.Source.IsPlaying)
				{
					Pool(audioActor);
				}
			}
			IList<SpatialAudioActor> activeActors2 = AudioActorFactory<SpatialAudioActor>.GetActiveActors();
			for (int num2 = activeActors2.Count - 1; num2 >= 0; num2--)
			{
				SpatialAudioActor spatialAudioActor = activeActors2[num2];
				if (!spatialAudioActor.Source.Loop && !spatialAudioActor.Source.IsPlaying)
				{
					Pool(spatialAudioActor);
				}
				else
				{
					if ((Object)(object)spatialAudioActor.Target != (Object)null)
					{
						spatialAudioActor.Position = spatialAudioActor.Target.position;
					}
					int listeners = GetListeners(spatialAudioActor.Position, spatialAudioActor.Source.RollOff.MaxDistance);
					if (listeners == 0)
					{
						if (!spatialAudioActor.Source.Loop)
						{
							Pool(spatialAudioActor);
						}
						else if (spatialAudioActor.Source.IsPlaying)
						{
							spatialAudioActor.Source.Pause();
						}
					}
					else
					{
						float num3 = 0f;
						float num4 = 0f;
						for (int i = 0; i < listeners; i++)
						{
							IPlayer player = _listeners[i];
							Vector3 val = player.transform.position - spatialAudioActor.Position;
							float magnitude = val.magnitude;
							float num5 = spatialAudioActor.Source.RollOff.Evaluate(magnitude);
							num3 += num5 * (float)((player.Id != 0) ? 1 : (-1));
							num4 += num5;
						}
						if (num4 > 0f)
						{
							spatialAudioActor.Source.Pan = ((MPExtensions.GetLocalPlayersCount(Game.Mode) == 1) ? 0f : (num3 / num4));
							spatialAudioActor.Source.Level = Mathf.Clamp01(num4 / (float)listeners);
						}
						if (spatialAudioActor.Source.IsPaused)
						{
							spatialAudioActor.Source.Unpause();
						}
						else if (!spatialAudioActor.Source.IsPlaying)
						{
							spatialAudioActor.Source.Play();
						}
					}
				}
			}
			yield return 0f;
		}
	}

	public AudioMixerGroup GetAudioMixerGroup(AudioMixerChannels channel)
	{
		return channel switch
		{
			AudioMixerChannels.Music => _musicMixerGroup, 
			AudioMixerChannels.Environment => _environmentMixerGroup, 
			AudioMixerChannels.FX => _fxMixerGroup, 
			AudioMixerChannels.Voice => _voiceMixerGroup, 
			AudioMixerChannels.UI => _uiMixerGroup, 
			_ => (AudioMixerGroup)null, 
		};
	}

	private AudioRollOff GetAudioRollOff(AudioRollOffDistance distance)
	{
		return distance switch
		{
			AudioRollOffDistance.VeryNear => _veryNearRollOff, 
			AudioRollOffDistance.Near => _nearRollOff, 
			AudioRollOffDistance.VeryFar => _veryFarRollOff, 
			_ => _farRollOff, 
		};
	}
}
