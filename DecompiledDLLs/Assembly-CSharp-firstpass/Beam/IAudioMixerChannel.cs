namespace Beam;

public interface IAudioMixerChannel
{
	bool Mute { get; set; }

	float Volume { get; set; }
}
