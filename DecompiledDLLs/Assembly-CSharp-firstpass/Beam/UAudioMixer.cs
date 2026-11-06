using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

namespace Beam;

public class UAudioMixer : IAudioMixer
{
	private readonly AudioMixer _audioMixer;

	private readonly List<UAudioMixerChannel> _audioMixerChannels;

	public UAudioMixer(AudioMixer audioMixer)
	{
		_audioMixer = audioMixer;
		_audioMixerChannels = new List<UAudioMixerChannel>
		{
			new UAudioMixerChannel(_audioMixer, AudioMixerChannels.Master),
			new UAudioMixerChannel(_audioMixer, AudioMixerChannels.Music),
			new UAudioMixerChannel(_audioMixer, AudioMixerChannels.UI),
			new UAudioMixerChannel(_audioMixer, AudioMixerChannels.Environment),
			new UAudioMixerChannel(_audioMixer, AudioMixerChannels.FX),
			new UAudioMixerChannel(_audioMixer, AudioMixerChannels.Voice)
		};
	}

	public IAudioMixerChannel GetChannel(AudioMixerChannels channel)
	{
		return _audioMixerChannels.FirstOrDefault((UAudioMixerChannel x) => x.Channel == channel);
	}

	public AudioMixerGroup[] FindMatchingGroups(string name)
	{
		return _audioMixer.FindMatchingGroups(name);
	}
}
