using Photon.Bolt;

namespace Funlabs;

public interface IMultiplayerState<T> where T : IState
{
	T MultiplayerState { get; }
}
