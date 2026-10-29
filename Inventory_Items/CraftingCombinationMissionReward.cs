using Beam.Crafting;
using Beam.UI;
using UnityEngine;

namespace Beam;

public class CraftingCombinationMissionReward : MissionRewardBase
{
	[SerializeField]
	private string _craftingCombinationName;

	public override string Name => CraftingCombinations.GetCombinationTerm(_craftingCombinationName);

	public override int Quantity => 1;

	public override Sprite GetIcon()
	{
		return CraftingCombinations.GetCombinationIcon(_craftingCombinationName);
	}

	public override void DoReward()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			CraftingCombination combination = allPlayer.Crafter.CraftingCombinations.GetCombination(_craftingCombinationName);
			combination.Unlocked = true;
			LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
			localizedNotification.Priority = NotificationPriority.Immediate;
			localizedNotification.Duration = 8f;
			localizedNotification.MessageText.SetTerm("MISSIONS_MISSION_CRAFTING_COMBINATION_UNLOCK_NTF");
			localizedNotification.MessageText.SetArgs(combination.Term);
			localizedNotification.Raise();
		}
	}
}
