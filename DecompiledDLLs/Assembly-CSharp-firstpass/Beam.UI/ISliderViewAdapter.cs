namespace Beam.UI;

public interface ISliderViewAdapter : IViewAdapter
{
	string Text { get; set; }

	string ValueText { get; set; }

	float Value { get; set; }

	event FloatEventHandler ValueChanged;
}
