using UnityEngine;

namespace Beam.UI;

public class BrokenBonesPlayerEffect : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private AudioClip[] _clips;

	[SerializeField]
	private AudioClip[] _clipsFemale;

	private void Start()
	{
		_player.Statistics.PlayerEffectApplied += PlayerStatistics_PlayerEffectApplied;
	}

	private void OnDestroy()
	{
		if (_player.Statistics != null)
		{
			_player.Statistics.PlayerEffectApplied -= PlayerStatistics_PlayerEffectApplied;
		}
	}

	private void PlayerStatistics_PlayerEffectApplied(PlayerEffect effect)
	{
		if (effect.Name == "BROKEN_BONES")
		{
			AudioManager.GetAudioPlayer().Play3D((_player.Gender == 0) ? _clips : _clipsFemale, base.transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
		}
	}
}
