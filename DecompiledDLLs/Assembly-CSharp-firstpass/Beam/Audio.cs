using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class Audio : MonoBehaviour, INeedPlayerComponent, IIdleUntilInitialized
{
	[SerializeField]
	private Footstep _footstep;

	[SerializeField]
	private AudioClip[] _climbLedge;

	[SerializeField]
	private AudioClip[] _jump;

	[SerializeField]
	private AudioClip[] _landGround;

	[SerializeField]
	private AudioClip[] _landWater;

	[SerializeField]
	private AudioClip[] _airGaspQuick;

	[SerializeField]
	private AudioClip[] _airGaspLight;

	[SerializeField]
	private AudioClip[] _airGaspMed;

	[SerializeField]
	private AudioClip[] _airGaspHeavy;

	[SerializeField]
	private AudioClip _heavyBreathingClip;

	[SerializeField]
	private AudioClip _drowningClip;

	[SerializeField]
	private AudioClip[] _climbLedgeFemale;

	[SerializeField]
	private AudioClip[] _jumpFemale;

	[SerializeField]
	private AudioClip[] _landGroundFemale;

	[SerializeField]
	private AudioClip[] _landWaterFemale;

	[SerializeField]
	private AudioClip[] _airGaspQuickFemale;

	[SerializeField]
	private AudioClip[] _airGaspLightFemale;

	[SerializeField]
	private AudioClip[] _airGaspMedFemale;

	[SerializeField]
	private AudioClip[] _airGaspHeavyFemale;

	[SerializeField]
	private AudioClip _heavyBreathingClipFemale;

	[SerializeField]
	private AudioClip _drowningClipFemale;

	private IPlayer _player;

	private AudioActorHandle _drowningSourceHandle;

	private IAudioSource _drowningSource;

	private bool _initialized;

	public Footstep Footstep => _footstep;

	public IPlayer Player => _player;

	public bool IsInitialized => _initialized;

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Movement.ClimbedLedge += PlayerMovement_ClimbedLedge;
		_player.Movement.Jumped += PlayerMovement_Jumped;
		_player.Movement.Landed += PlayerMovement_Landed;
		_player.Statistics.Dived += PlayerStatistics_Dived;
		_player.Statistics.Resurfaced += PlayerStatistics_Resurfaced;
		_player.Statistics.OxygenUpdated += PlayerStatistics_OxygenUpdated;
		_player.Statistics.KnockedOut += PlayerStatistics_KnockedOut;
		_footstep.PlayerMovement = _player.Movement;
	}

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			_drowningSourceHandle = AudioManager.GetAudioPlayer().Play3D((_player.Gender == 0) ? _drowningClip : _drowningClipFemale, ((Component)this).transform, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear, AudioPlayMode.Persistent);
			_drowningSource = AudioManager.GetAudioPlayer().GetAudioSource(_drowningSourceHandle);
			_drowningSource.Volume = 0f;
		}
	}

	private void OnDestroy()
	{
		if (_player != null)
		{
			_player.Movement.ClimbedLedge -= PlayerMovement_ClimbedLedge;
			_player.Movement.Jumped -= PlayerMovement_Jumped;
			_player.Movement.Landed -= PlayerMovement_Landed;
			_player.Statistics.Dived -= PlayerStatistics_Dived;
			_player.Statistics.Resurfaced -= PlayerStatistics_Resurfaced;
			_player.Statistics.OxygenUpdated -= PlayerStatistics_OxygenUpdated;
			_player.Statistics.KnockedOut -= PlayerStatistics_KnockedOut;
		}
		AudioManager.GetAudioPlayer().Stop(_drowningSourceHandle);
		_drowningSourceHandle = AudioActorHandle.Empty;
	}

	private void PlayerMovement_ClimbedLedge()
	{
		PlaySoundRange((_player.Gender == 0) ? _climbLedge : _climbLedgeFemale);
	}

	private void PlayerMovement_Jumped()
	{
		PlaySoundRange((_player.Gender == 0) ? _jump : _jumpFemale);
	}

	private void PlayerMovement_Landed(LandType land)
	{
		Audio audio = this;
		AudioClip[] clips;
		if (land != LandType.Ground)
		{
			clips = ((_player.Gender == 0) ? _landWater : _landWaterFemale);
		}
		else
		{
			clips = ((_player.Gender == 0) ? _landGround : _landGroundFemale);
		}
		audio.PlaySoundRange(clips);
	}

	private void PlayerStatistics_Dived()
	{
		if (_drowningSourceHandle != AudioActorHandle.Empty)
		{
			_drowningSource.Volume = 0f;
			_drowningSource.Play();
		}
	}

	private void PlayerStatistics_Resurfaced(float underwaterTime)
	{
		if (_drowningSourceHandle != AudioActorHandle.Empty)
		{
			_drowningSource.Volume = 0f;
			_drowningSource.Pause();
		}
		if (underwaterTime > 20f)
		{
			PlaySoundRange((_player.Gender == 0) ? _airGaspHeavy : _airGaspHeavyFemale);
		}
		else if (underwaterTime > 12f)
		{
			PlaySoundRange((_player.Gender == 0) ? _airGaspMed : _airGaspMedFemale);
		}
		else if (underwaterTime > 10f)
		{
			PlaySoundRange((_player.Gender == 0) ? _airGaspLight : _airGaspLightFemale);
		}
		else if (underwaterTime > 2f)
		{
			PlaySoundRange((_player.Gender == 0) ? _airGaspQuick : _airGaspQuickFemale);
		}
	}

	private void PlayerStatistics_OxygenUpdated(float oxygen)
	{
		if (!(_drowningSourceHandle == AudioActorHandle.Empty) && !_player.Statistics.IsKnockedOut)
		{
			float num = 1f - Easing.EaseOut(BeamMath.Normalize(oxygen, 0f, 0.5f), EasingType.Quadratic);
			_drowningSource.Volume = ((oxygen < 0.5f) ? num : 0f);
		}
	}

	private void PlayerStatistics_KnockedOut(bool isKnockedOut)
	{
		if (!(_drowningSourceHandle == AudioActorHandle.Empty) && isKnockedOut)
		{
			_drowningSource.Volume = 0f;
		}
	}

	public void PlaySound(AudioClip clip)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(clip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}

	public void PlaySoundRange(AudioClip[] clips)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(clips, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}
}
