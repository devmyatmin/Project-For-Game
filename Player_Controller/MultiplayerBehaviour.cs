using Photon.Bolt;

namespace Funlabs;

public class MultiplayerBehaviour : EntityEventListener<IState>
{
	public T GetMultiplayerState<T>() where T : IState
	{
		return (T)base.state;
	}

	public override void Attached()
	{
		MultiplayerMng.Log(ToString() + " " + MultiplayerTokens.GetDesc(base.entity.AttachToken) + ", " + (base.entity.IsOwner ? "Owned" : "Replicant") + " Attached", "attach/detach");
	}

	public override void Detached()
	{
		MultiplayerMng.Log(ToString() + " " + MultiplayerTokens.GetDesc(base.entity.AttachToken) + ", " + (base.entity.IsOwner ? "Owned" : "Replicant") + " Dettached", "attach/detach");
	}
}
