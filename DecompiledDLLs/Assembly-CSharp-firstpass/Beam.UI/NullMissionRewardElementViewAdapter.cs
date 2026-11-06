namespace Beam.UI;

public class NullMissionRewardElementViewAdapter : MissionRewardElementViewAdapterBase
{
	private static readonly NullMissionRewardElementViewAdapter _Instance = new NullMissionRewardElementViewAdapter();

	private readonly NullLabelViewAdapter _rewardQuantityLabel = NullViewAdapter<NullLabelViewAdapter>.Instance;

	private readonly NullLabelViewAdapter _rewardLabel = NullViewAdapter<NullLabelViewAdapter>.Instance;

	private readonly NullImageViewAdapter _rewardImage = NullViewAdapter<NullImageViewAdapter>.Instance;

	public static NullMissionRewardElementViewAdapter Instance => _Instance;

	public override ILabelViewAdapter RewardQuantityLabel => _rewardQuantityLabel;

	public override ILabelViewAdapter RewardLabel => _rewardLabel;

	public override IImageViewAdapter RewardImage => _rewardImage;
}
