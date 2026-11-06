namespace Beam;

public interface IMissionData
{
	string Name { get; }

	string Description { get; }

	IMissionRewardProvider RewardProvider { get; }

	TrackType IntroTrack { get; }

	TrackType LoopTrack { get; }

	float IntroTrackLength { get; }
}
