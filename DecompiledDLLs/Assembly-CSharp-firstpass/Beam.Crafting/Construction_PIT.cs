using Beam.UI;
using Beam.Utilities;

namespace Beam.Crafting;

public class Construction_PIT : Constructing
{
	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (BeamTeam.IsNullOrDestroyed(pickupable))
		{
			return;
		}
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
		if (BeamTeam.IsNullOrDestroyed(playerFromId))
		{
			return;
		}
		for (int i = 0; i < MyConnectors.Length; i++)
		{
			if (!MyConnectors[i].IsValid)
			{
				LocalizedNotification.Post(playerFromId, NotificationPriority.Low, 4f, "CONSTRUCTING_NTF_CANT_DESTROY_ANOTHER_DEPENDS_DESC");
				return;
			}
		}
		base.Damage(damage, damager);
	}
}
