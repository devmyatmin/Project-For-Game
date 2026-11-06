using Beam.Events;

namespace Beam.Obfuscated.Users;

public class AddStatsEvent : IEvent
{
	private readonly int _playerId;

	private readonly Stats _statsToAdd;

	public int PlayerId => _playerId;

	public Stats StatsToAdd => _statsToAdd;

	public AddStatsEvent(int playerId, Stats statsToAdd)
	{
		_playerId = playerId;
		_statsToAdd = statsToAdd;
	}
}
