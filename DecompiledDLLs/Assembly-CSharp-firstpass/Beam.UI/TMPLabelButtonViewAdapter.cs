using UnityEngine;

namespace Beam.UI;

public class TMPLabelButtonViewAdapter : UButtonViewAdapter, ILabelButtonViewAdapter, ILabelViewAdapter, IViewAdapter, IButtonViewAdapter, ISelectableViewAdapter
{
	[SerializeField]
	private TMPLabelViewAdapter _label;

	protected TMPLabelViewAdapter Label
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_label))
			{
				_label = ((Component)this).GetComponent<TMPLabelViewAdapter>();
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
