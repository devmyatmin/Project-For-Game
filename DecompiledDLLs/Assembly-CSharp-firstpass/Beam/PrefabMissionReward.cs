using Beam.UI;
using Funlabs;
using UnityEngine;

namespace Beam;

public class PrefabMissionReward : MissionRewardBase
{
	[SerializeField]
	private BaseObject _rewardPrefab;

	[SerializeField]
	private int _quantity;

	public override string Name => _rewardPrefab.DisplayName;

	public override int Quantity => _quantity;

	public override Sprite GetIcon()
	{
		return Icons.GetSmallIcon(new IconKey
		{
			CraftingType = _rewardPrefab.CraftingType
		});
	}

	public override void DoReward()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsClient(Game.Mode))
		{
			MultiplayerMng.Instantiate<SaveablePrefab>(_rewardPrefab.PrefabId, MiniGuid.New());
		}
	}
}
