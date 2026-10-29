using Funlabs;

namespace Beam;

public interface IMultiplayerStorageReference
{
	MiniGuid ReferenceId { get; set; }

	void OnDisconnect(PlayerDisconnectedEvent @event);
}
