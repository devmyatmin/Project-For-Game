namespace Beam.UI;

public interface IInputFieldButtonViewAdapter : IButtonViewAdapter, ISelectableViewAdapter, IViewAdapter
{
	InputFieldButtonType Type { get; set; }

	InputFieldButtonInfo Info { get; set; }

	string Text { get; set; }
}
