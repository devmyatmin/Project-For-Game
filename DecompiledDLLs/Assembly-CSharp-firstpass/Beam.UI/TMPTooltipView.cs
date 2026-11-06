using UnityEngine;

namespace Beam.UI;

public class TMPTooltipView : TooltipViewBase
{
	[SerializeField]
	private TMPLabelViewAdapter _label;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	public override ILabelViewAdapter Label => _label;

	public override UCanvasGroupViewAdapter CanvasGroup => _canvasGroup;
}
