using UnityEngine;

namespace Beam.UI;

public abstract class LabelViewAdapterBase<TLabel> : ViewAdapterBase, ILabelViewAdapter, IViewAdapter where TLabel : MonoBehaviour
{
	private TLabel _textField;

	protected TLabel TextField
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_textField))
			{
				_textField = ((Component)this).GetComponent<TLabel>();
			}
			return _textField;
		}
	}

	public abstract string Text { get; set; }

	public abstract Color Color { get; set; }
}
