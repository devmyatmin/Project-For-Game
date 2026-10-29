namespace Beam.UI;

public abstract class PlayerSpeechViewAdapterBase : ViewAdapterBase
{
	public abstract ICanvasGroupViewAdapter CanvasGroup { get; }

	public abstract ILabelViewAdapter SubtitlesLabel { get; }
}
