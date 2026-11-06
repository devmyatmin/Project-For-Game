namespace Beam.UI;

public interface IOptionWidgetViewAdapter : ISelectableViewAdapter, IViewAdapter
{
	string Text { get; set; }

	string GetTooltip();
}
