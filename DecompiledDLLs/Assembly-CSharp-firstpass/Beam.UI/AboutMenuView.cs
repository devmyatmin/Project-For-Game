using UnityEngine;

namespace Beam.UI;

public abstract class AboutMenuView : ViewAdapterBase
{
	public abstract Transform CategoriesContainer { get; }

	public abstract void SetCopyrightText(string text);

	public abstract void SetCreditsText(string text);
}
