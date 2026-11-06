using System;

namespace Beam.UI;

public interface ISelectableViewAdapter : IViewAdapter
{
	bool Interactable { get; set; }

	event Action<ISelectableViewAdapter> Selected;

	event Action<ISelectableViewAdapter> MouseEnter;

	event Action<ISelectableViewAdapter> MouseLeave;
}
