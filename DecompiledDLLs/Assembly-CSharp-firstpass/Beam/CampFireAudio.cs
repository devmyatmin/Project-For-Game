using System.Collections.Generic;
using Beam.Crafting;
using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class CampFireAudio : InteractiveAudioHelper
{
	[SerializeField]
	private List<InteractiveAudioCombos.AudioTypeCombo> _refuelClips;

	[SerializeField]
	private AudioClip[] _attachedClips;

	[SerializeField]
	private AudioClip _ignitedClip;

	[SerializeField]
	private AudioClip _burningClip;

	[SerializeField]
	private AudioClip _cookingClip;

	private AudioActorHandle _burningAudioHandle;

	private AudioActorHandle _cookingAudioHandle;

	private void OnDestroy()
	{
		Stop();
	}

	public void CampFire_Ignited(IBase sender, IBaseActionEventData data)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		PlaySingle3D(_ignitedClip);
		_burningAudioHandle = AudioManager.GetAudioPlayer().Play3D(_burningClip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near, AudioPlayMode.Persistent);
	}

	public void CampFire_Extinguished(IBase sender, IBaseActionEventData data)
	{
		PlaySingle3D(_ignitedClip);
		Stop();
	}

	public void CampFire_StartedCooking(IBase sender, IBaseActionEventData data)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_cookingAudioHandle = AudioManager.GetAudioPlayer().Play3D(_cookingClip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear, AudioPlayMode.Persistent);
	}

	public void CampFire_StoppedCooking(IBase sender, IBaseActionEventData data)
	{
		AudioManager.GetAudioPlayer().Stop(_cookingAudioHandle);
		_cookingAudioHandle = AudioActorHandle.Empty;
	}

	public void CampFire_Refuelled(IBase sender, IBaseActionEventData data)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		InteractiveAudioCombos.AudioTypeCombo audioTypeCombo = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_refuelClips, (InteractiveAudioCombos.AudioTypeCombo cmb, CraftingType typ) => cmb.Type == typ, data.OtherType);
		if (audioTypeCombo == null)
		{
			audioTypeCombo = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_refuelClips);
		}
		AudioManager.GetAudioPlayer().Play3D(audioTypeCombo?.AudioClips, data.Position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}

	public void CampFire_Attached(IBase sender, IBaseActionEventData data)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.GetAudioPlayer().Play3D(_attachedClips, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}

	private void Stop()
	{
		AudioManager.GetAudioPlayer().Stop(_burningAudioHandle);
		_burningAudioHandle = AudioActorHandle.Empty;
		AudioManager.GetAudioPlayer().Stop(_cookingAudioHandle);
		_cookingAudioHandle = AudioActorHandle.Empty;
	}
}
