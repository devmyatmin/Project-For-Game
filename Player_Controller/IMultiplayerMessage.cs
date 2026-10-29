using Photon.Bolt;

namespace Funlabs;

public interface IMultiplayerMessage : IProtocolToken
{
	void OnPeer();
}
