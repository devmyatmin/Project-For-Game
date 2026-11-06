namespace Beam.UI;

public class NullDisplayMissionNameViewAdapter : DisplayMissionNameViewAdapterBase
{
	private static readonly NullDisplayMissionNameViewAdapter _Instance = new NullDisplayMissionNameViewAdapter();

	public static NullDisplayMissionNameViewAdapter Instance => _Instance;

	public override IGameObjectViewAdapter Panel => NullObject<NullGameObjectViewAdapter>.Instance;

	public override ILabelViewAdapter MissionNameLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;

	public override ICanvasGroupViewAdapter MissionNameGroup => NullViewAdapter<NullCanvasGroupViewAdapter>.Instance;
}
