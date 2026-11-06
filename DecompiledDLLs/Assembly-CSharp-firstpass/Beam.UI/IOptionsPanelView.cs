using UnityEngine;

namespace Beam.UI;

public interface IOptionsPanelView : IViewAdapter
{
	Transform CategoriesContainer { get; }
}
