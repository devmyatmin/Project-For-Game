using Beam.Events;

namespace Beam;

public class QuickAccessSlotUnlockedEvent : IEvent
{
	public int PlayerId { get; }

	public int Level { get; }

	public QuickAccessSlotUnlockedEvent(int playerId, int level)
	{
		PlayerId = playerId;
		Level = level;
	}
}
