using Funlabs;

namespace Beam.Crafting;

public class Constructing_LIFERAFT_HOOK : Constructing_HOOK
{
	protected override void Awake()
	{
		ReferenceId = MiniGuid.PADDLE_RAFT;
		base.Awake();
	}
}
