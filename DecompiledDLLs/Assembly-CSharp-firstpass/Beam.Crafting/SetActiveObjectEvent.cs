using Beam.Events;

namespace Beam.Crafting;

public class SetActiveObjectEvent : IEvent
{
	public IBase ActiveObject;

	public SetActiveObjectEvent(IBase activeObject)
	{
		ActiveObject = activeObject;
	}
}
