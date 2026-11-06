using UnityEngine.UI;

namespace Beam.UI;

public class USelectableViewAdapter : SelectableViewAdapterBase<Selectable>
{
	public override bool Interactable
	{
		get
		{
			return Selectable.interactable;
		}
		set
		{
			Selectable.interactable = value;
		}
	}

	public override Selectable GetUnderlyingElement()
	{
		return Selectable;
	}
}
