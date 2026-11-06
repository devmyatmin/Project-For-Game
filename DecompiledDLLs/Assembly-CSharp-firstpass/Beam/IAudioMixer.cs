namespace Beam;

public interface IAudioMixer
{
	IAudioMixerChannel GetChannel(AudioMixerChannels channel);
}
