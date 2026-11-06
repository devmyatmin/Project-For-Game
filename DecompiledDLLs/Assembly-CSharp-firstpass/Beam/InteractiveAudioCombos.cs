using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beam;

public class InteractiveAudioCombos : InteractiveAudioHelper
{
	[Serializable]
	public class AudioTypeCombo
	{
		[SerializeField]
		private CraftingType _type;

		[SerializeField]
		private AudioClip[] _audioClips;

		public CraftingType Type => _type;

		public AudioClip[] AudioClips => _audioClips;
	}

	[Header("COMBINATION AUDIO CLIP RANGES")]
	[FormerlySerializedAs("_comboClipsTemp")]
	[SerializeField]
	private List<AudioTypeCombo> _combinationClips;

	public override void PlayRange(IBase sender, IBaseActionEventData data)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		base.PlayRange(sender, data);
		AudioTypeCombo audioTypeCombo = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_combinationClips, (AudioTypeCombo cmb, CraftingType typ) => cmb.Type == typ, data.OtherType);
		AudioManager.GetAudioPlayer().Play3D(audioTypeCombo?.AudioClips, data.Position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}
}
