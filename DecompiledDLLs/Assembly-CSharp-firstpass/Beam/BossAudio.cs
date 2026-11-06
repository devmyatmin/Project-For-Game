using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class BossAudio : MonoBehaviour
{
	[SerializeField]
	private Piscus_Creature _swimCreature;

	[SerializeField]
	private InteractiveObject_GOAL _goal;

	[SerializeField]
	private AudioClip _ambientClip;

	[SerializeField]
	private AudioClip _awakenedClip;

	[SerializeField]
	private AudioClip _deathClip;

	[SerializeField]
	private AudioClip[] _attackedClips;

	[SerializeField]
	private AudioClip[] _jumpedClips;

	[SerializeField]
	private AudioClip[] _landedClips;

	[SerializeField]
	private AudioClip[] _damagedClips;

	private AudioActorHandle _ambientHandle;

	private void Start()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected Obj, but got Unknown
		if ((Object)(object)_swimCreature != (Object)null && (Object)(object)_goal != (Object)null)
		{
			_swimCreature.Awakened += Boss_Awakened;
			_swimCreature.Attacked += Boss_Attacked;
			_swimCreature.Jumped += Boss_Jumped;
			_swimCreature.Landed += Boss_Landed;
			((UnityEvent<IBase, IBaseActionEventData>)_goal.Damaged).AddListener((UnityAction<IBase, IBaseActionEventData>)Boss_Damaged);
			_goal.Destroyed.AddListener((UnityAction)Boss_Destroyed);
			_ambientHandle = AudioManager.GetAudioPlayer().Play3D(_ambientClip, ((Component)this).transform, AudioMixerChannels.FX, AudioRollOffDistance.Far, AudioPlayMode.Persistent);
		}
	}

	private void OnDestroy()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected Obj, but got Unknown
		if ((Object)(object)_swimCreature != (Object)null)
		{
			_swimCreature.Awakened += Boss_Awakened;
			_swimCreature.Attacked += Boss_Attacked;
			_swimCreature.Jumped += Boss_Jumped;
			_swimCreature.Landed += Boss_Landed;
		}
		if ((Object)(object)_goal != (Object)null)
		{
			((UnityEvent<IBase, IBaseActionEventData>)_goal.Damaged).RemoveListener((UnityAction<IBase, IBaseActionEventData>)Boss_Damaged);
			_goal.Destroyed.RemoveListener((UnityAction)Boss_Destroyed);
		}
		AudioManager.GetAudioPlayer().Stop(_ambientHandle);
		_ambientHandle = AudioActorHandle.Empty;
	}

	public void Boss_Awakened()
	{
		PlaySingle3D(_awakenedClip);
	}

	public void Boss_Attacked()
	{
		Play3D(_attackedClips);
	}

	public void Boss_Jumped()
	{
		Play3D(_jumpedClips);
	}

	public void Boss_Landed()
	{
		Play3D(_landedClips);
	}

	public void Boss_Damaged(IBase sender, IBaseActionEventData data)
	{
		Play3D(_damagedClips);
	}

	public void Boss_Destroyed()
	{
		PlaySingle3D(_deathClip);
	}

	public void PlaySingle3D(AudioClip clip)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(clip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryFar);
	}

	private void Play3D(AudioClip[] clips)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(clips, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryFar);
	}
}
