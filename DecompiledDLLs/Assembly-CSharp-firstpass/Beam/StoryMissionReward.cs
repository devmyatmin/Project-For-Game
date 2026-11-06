using UnityEngine;

namespace Beam;

public class StoryMissionReward : MissionRewardBase
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private int _quantity;

	public override string Name => _name;

	public override int Quantity => _quantity;

	public override Sprite GetIcon()
	{
		return null;
	}

	public override void DoReward()
	{
	}
}
