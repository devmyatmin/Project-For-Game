using UnityEngine;

public class RaftPieceGhostCollisionModifier : GhostCollisionModifierBase
{
	public override bool ValidateCollision(Collider other)
	{
		if (((Component)this).gameObject.layer == Layers.CONSTRUCTIONS_RAFTS)
		{
			return ((Component)other).gameObject.layer != Layers.CONSTRUCTIONS_RAFTS;
		}
		return true;
	}
}
