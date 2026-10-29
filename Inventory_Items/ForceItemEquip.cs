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
		IPlayer player = PlayerUtilities.FindClosestPlayer(base.transform.position, 3.3000002f);
		if (!player.IsNullOrDestroyed())
		{
			IPickupable playerItem = GetPlayerItem(player, _craftingTypeProvider.GetCraftingType());
			if (!playerItem.IsNullOrDestroyed())
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
