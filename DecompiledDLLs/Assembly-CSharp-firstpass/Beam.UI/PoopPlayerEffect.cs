using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class PoopPlayerEffect : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private AudioClip[] _clips;

	[SerializeField]
	private GameObject _poopPrefab;

	private CoroutineHandle _sequence;

	private void Start()
	{
		_player.Statistics.Pooped += PlayerStatistics_Pooped;
	}

	private void OnDestroy()
	{
		if ((Object)(object)_player.Statistics != (Object)null)
		{
			_player.Statistics.Pooped -= PlayerStatistics_Pooped;
		}
	}

	private void PlayerStatistics_Pooped(float poopTime)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_sequence);
		_sequence = Timing.RunCoroutine(Poop_Sequence(poopTime));
	}

	private IEnumerator<float> Poop_Sequence(float poopTime)
	{
		_player.Movement.crouched = true;
		AudioManager.GetAudioPlayer().Play3D(_clips, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
		yield return Timing.WaitForSeconds(poopTime);
	}
}
