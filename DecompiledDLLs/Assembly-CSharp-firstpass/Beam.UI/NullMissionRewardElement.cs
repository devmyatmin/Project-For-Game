namespace Beam.UI;

public class NullMissionRewardElement : NullObject<NullMissionRewardElement>, IMissionRewardElement
{
	public ITransformViewAdapter Transform => NullObject<NullTransformViewAdapter>.Instance;

	public void Initialize(IMissionReward reward)
	{
	}
}
