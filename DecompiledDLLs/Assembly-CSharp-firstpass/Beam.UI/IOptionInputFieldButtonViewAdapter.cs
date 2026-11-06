using System.Collections.Generic;

namespace Beam.UI;

public interface IOptionInputFieldButtonViewAdapter : IOptionWidgetViewAdapter, ISelectableViewAdapter, IViewAdapter
{
	IList<IInputFieldButtonViewAdapter> Buttons { get; }

	IInputFieldButtonViewAdapter CreateButton();

	void SetNavigation();
}
