using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class IllnessPlayerEffect : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private AudioClip[] _coughingClips;

	[SerializeField]
	private AudioClip[] _coughingClipsFemale;

	private CoroutineHandle _sequence;

	private void Start()
	{
		_player.Statistics.PlayerEffectApplied += PlayerStatistics_PlayerEffectApplied;
		_player.Statistics.PlayerEffectRemoved += PlayerStatistics_PlayerEffectRemoved;
	}

	private void OnDestroy()
	{
		if ((Object)(object)_player.Statistics != (Object)null)
		{
			_player.Statistics.PlayerEffectApplied -= PlayerStatistics_PlayerEffectApplied;
			_player.Statistics.PlayerEffectRemoved -= PlayerStatistics_PlayerEffectRemoved;
		}
	}

	private void PlayerStatistics_PlayerEffectApplied(PlayerEffect effect)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (effect.Name == "ILLNESS")
		{
			Timing.KillCoroutines(_sequence);
			_sequence = Timing.RunCoroutine(Illness_Sequence());
		}
	}

	private void PlayerStatistics_PlayerEffectRemoved(PlayerEffect effect)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (effect.Name == "ILLNESS")
		{
			Timing.KillCoroutines(_sequence);
		}
	}

	private IEnumerator<float> Illness_Sequence()
	{
		float coughTimer = 0f;
		float lastCough = 0f;
		while (true)
		{
			lastCough += Time.deltaTime;
			if (lastCough >= coughTimer && !_player.Movement.IsUnderwater)
			{
				AudioManager.GetAudioPlayer().Play3D((_player.Gender == 0) ? _coughingClips : _coughingClipsFemale, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
				lastCough = 0f;
				coughTimer = Random.Range(60, 300);
			}
			yield return 0f;
		}
	}
}
