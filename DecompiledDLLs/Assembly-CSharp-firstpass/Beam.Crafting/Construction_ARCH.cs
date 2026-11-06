using Beam.UI;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class Construction_ARCH : Construction_PANEL, IHaveAttribute
{
	[SerializeField]
	private Connector _doorConnector;

	[SerializeField]
	private Collider _wallCollider;

	public override void Place()
	{
		_wallCollider.enabled = true;
		base.Place();
		_wallCollider.enabled = false;
	}

	public override void Destroy()
	{
		_wallCollider.enabled = true;
		base.Destroy();
	}

	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (BeamTeam.IsNullOrDestroyed(pickupable))
		{
			return;
		}
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
		if (!BeamTeam.IsNullOrDestroyed(playerFromId))
		{
			if (!_doorConnector.IsValid)
			{
				LocalizedNotification.Post(playerFromId, NotificationPriority.Low, 4f, "CONSTRUCTING_NTF_CANT_DESTROY_SUPPORTS_DOOR_DESC");
			}
			else
			{
				base.Damage(damage, damager);
			}
		}
	}
}
