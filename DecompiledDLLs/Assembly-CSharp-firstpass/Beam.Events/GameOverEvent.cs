using System.Collections.Generic;

namespace Beam.Events;

public class GameOverEvent : IEvent
{
	public readonly IList<PlayerDeathEventData> Causes;

	public GameOverEvent(IList<PlayerDeathEventData> causes)
	{
		Causes = causes;
	}
}
