using Beam.Utilities;
using UnityEngine.Audio;

namespace Beam;

public class UAudioMixerChannel : IAudioMixerChannel
{
	private readonly AudioMixer _audioMixer;

	private readonly AudioMixerChannels _channel;

	private AudioMixerGroup _group;

	private bool _mute;

	private float _volume;

	private string _volParam;

	public AudioMixerChannels Channel => _channel;

	public bool Mute
	{
		get
		{
			return _mute;
		}
		set
		{
			_mute = value;
			_audioMixer.SetFloat(VolumeParameter, _mute ? (-80f) : BeamTeam.LinearToDecibel(_volume));
		}
	}

	public float Volume
	{
		get
		{
			return _volume;
		}
		set
		{
			_volume = value;
			if (!_mute)
			{
				_audioMixer.SetFloat(VolumeParameter, BeamTeam.LinearToDecibel(_volume));
			}
		}
	}

	private string VolumeParameter => _volParam ?? (_volParam = Channel.ToString() + "Volume");

	public UAudioMixerChannel(AudioMixer audioMixer, AudioMixerChannels channel)
	{
		_audioMixer = audioMixer;
		_channel = channel;
	}
}
