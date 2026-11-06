using UnityEngine;

namespace Beam.UI;

public class TMPMissionRewardElementViewAdapter : MissionRewardElementViewAdapterBase
{
	[SerializeField]
	private TMPLabelViewAdapter _rewardQuantityLabel;

	[SerializeField]
	private TMPLabelViewAdapter _rewardLabel;

	[SerializeField]
	private UImageViewAdapter _rewardImage;

	public override ILabelViewAdapter RewardQuantityLabel => _rewardQuantityLabel;

	public override ILabelViewAdapter RewardLabel => _rewardLabel;

	public override IImageViewAdapter RewardImage => _rewardImage;
}
