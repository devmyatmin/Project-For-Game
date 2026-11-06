using System;
using Beam.Language;
using UnityEngine;

namespace Beam;

[Serializable]
public class MissionData : IMissionData
{
	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _name;

	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _description;

	[SerializeField]
	private MissionRewardProvider _rewardProvider;

	[SerializeField]
	private TrackType _introTrack;

	[SerializeField]
	private TrackType _loopTrack;

	[SerializeField]
	private float _introTrackLength;

	public string Name => _name;

	public string Description => _description;

	public IMissionRewardProvider RewardProvider => _rewardProvider;

	public TrackType IntroTrack => _introTrack;

	public TrackType LoopTrack => _loopTrack;

	public float IntroTrackLength => _introTrackLength;
}
