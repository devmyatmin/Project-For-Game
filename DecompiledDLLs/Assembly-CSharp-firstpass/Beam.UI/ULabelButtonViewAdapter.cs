using UnityEngine;

namespace Beam.UI;

public class ULabelButtonViewAdapter : UButtonViewAdapter, ILabelButtonViewAdapter, ILabelViewAdapter, IViewAdapter, IButtonViewAdapter, ISelectableViewAdapter
{
	[SerializeField]
	private ULabelViewAdapter _label;

	protected ULabelViewAdapter Label
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_label))
			{
				_label = ((Component)this).GetComponent<ULabelViewAdapter>();
			}
			return _label;
		}
	}

	public string Text
	{
		get
		{
			return Label.Text;
		}
		set
		{
			Label.Text = value;
		}
	}

	public override Color Color
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Label.Color;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Label.Color = value;
		}
	}
}
