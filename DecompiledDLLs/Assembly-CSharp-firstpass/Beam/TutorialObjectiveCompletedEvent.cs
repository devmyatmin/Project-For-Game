using Beam.Events;

namespace Beam;

public class TutorialObjectiveCompletedEvent : IEvent
{
	public readonly int Id;

	public TutorialObjectiveCompletedEvent(int id)
	{
		Id = id;
	}
}
