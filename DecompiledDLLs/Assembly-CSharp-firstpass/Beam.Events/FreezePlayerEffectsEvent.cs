namespace Beam.Events;

public class FreezePlayerEffectsEvent : IEvent
{
	public readonly bool Freeze;

	public FreezePlayerEffectsEvent(bool freeze)
	{
		Freeze = freeze;
	}
}
