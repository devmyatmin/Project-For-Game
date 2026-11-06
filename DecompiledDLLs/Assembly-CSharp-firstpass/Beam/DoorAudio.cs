using UnityEngine;

namespace Beam;

public class DoorAudio : DamageAudioHelper
{
	[SerializeField]
	private AudioClip _openClip;

	[SerializeField]
	private AudioClip _closeClip;

	public void Door_Opened(IBase sender, IBaseActionEventData data)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (sender is IDoor door)
		{
			AudioManager.GetAudioPlayer().Play3D(door.IsOpened ? _closeClip : _openClip, ((Component)this).transform.position, AudioMixerChannels.FX);
		}
	}
}
