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
		if ((Object)(object)_player.Statistics != (Object)null)
		{
			_player.Statistics.Damaged -= PlayerStatistics_Damaged;
		}
	}

	private void PlayerStatistics_Damaged()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		AudioClip[] clips;
		if (_player.Movement.IsUnderwater)
		{
			clips = ((_player.Gender == 0) ? _underWaterClips : _underWaterClipsFemale);
		}
		else
		{
			clips = ((_player.Gender == 0) ? _aboveWaterClips : _aboveWaterClipsFemale);
		}
		AudioManager.GetAudioPlayer().Play3D(clips, ((Component)this).transform.position, AudioMixerChannels.Voice, AudioRollOffDistance.Near);
	}
}
