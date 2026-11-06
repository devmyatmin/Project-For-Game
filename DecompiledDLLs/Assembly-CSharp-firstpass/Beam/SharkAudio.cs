using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class SharkAudio : MonoBehaviour
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Piscus_Creature _piscus;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private InteractiveObject _shark;

	[SerializeField]
	private AudioClip _ambientClip;

	[SerializeField]
	private AudioClip _attackedRaftAudioClip;

	[SerializeField]
	private AudioClip[] _attackedAudioClips;

	[SerializeField]
	private AudioClip[] _damagedAudioClips;

	private AudioActorHandle _ambientHandle;

	private void Start()
	{
		if ((Object)(object)_piscus != (Object)null && (Object)(object)_shark != (Object)null)
		{
			_piscus.Attacked += Shark_Attacked;
			_piscus.AttackedRaft += Shark_AttackedRaft;
			((UnityEvent<IBase, IBaseActionEventData>)_shark.Damaged).AddListener((UnityAction<IBase, IBaseActionEventData>)Shark_Damaged);
			_ambientHandle = AudioManager.GetAudioPlayer().Play3D(_ambientClip, ((Component)this).transform, AudioMixerChannels.FX, AudioRollOffDistance.Far, AudioPlayMode.Persistent);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)_piscus != (Object)null)
		{
			_piscus.Attacked -= Shark_Attacked;
			_piscus.AttackedRaft -= Shark_AttackedRaft;
		}
		if ((Object)(object)_shark != (Object)null)
		{
			((UnityEvent<IBase, IBaseActionEventData>)_shark.Damaged).RemoveListener((UnityAction<IBase, IBaseActionEventData>)Shark_Damaged);
		}
		AudioManager.GetAudioPlayer().Stop(_ambientHandle);
		_ambientHandle = AudioActorHandle.Empty;
	}

	public void Shark_Attacked()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(_attackedAudioClips, ((Component)this).transform.position, AudioMixerChannels.FX);
	}

	public void Shark_AttackedRaft()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(_attackedRaftAudioClip, ((Component)this).transform.position, AudioMixerChannels.FX);
	}

	public void Shark_Damaged(IBase sender, IBaseActionEventData data)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(_damagedAudioClips, ((Component)this).transform.position, AudioMixerChannels.FX);
	}
}
