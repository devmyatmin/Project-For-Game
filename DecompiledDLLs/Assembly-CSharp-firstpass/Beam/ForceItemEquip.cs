using Beam.Crafting;
using Beam.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Beam;

public class ForceItemEquip : SerializedMonoBehaviour
{
	[SerializeField]
	private ICraftingTypeProvider _craftingTypeProvider;

	public void DoForceEquipItem()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		IPlayer player = PlayerUtilities.FindClosestPlayer(((Component)this).transform.position, 3.3000002f);
		if (!BeamTeam.IsNullOrDestroyed(player))
		{
			IPickupable playerItem = GetPlayerItem(player, _craftingTypeProvider.GetCraftingType());
			if (!BeamTeam.IsNullOrDestroyed(playerItem))
			{
				player.Holder.ReplicatedSelect(playerItem);
			}
		}
	}

	private IPickupable GetPlayerItem(IPlayer player, CraftingType type)
	{
		IPickupable result = null;
		if (!(player.Holder.CurrentObject?.CraftingType == type))
		{
			result = player.Holder.Storage.Get<IPickupable>(type);
		}
		return result;
	}
}
