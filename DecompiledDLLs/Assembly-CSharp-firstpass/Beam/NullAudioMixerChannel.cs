namespace Beam;

public class NullAudioMixerChannel : NullObject<NullAudioMixerChannel>, IAudioMixerChannel
{
	public bool Mute { get; set; }

	public float Volume { get; set; }
}
