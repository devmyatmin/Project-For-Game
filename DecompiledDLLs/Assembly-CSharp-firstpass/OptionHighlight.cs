using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionHighlight : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image[] _controlBackground;

	[SerializeField]
	private Selectable[] _controlButtons;

	[SerializeField]
	private Text _controlName;

	[SerializeField]
	private Graphic _controlControl;

	private void Awake()
	{
		Unhighlight();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Highlight();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Unhighlight();
	}

	public void Highlight()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _controlBackground.Length; i++)
		{
			((Behaviour)_controlBackground[i]).enabled = true;
		}
		for (int j = 0; j < _controlButtons.Length; j++)
		{
			_controlButtons[j].targetGraphic.color = Color.white;
		}
		if (Object.op_Implicit((Object)(object)_controlControl))
		{
			_controlControl.color = Color.white;
		}
		((Graphic)_controlName).color = Color.white;
	}

	public void Unhighlight()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _controlBackground.Length; i++)
		{
			((Behaviour)_controlBackground[i]).enabled = false;
		}
		for (int j = 0; j < _controlButtons.Length; j++)
		{
			_controlButtons[j].targetGraphic.color = Color.clear;
		}
		if (Object.op_Implicit((Object)(object)_controlControl))
		{
			_controlControl.color = Color.grey;
		}
		((Graphic)_controlName).color = Color.grey;
	}
}
