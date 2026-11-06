namespace Beam.UI;

public class NullActiveMissionHudViewAdapter : ActiveMissionHudViewAdapterBase
{
	private static readonly NullActiveMissionHudViewAdapter _Instance = new NullActiveMissionHudViewAdapter();

	public static NullActiveMissionHudViewAdapter Instance => _Instance;

	public override IGameObjectViewAdapter Panel => NullObject<NullGameObjectViewAdapter>.Instance;

	public override ILabelViewAdapter MissionNameLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;

	public override ISliderViewAdapter CompletionSlider => NullViewAdapter<NullSliderViewAdapter>.Instance;

	public override IImageViewAdapter CompletionSliderFill => NullViewAdapter<NullImageViewAdapter>.Instance;
}
