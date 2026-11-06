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
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		_player.Statistics.Vomited += PlayerStatistics_Vomited;
		((Component)_view).transform.parent = _player.Character.CameraRigTransform;
		((Component)_view).transform.localPosition = Vector3.zero;
	}

	private void OnDestroy()
	{
		if ((Object)(object)_player.Statistics != (Object)null)
		{
			_player.Statistics.Vomited -= PlayerStatistics_Vomited;
		}
	}

	private void PlayerStatistics_Vomited(float vomitTime)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_sequence);
		_sequence = Timing.RunCoroutine(Vomit_Sequence(vomitTime));
	}

	private IEnumerator<float> Vomit_Sequence(float vomitTime)
	{
		AudioManager.GetAudioPlayer().Play3D((_player.Gender == 0) ? _clips : _clipsFemale, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
		_view.time = 0f;
		_view.Play(true);
		yield return Timing.WaitForSeconds(vomitTime);
		_view.Stop();
	}
}
