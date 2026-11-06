namespace Beam.UI;

public class NullPlayerSpeechViewAdapter : PlayerSpeechViewAdapterBase
{
	public static NullPlayerSpeechViewAdapter Instance { get; } = new NullPlayerSpeechViewAdapter();

	public override ICanvasGroupViewAdapter CanvasGroup => NullViewAdapter<NullCanvasGroupViewAdapter>.Instance;

	public override ILabelViewAdapter SubtitlesLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;
}
