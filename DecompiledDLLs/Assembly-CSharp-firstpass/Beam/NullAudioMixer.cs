namespace Beam;

public class NullAudioMixer : NullObject<NullAudioMixer>, IAudioMixer
{
	public IAudioMixerChannel GetChannel(AudioMixerChannels channel)
	{
		return NullObject<NullAudioMixerChannel>.Instance;
	}
}
