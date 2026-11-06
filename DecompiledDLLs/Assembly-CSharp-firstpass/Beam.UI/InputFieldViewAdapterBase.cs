using UnityEngine;

namespace Beam.UI;

public abstract class InputFieldViewAdapterBase<TInput> : ViewAdapterBase, IInputFieldViewAdapter, IViewAdapter where TInput : MonoBehaviour
{
	private TInput _inputField;

	protected TInput InputField
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_inputField))
			{
				_inputField = ((Component)this).GetComponent<TInput>();
			}
			return _inputField;
		}
	}

	public abstract string Text { get; set; }

	public abstract int CharacterLimit { get; set; }

	public event InputFieldEventHandler ValueChanged;

	public event InputFieldEventHandler KeyDown;

	protected virtual void OnValueChanged(string value)
	{
		ValueChanged?.Invoke(value);
	}

	protected virtual void OnKeyDown(string value)
	{
		KeyDown?.Invoke(value);
	}

	public virtual void Select()
	{
	}

	protected virtual void OnDestroy()
	{
		ValueChanged = null;
		KeyDown = null;
	}
}
