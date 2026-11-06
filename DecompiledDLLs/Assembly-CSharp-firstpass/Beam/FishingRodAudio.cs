using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class FishingRodAudio : MonoBehaviour
{
	[SerializeField]
	[PREFAB_PROCESSOR_GetComponent]
	private Interacting_FISHINGROD _fishingRod;

	[SerializeField]
	private AudioClip _chargeClip;

	[SerializeField]
	private AudioClip _reelingInClip;

	[SerializeField]
	private AudioClip _reelingOutClip;

	[SerializeField]
	private AudioClip _tensionClip;

	[SerializeField]
	private AudioClip _lineSnappedClip;

	[SerializeField]
	private AudioClip _fishCaughtClip;

	[SerializeField]
	private AudioClip _fishEscapedClip;

	private IAudioPlayer _audioPlayer;

	private AudioActorHandle _reelAudioHandle;

	private AudioActorHandle _tensionAudioHandle;

	private CoroutineHandle _reelSequence;

	private float _reelAudioPitch;

	private void Start()
	{
		if ((Object)(object)_fishingRod != (Object)null)
		{
			_audioPlayer = AudioManager.GetAudioPlayer();
			((UnityEvent<IBase, IBaseActionEventData>)_fishingRod.Charged).AddListener((UnityAction<IBase, IBaseActionEventData>)FishingRod_Charged);
			((UnityEvent<IBase, IBaseActionEventData>)_fishingRod.UsedSecondary).AddListener((UnityAction<IBase, IBaseActionEventData>)FishingRod_UsedSecondary);
			_fishingRod.LineSnapped += FishingRod_LineSnapped;
			_fishingRod.FishCaught += FishingRod_FishCaught;
			_fishingRod.FishEscaped += FishingRod_FishEscaped;
			_fishingRod.BobberReset += FishingRod_BobberReset;
		}
	}

	private void OnDestroy()
	{
		Stop();
		if ((Object)(object)_fishingRod != (Object)null)
		{
			((UnityEvent<IBase, IBaseActionEventData>)_fishingRod.Charged).RemoveListener((UnityAction<IBase, IBaseActionEventData>)FishingRod_Charged);
			((UnityEvent<IBase, IBaseActionEventData>)_fishingRod.UsedSecondary).RemoveListener((UnityAction<IBase, IBaseActionEventData>)FishingRod_UsedSecondary);
			_fishingRod.LineSnapped -= FishingRod_LineSnapped;
			_fishingRod.FishCaught -= FishingRod_FishCaught;
			_fishingRod.FishEscaped -= FishingRod_FishEscaped;
			_fishingRod.BobberReset -= FishingRod_BobberReset;
		}
	}

	private void FishingRod_Charged(IBase sender, IBaseActionEventData data)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_chargeClip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear);
	}

	private void FishingRod_UsedSecondary(IBase sender, IBaseActionEventData data)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_reelSequence = Timing.RunCoroutine(Reel_Sequence());
	}

	private void FishingRod_LineSnapped()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_lineSnappedClip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear);
	}

	private void FishingRod_FishCaught()
	{
		_audioPlayer.Play2D(_fishCaughtClip, AudioMixerChannels.FX);
	}

	private void FishingRod_FishEscaped()
	{
		_audioPlayer.Play2D(_fishEscapedClip, AudioMixerChannels.FX);
	}

	private void FishingRod_BobberReset()
	{
		Stop();
	}

	private void Stop()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_reelSequence);
		_audioPlayer.Stop(_reelAudioHandle);
		_reelAudioHandle = AudioActorHandle.Empty;
		_audioPlayer.Stop(_tensionAudioHandle);
		_tensionAudioHandle = AudioActorHandle.Empty;
	}

	private IEnumerator<float> Reel_Sequence()
	{
		_reelAudioHandle = _audioPlayer.Play3D(_reelingOutClip, ((Component)this).transform, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear, AudioPlayMode.Persistent);
		_tensionAudioHandle = _audioPlayer.Play3D(_tensionClip, ((Component)this).transform, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear, AudioPlayMode.Persistent);
		IAudioSource reelAudioSource = _audioPlayer.GetAudioSource(_reelAudioHandle);
		IAudioSource tensionAudioSource = _audioPlayer.GetAudioSource(_tensionAudioHandle);
		tensionAudioSource.Volume = 0f;
		while (_fishingRod.Casting)
		{
			_reelAudioPitch = 1.5f;
			reelAudioSource.Pitch = Mathf.Lerp(reelAudioSource.Pitch, _reelAudioPitch, Time.deltaTime * 30f);
			yield return 0f;
		}
		while (true)
		{
			_reelAudioPitch = 1f;
			if (_fishingRod.ReelingIn)
			{
				if (!_fishingRod.FishPulling)
				{
					if (_fishingRod.LineTension > 0.7f)
					{
						_reelAudioPitch = 1f - _fishingRod.LineTension;
					}
				}
				else
				{
					_reelAudioPitch = 1f - _fishingRod.LineTension;
				}
			}
			else if (_fishingRod.ReelingOut)
			{
				_reelAudioPitch = Mathf.Clamp(_fishingRod.BobberMovingSpeed * 0.5f, 0.3f, 1f);
			}
			else
			{
				_reelAudioPitch = 0.3f;
			}
			reelAudioSource.Pitch = Mathf.Lerp(reelAudioSource.Pitch, _reelAudioPitch, Time.deltaTime * 15f);
			reelAudioSource.Volume = ((_fishingRod.ReelingIn || _fishingRod.ReelingOut) ? 1f : 0f);
			tensionAudioSource.Volume = (_fishingRod.HasFish ? _fishingRod.LineTension : 0f);
			yield return 0f;
		}
	}
}
