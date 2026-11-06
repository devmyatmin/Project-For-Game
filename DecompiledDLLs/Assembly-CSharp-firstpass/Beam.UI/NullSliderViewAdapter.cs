namespace Beam.UI;

public class NullSliderViewAdapter : NullViewAdapter<NullSliderViewAdapter>, ISliderViewAdapter, IViewAdapter
{
	public string Text { get; set; }

	public string ValueText { get; set; }

	public float Value { get; set; }

	event FloatEventHandler ISliderViewAdapter.ValueChanged
	{
		add
		{
		}
		remove
		{
		}
	}
}
