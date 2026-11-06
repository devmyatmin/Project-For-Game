using Photon.Bolt;
using Photon.Bolt.Internal;

namespace Funlabs;

public class MultiplayerBehaviour : EntityEventListener<IState>
{
	public T GetMultiplayerState<T>() where T : IState
	{
		return (T)(object)((EntityEventListenerBase<IState>)(object)this).state;
	}

	public override void Attached()
	{
		MultiplayerMng.Log(((object)this).ToString() + " " + MultiplayerTokens.GetDesc(((EntityBehaviour)this).entity.AttachToken) + ", " + (((EntityBehaviour)this).entity.IsOwner ? "Owned" : "Replicant") + " Attached", "attach/detach");
	}

	public override void Detached()
	{
		MultiplayerMng.Log(((object)this).ToString() + " " + MultiplayerTokens.GetDesc(((EntityBehaviour)this).entity.AttachToken) + ", " + (((EntityBehaviour)this).entity.IsOwner ? "Owned" : "Replicant") + " Dettached", "attach/detach");
	}
}
