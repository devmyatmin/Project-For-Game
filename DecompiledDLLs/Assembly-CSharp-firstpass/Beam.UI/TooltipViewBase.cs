using UnityEngine;

namespace Beam.UI;

public abstract class TooltipViewBase : MonoBehaviour
{
	public abstract ILabelViewAdapter Label { get; }

	public abstract UCanvasGroupViewAdapter CanvasGroup { get; }
}
