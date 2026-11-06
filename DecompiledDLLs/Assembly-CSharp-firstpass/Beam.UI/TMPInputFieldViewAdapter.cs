using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beam.UI;

[RequireComponent(typeof(TMP_InputField))]
public class TMPInputFieldViewAdapter : InputFieldViewAdapterBase<TMP_InputField>
{
	public override string Text
	{
		get
		{
			return InputField.text;
		}
		set
		{
			InputField.text = value;
		}
	}

	public override int CharacterLimit
	{
		get
		{
			return InputField.characterLimit;
		}
		set
		{
			InputField.characterLimit = value;
		}
	}

	private void Awake()
	{
		((UnityEventBase)InputField.onValueChanged).RemoveAllListeners();
		((UnityEvent<string>)(object)InputField.onValueChanged).AddListener((UnityAction<string>)OnValueChanged);
		((UnityEventBase)InputField.onEndEdit).RemoveAllListeners();
		((UnityEvent<string>)(object)InputField.onEndEdit).AddListener((UnityAction<string>)OnKeyDown);
	}

	protected override void OnDestroy()
	{
		((UnityEventBase)InputField.onValueChanged).RemoveAllListeners();
		InputField.onValueChanged = null;
		((UnityEventBase)InputField.onEndEdit).RemoveAllListeners();
		InputField.onEndEdit = null;
		base.OnDestroy();
	}

	public override void Select()
	{
		base.Select();
		((Selectable)InputField).Select();
		InputField.ActivateInputField();
	}
}
