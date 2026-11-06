namespace Beam.Events;

public class RewiredControllerChangedEvent : IEvent
{
	public readonly int Index;

	public RewiredControllerChangedEvent(int index)
	{
		Index = index;
	}
}
