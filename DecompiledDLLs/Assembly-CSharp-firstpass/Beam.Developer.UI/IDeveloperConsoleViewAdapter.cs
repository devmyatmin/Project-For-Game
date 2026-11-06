using Beam.UI;

namespace Beam.Developer.UI;

public interface IDeveloperConsoleViewAdapter : IViewAdapter
{
	ILabelViewAdapter OutputLabel { get; }

	IScrollbarViewAdapter Scrollbar { get; }

	IInputFieldViewAdapter InputField { get; }
}
