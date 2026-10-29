using UnityEngine;

namespace Beam.UI;

public class DamagePlayerEffect : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private AudioClip[] _aboveWaterClips;

	[SerializeField]
	private AudioClip[] _underWaterClips;

	[SerializeField]
	private AudioClip[] _aboveWaterClipsFemale;

	[SerializeField]
	private AudioClip[] _underWaterClipsFemale;

	private void Start()
	{
		_player.Statistics.Damaged += PlayerStatistics_Damaged;
	}

	private void OnDestroy()
	{
		if (_player.Statistics != null)
		{
			_player.Statistics.Damaged -= PlayerStatistics_Damaged;
		}
	}

	private void PlayerStatistics_Damaged()
	{
		AudioClip[] clips = ((!_player.Movement.IsUnderwater) ? ((_player.Gender == 0) ? _aboveWaterClips : _aboveWaterClipsFemale) : ((_player.Gender == 0) ? _underWaterClips : _underWaterClipsFemale));
		AudioManager.GetAudioPlayer().Play3D(clips, base.transform.position, AudioMixerChannels.Voice, AudioRollOffDistance.Near);
	}
}
