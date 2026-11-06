namespace Beam.UI;

public interface IInputFieldViewAdapter : IViewAdapter
{
	string Text { get; set; }

	event InputFieldEventHandler ValueChanged;

	event InputFieldEventHandler KeyDown;

	void Select();
}
