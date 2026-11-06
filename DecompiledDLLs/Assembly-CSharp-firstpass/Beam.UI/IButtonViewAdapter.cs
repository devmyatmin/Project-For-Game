using System;
using UnityEngine;

namespace Beam.UI;

public interface IButtonViewAdapter : ISelectableViewAdapter, IViewAdapter
{
	Color Color { get; }

	event Action<object> Click;

	void PerformClick();

	void Select();

	void ClearEventSubscribers();
}
