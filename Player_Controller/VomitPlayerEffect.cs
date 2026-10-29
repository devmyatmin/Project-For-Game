using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class VomitPlayerEffect : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _view;

	[SerializeField]
	private Player _player;

	[SerializeField]
	private AudioClip[] _clips;

	[SerializeField]
	private AudioClip[] _clipsFemale;

	private CoroutineHandle _sequence;

	private void Start()
	{
		_player.Statistics.Vomited += PlayerStatistics_Vomited;
		_view.transform.parent = _player.Character.CameraRigTransform;
		_view.transform.localPosition = Vector3.zero;
	}

	private void OnDestroy()
	{
		if (_player.Statistics != null)
		{
			_player.Statistics.Vomited -= PlayerStatistics_Vomited;
		}
	}

	private void PlayerStatistics_Vomited(float vomitTime)
	{
		Timing.KillCoroutines(_sequence);
		_sequence = Timing.RunCoroutine(Vomit_Sequence(vomitTime));
	}

	private IEnumerator<float> Vomit_Sequence(float vomitTime)
	{
		AudioManager.GetAudioPlayer().Play3D((_player.Gender == 0) ? _clips : _clipsFemale, base.transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
		_view.time = 0f;
		_view.Play(withChildren: true);
		yield return Timing.WaitForSeconds(vomitTime);
		_view.Stop();
	}
}
