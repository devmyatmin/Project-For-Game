using UnityEngine;

namespace Beam.UI;

public abstract class InputFieldButtonBase<TLabel> : UButtonViewAdapter, IInputFieldButtonViewAdapter, IButtonViewAdapter, ISelectableViewAdapter, IViewAdapter, ILabelButtonViewAdapter, ILabelViewAdapter where TLabel : MonoBehaviour, ILabelViewAdapter
{
	[SerializeField]
	private TLabel _label;

	protected TLabel Label
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_label))
			{
				_label = ((Component)this).GetComponent<TLabel>();
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

	public InputFieldButtonType Type { get; set; }

	public InputFieldButtonInfo Info { get; set; }
}
