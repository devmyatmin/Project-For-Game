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
		if ((Object)(object)_player.Statistics != (Object)null)
		{
			_player.Statistics.PlayerEffectApplied -= PlayerStatistics_PlayerEffectApplied;
		}
	}

	private void PlayerStatistics_PlayerEffectApplied(PlayerEffect effect)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (effect.Name == "BROKEN_BONES")
		{
			AudioManager.GetAudioPlayer().Play3D((_player.Gender == 0) ? _clips : _clipsFemale, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
		}
	}
}
