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
			if ((Object)(object)AttachedFishingRod != (Object)null)
			{
				if ((Object)(object)AttachedFishingRod.ActiveBobber != (Object)(object)this || AttachedFishingRod.OwnerId == -1)
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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		rigidbody.AddForce(force, (ForceMode)2);
	}
}
