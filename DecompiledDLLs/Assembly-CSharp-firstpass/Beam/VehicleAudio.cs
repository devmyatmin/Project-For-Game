using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Beam;

public class VehicleAudio<TMovement> : MonoBehaviour where TMovement : VehicleMovementBase
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private TMovement _vehicleMovement;

	[SerializeField]
	private AudioClip _startAudioClip;

	[SerializeField]
	private AudioClip _stopAudioClip;

	[SerializeField]
	private AudioClip _runAudioClip;

	[SerializeField]
	private AudioClip _failedAudioClip;

	private IAudioPlayer _audioPlayer;

	private AudioActorHandle _runningAudioHandle;

	private CoroutineHandle _startSequence;

	private CoroutineHandle _stopSequence;

	private CoroutineHandle _runSequence;

	private float _pitch;

	protected TMovement VehicleMovement => _vehicleMovement;

	protected IAudioPlayer AudioPlayer => _audioPlayer;

	protected virtual void Start()
	{
		_audioPlayer = AudioManager.GetAudioPlayer();
		if ((Object)(object)_vehicleMovement != (Object)null)
		{
			_vehicleMovement.Operated += VehicleMovement_Operated;
			_vehicleMovement.OperatingFailed += VehicleMovement_OperatingFailed;
			_vehicleMovement.Using += VehicleMovement_Using;
		}
	}

	protected virtual void OnDestroy()
	{
		_audioPlayer?.Stop(_runningAudioHandle);
		if ((Object)(object)_vehicleMovement != (Object)null)
		{
			_vehicleMovement.Operated -= VehicleMovement_Operated;
			_vehicleMovement.OperatingFailed -= VehicleMovement_OperatingFailed;
			_vehicleMovement.Using -= VehicleMovement_Using;
		}
	}

	protected virtual void VehicleMovement_Operated(bool active)
	{
		if (active)
		{
			StartVehicleAudio();
		}
		else
		{
			StopVehicleAudio();
		}
	}

	private void VehicleMovement_OperatingFailed()
	{
		_audioPlayer.Play3D(_failedAudioClip, ((Component)this).transform, AudioMixerChannels.FX);
	}

	protected virtual void VehicleMovement_Using()
	{
	}

	private void StartVehicleAudio()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_startSequence);
		Timing.KillCoroutines(_runSequence);
		if (Timing.KillCoroutines(_stopSequence) > 0)
		{
			_audioPlayer.Stop(_runningAudioHandle);
			_runningAudioHandle = AudioActorHandle.Empty;
		}
		_startSequence = Timing.RunCoroutine(StartVehicle_Sequence());
	}

	protected void StopVehicleAudio()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_startSequence);
		Timing.KillCoroutines(_runSequence);
		if (_vehicleMovement.EngineIsRunning())
		{
			_stopSequence = Timing.RunCoroutine(StopVehicle_Sequence());
			return;
		}
		_audioPlayer.Stop(_runningAudioHandle);
		_runningAudioHandle = AudioActorHandle.Empty;
		_audioPlayer.Play3D(_stopAudioClip, ((Component)this).transform, AudioMixerChannels.FX);
	}

	private IEnumerator<float> StartVehicle_Sequence()
	{
		if (_audioPlayer.Play3D(_startAudioClip, ((Component)this).transform, AudioMixerChannels.FX) != AudioActorHandle.Empty)
		{
			yield return Timing.WaitForSeconds(_startAudioClip.length);
		}
		_runSequence = Timing.RunCoroutine(RunVehicle_Sequence());
	}

	private IEnumerator<float> RunVehicle_Sequence()
	{
		_runningAudioHandle = _audioPlayer.Play3D(_runAudioClip, ((Component)this).transform, AudioMixerChannels.FX, AudioRollOffDistance.Far, AudioPlayMode.Persistent);
		IAudioSource audioSource = _audioPlayer.GetAudioSource(_runningAudioHandle);
		while (_vehicleMovement.EngineIsRunning())
		{
			float num = Time.deltaTime * 0.7f;
			audioSource.Pitch = Mathf.Lerp(audioSource.Pitch, 1f + Mathf.Abs(_vehicleMovement.GetEngineLoad()) * 0.3f, num);
			audioSource.Volume = Mathf.Lerp(audioSource.Volume, Mathf.Clamp01(0.7f + Mathf.Abs(_vehicleMovement.GetEngineLoad())), num);
			yield return 0f;
		}
	}

	private IEnumerator<float> StopVehicle_Sequence()
	{
		IAudioSource audioSource = _audioPlayer.GetAudioSource(_runningAudioHandle);
		while (_vehicleMovement.EngineIsRunning())
		{
			_pitch = Mathf.Lerp(_pitch, Mathf.Abs(_vehicleMovement.GetEngineLoad()), Time.deltaTime * 0.4f);
			audioSource.Pitch = 1f + _pitch * 0.3f;
			audioSource.Volume = Mathf.Abs(_vehicleMovement.GetEngineLoad());
			yield return 0f;
		}
		_audioPlayer.Stop(_runningAudioHandle);
		_runningAudioHandle = AudioActorHandle.Empty;
	}
}
