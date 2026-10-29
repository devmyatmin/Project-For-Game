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
		if (_player.Statistics != null)
		{
			_player.Statistics.Pooped -= PlayerStatistics_Pooped;
		}
	}

	private void PlayerStatistics_Pooped(float poopTime)
	{
		Timing.KillCoroutines(_sequence);
		_sequence = Timing.RunCoroutine(Poop_Sequence(poopTime));
	}

	private IEnumerator<float> Poop_Sequence(float poopTime)
	{
		_player.Movement.crouched = true;
		AudioManager.GetAudioPlayer().Play3D(_clips, base.transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
		yield return Timing.WaitForSeconds(poopTime);
	}
}
