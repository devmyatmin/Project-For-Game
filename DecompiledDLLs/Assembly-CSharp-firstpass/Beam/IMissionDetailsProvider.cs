namespace Beam;

public interface IMissionDetailsProvider
{
	IMissionData MissionData { get; }

	IMissionGoal MissionGoal { get; }
}
