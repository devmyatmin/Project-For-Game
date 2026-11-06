using UnityEngine;

namespace Beam.UI;

public class TMPActionHudElementViewAdapter : ActionHudElementViewAdapterBase
{
	[SerializeField]
	private TMPLabelViewAdapter _pressLabel;

	[SerializeField]
	private TMPLabelViewAdapter _keyLabel;

	[SerializeField]
	private UFilledImageViewAdapter _keyFill;

	[SerializeField]
	private TMPLabelViewAdapter _actionLabel;

	public override ILabelViewAdapter PressLabel => _pressLabel;

	public override ILabelViewAdapter KeyLabel => _keyLabel;

	public override IFilledImageViewAdapter KeyFill => _keyFill;

	public override ILabelViewAdapter ActionLabel => _actionLabel;
}
