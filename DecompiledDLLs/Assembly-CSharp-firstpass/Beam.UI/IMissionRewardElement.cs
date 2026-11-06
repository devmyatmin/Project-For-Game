namespace Beam.UI;

public interface IMissionRewardElement
{
	ITransformViewAdapter Transform { get; }

	void Initialize(IMissionReward reward);
}
