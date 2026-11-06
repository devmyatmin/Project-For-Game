using UnityEngine;

namespace Beam;

public class AnimalAudioBase<TAnimal> : MonoBehaviour where TAnimal : LandAnimal
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private TAnimal _animal;

	[SerializeField]
	private AudioClip _ambentAudioCliip;

	[SerializeField]
	private AudioClip[] _attackedAudioClips;

	[SerializeField]
	private AudioClip[] _killedAudioClips;

	[SerializeField]
	private AudioClip[] _damagedAudioClips;

	private AudioActorHandle _ambientHandle;

	protected TAnimal Animal => _animal;

	protected virtual void Start()
	{
		if ((Object)(object)_animal != (Object)null && (LandAnimalState)_animal.CurrentState != LandAnimalState.Dying)
		{
			_animal.Attacked += Animal_Attacked;
			_animal.Killed += Animal_Killed;
			_ambientHandle = AudioManager.GetAudioPlayer().Play3D(_ambentAudioCliip, ((Component)this).transform, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear, AudioPlayMode.Persistent);
		}
	}

	protected virtual void OnDestroy()
	{
		if ((Object)(object)_animal != (Object)null)
		{
			_animal.Attacked -= Animal_Attacked;
			_animal.Killed -= Animal_Killed;
		}
		Stop();
	}

	private void Animal_Attacked()
	{
		PlaySingle3D(_attackedAudioClips);
	}

	private void Animal_Killed()
	{
		PlaySingle3D(_killedAudioClips);
		Stop();
	}

	public void Animal_Damaged()
	{
		PlaySingle3D(_damagedAudioClips);
	}

	private void PlaySingle3D(AudioClip[] clips)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(clips, ((Component)this).transform.position, AudioMixerChannels.FX);
	}

	private void Stop()
	{
		AudioManager.GetAudioPlayer().Stop(_ambientHandle);
		_ambientHandle = AudioActorHandle.Empty;
	}
}
