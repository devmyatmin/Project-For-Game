using UnityEngine;

namespace Beam;

public class HelicopterVehicleAudio : VehicleAudio<HelicopterVehicleMovement>
{
	[SerializeField]
	private AudioClip _warningAudioClip;

	[SerializeField]
	private AudioClip _engineFailedClip;

	private AudioActorHandle _warningAudioHandle;

	protected override void Start()
	{
		base.Start();
		if ((Object)(object)VehicleMovement != (Object)null)
		{
			VehicleMovement.EngineFailed += VehicleMovement_EngineFailed;
		}
	}

	protected override void OnDestroy()
	{
		Stop();
		if ((Object)(object)VehicleMovement != (Object)null)
		{
			VehicleMovement.EngineFailed -= VehicleMovement_EngineFailed;
		}
		base.OnDestroy();
	}

	protected override void VehicleMovement_Operated(bool active)
	{
		base.VehicleMovement_Operated(active);
		if (!active)
		{
			Stop();
		}
	}

	private void VehicleMovement_EngineFailed(bool failed)
	{
		if (failed)
		{
			EngineFailed();
		}
		else
		{
			EngineRestored();
		}
	}

	private void EngineFailed()
	{
		_warningAudioHandle = AudioPlayer.Play3D(_warningAudioClip, ((Component)this).transform, AudioMixerChannels.FX, AudioRollOffDistance.Near, AudioPlayMode.Persistent);
		AudioPlayer.Play3D(_engineFailedClip, ((Component)this).transform, AudioMixerChannels.FX);
	}

	private void EngineRestored()
	{
		AudioPlayer.Play3D(_engineFailedClip, ((Component)this).transform, AudioMixerChannels.FX);
		Stop();
	}

	private void Stop()
	{
		AudioPlayer.Stop(_warningAudioHandle);
		_warningAudioHandle = AudioActorHandle.Empty;
	}
}
