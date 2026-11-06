using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Scrollbar))]
public class UScrollbarViewAdapter : ScrollbarViewAdapterBase<Scrollbar>
{
	public override float Value
	{
		get
		{
			return Scrollbar.value;
		}
		set
		{
			Scrollbar.value = value;
		}
	}
}
