using UnityEngine;

namespace Beam.Crafting;

public class Interacting_FISHING_BOBBER : ProjectileObject
{
	public FishSize BobberSize;

	public Interacting_FISHINGROD AttachedFishingRod { get; set; }

	public bool CanCollect { get; set; }

	public override bool CanPickUp
	{
		get
		{
			if (AttachedFishingRod != null)
			{
				if (AttachedFishingRod.ActiveBobber != this || AttachedFishingRod.OwnerId == -1)
				{
					AttachedFishingRod = null;
				}
				else if (!CanCollect)
				{
					return false;
				}
				CanCollect = false;
			}
			return base.CanPickUp;
		}
		set
		{
			base.CanPickUp = value;
		}
	}

	public void Bob(Vector3 force)
	{
		base.rigidbody.AddForce(force, ForceMode.VelocityChange);
	}
}
