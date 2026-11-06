using System;
using System.Collections.Generic;

namespace Beam.UI;

public interface IOptionButtonViewAdapter : IOptionWidgetViewAdapter, ISelectableViewAdapter, IViewAdapter, IButtonViewAdapter
{
	IReadOnlyList<IOptionElementData> Elements { get; }

	IOptionElementData SelectedElement { get; }

	int SelectedIndex { get; set; }

	string SelectedText { get; set; }

	event Action<IOptionButtonViewAdapter> SelectionChanged;

	void Initialize(IEnumerable<IOptionElementData> items);
}
