namespace Beam.UI.MapEditor;

public interface IMapEditorSaveMenuViewAdapter : IViewAdapter
{
	ICanvasGroupViewAdapter CanvasGroup { get; }

	ILabelButtonViewAdapter SaveButton { get; }

	ILabelButtonViewAdapter BackButton { get; }

	IInputFieldViewAdapter NameInputField { get; }

	IInputFieldViewAdapter DescriptionInputField { get; }

	IImageViewAdapter PreviewImage { get; }
}
