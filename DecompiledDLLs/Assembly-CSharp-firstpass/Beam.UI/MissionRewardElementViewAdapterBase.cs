namespace Beam.UI;

public abstract class MissionRewardElementViewAdapterBase : ViewAdapterBase
{
	public abstract ILabelViewAdapter RewardQuantityLabel { get; }

	public abstract ILabelViewAdapter RewardLabel { get; }

	public abstract IImageViewAdapter RewardImage { get; }
}
