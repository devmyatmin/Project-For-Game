using UnityEngine;

namespace Beam;

public class SnakeAudio : AnimalAudioBase<Snake>
{
	[SerializeField]
	private AudioClip[] _rattledAudioClips;

	protected override void Start()
	{
		base.Start();
		if ((Object)(object)Animal != (Object)null)
		{
			Animal.Rattled += Snake_Rattled;
		}
	}

	protected override void OnDestroy()
	{
		if ((Object)(object)Animal != (Object)null)
		{
			Animal.Rattled -= Snake_Rattled;
		}
		base.OnDestroy();
	}

	public void Snake_Rattled()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(_rattledAudioClips, ((Component)this).transform.position, AudioMixerChannels.FX);
	}
}
