namespace Beam.UI.MapEditor;

public interface IMapEditorMenuViewAdapter : IViewAdapter
{
	ICanvasGroupViewAdapter CanvasGroup { get; }

	ILabelButtonViewAdapter ResumeButton { get; }

	ILabelButtonViewAdapter SaveButton { get; }

	ILabelButtonViewAdapter QuitButton { get; }

	ILabelViewAdapter MapTitleLabel { get; }
}
