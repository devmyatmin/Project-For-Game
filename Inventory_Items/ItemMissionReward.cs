using Beam.Language;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class ItemMissionReward : MissionRewardBase
{
	[SerializeField]
	private InteractiveObject _reward;

	[SerializeField]
	private int _quantity = 1;

	public override string Name => _reward.DisplayName;

	public override int Quantity => _quantity;

	public override Sprite GetIcon()
	{
		return Resources.Load<Sprite>("Icons/Medium/" + _reward.CraftingType.InteractiveType);
	}

	public override void DoReward()
	{
		InteractiveObject interactiveObject = Object.Instantiate(_reward);
		for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			IPlayer player = PlayerRegistry.AllPlayers[i];
			if (player.Holder.Storage.CanPush(interactiveObject))
			{
				PlatformLocalizedText platformLocalizedText = new PlatformLocalizedText();
				platformLocalizedText.SetTerm("INVENTORY_MENU_NTF_ITEM_STORED_DESC");
				platformLocalizedText.SetArgs(interactiveObject.DisplayName);
				platformLocalizedText.SetActions(12);
				LocalizedNotification.Post(player, NotificationPriority.Immediate, 8f, platformLocalizedText, surviveSuspension: true);
				break;
			}
		}
		BeamTeam.Destroy(interactiveObject);
		if (Game.Mode.IsClient())
		{
			return;
		}
		InteractiveObject interactiveObject2 = MultiplayerMng.Instantiate<InteractiveObject>(_reward.PrefabId, MiniGuid.New());
		for (int j = 0; j < PlayerRegistry.AllPlayers.Count; j++)
		{
			IPlayer player2 = PlayerRegistry.AllPlayers[j];
			if (player2.Holder.Storage.CanPush(interactiveObject2))
			{
				player2.Holder.Storage.ReplicatedPush(interactiveObject2);
				return;
			}
		}
		interactiveObject2.transform.position = PlayerRegistry.AllPlayers.FirstOrDefault_NonAlloc().transform.position;
	}
}
