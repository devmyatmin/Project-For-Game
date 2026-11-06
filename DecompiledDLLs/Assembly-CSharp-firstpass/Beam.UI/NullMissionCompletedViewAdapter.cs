namespace Beam.UI;

public class NullMissionCompletedViewAdapter : MissionCompletedViewAdapterBase
{
	public static NullMissionCompletedViewAdapter Instance { get; } = new NullMissionCompletedViewAdapter();

	public override IGameObjectViewAdapter Panel => NullObject<NullGameObjectViewAdapter>.Instance;

	public override IMissionRewardElement CreateRewardElement()
	{
		return NullObject<NullMissionRewardElement>.Instance;
	}

	public override ActionHudElementViewAdapterBase CreateActionLabel()
	{
		return NullActionHudElementViewAdapter.Instance;
	}

	public override void DestroyRewardElements()
	{
	}

	public override void DestroyActionLabels()
	{
	}
}
