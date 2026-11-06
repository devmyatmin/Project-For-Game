using UnityEngine;

namespace Beam;

public class LifeRaftVehicleAudio : VehicleAudio<VehicleMovementBase>
{
	[SerializeField]
	private AudioClip[] _paddleAudioClips;

	protected override void VehicleMovement_Using()
	{
		base.VehicleMovement_Using();
		AudioPlayer.Play3D(_paddleAudioClips[Random.Range(0, _paddleAudioClips.Length)], ((Component)this).transform, AudioMixerChannels.FX);
	}
}
