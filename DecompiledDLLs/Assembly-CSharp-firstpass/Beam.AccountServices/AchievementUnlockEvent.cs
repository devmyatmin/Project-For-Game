using Beam.Events;

namespace Beam.AccountServices;

public class AchievementUnlockEvent : IEvent
{
	private readonly EAchievement _achievement;

	public EAchievement Achievement => _achievement;

	public AchievementUnlockEvent(EAchievement achievement)
	{
		_achievement = achievement;
	}
}
