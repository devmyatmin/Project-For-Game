using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI;

public class TMPOptionInputFieldButtonViewAdapter : USelectableViewAdapter, IOptionInputFieldButtonViewAdapter, IOptionWidgetViewAdapter, ISelectableViewAdapter, IViewAdapter, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler
{
	[SerializeField]
	private TMPLabelViewAdapter _label;

	[SerializeField]
	private TMPInputFieldButtonViewAdapter _inputFieldButtonPrefab;

	[SerializeField]
	private Transform _inputFieldButtonContainer;

	private List<IInputFieldButtonViewAdapter> _buttons = new List<IInputFieldButtonViewAdapter>();

	protected ILabelViewAdapter Label
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

	public IList<IInputFieldButtonViewAdapter> Buttons => _buttons;

	public string GetTooltip()
	{
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		_buttons.Clear();
		base.OnDestroy();
	}

	public IInputFieldButtonViewAdapter CreateButton()
	{
		TMPInputFieldButtonViewAdapter tMPInputFieldButtonViewAdapter = Object.Instantiate<TMPInputFieldButtonViewAdapter>(_inputFieldButtonPrefab);
		((Component)tMPInputFieldButtonViewAdapter).transform.SetParent(_inputFieldButtonContainer, false);
		_buttons.Add(tMPInputFieldButtonViewAdapter);
		return tMPInputFieldButtonViewAdapter;
	}

	public void SetNavigation()
	{
		IInputFieldButtonViewAdapter inputFieldButtonViewAdapter = _buttons[0];
		IInputFieldButtonViewAdapter inputFieldButtonViewAdapter2 = _buttons[1];
		if (inputFieldButtonViewAdapter.Enabled)
		{
			((Component)this).transform.SetNavigation<Transform>(NavigationDirection.Right, inputFieldButtonViewAdapter.transform);
			inputFieldButtonViewAdapter.transform.SetNavigation<Transform>(NavigationDirection.Left, ((Component)this).transform);
			if (inputFieldButtonViewAdapter2.Enabled)
			{
				inputFieldButtonViewAdapter.transform.SetNavigation<Transform>(NavigationDirection.Right, inputFieldButtonViewAdapter2.transform);
				inputFieldButtonViewAdapter2.transform.SetNavigation<Transform>(NavigationDirection.Left, inputFieldButtonViewAdapter.transform);
			}
		}
		else if (inputFieldButtonViewAdapter2.Enabled)
		{
			((Component)this).transform.SetNavigation<Transform>(NavigationDirection.Right, inputFieldButtonViewAdapter2.transform);
			inputFieldButtonViewAdapter2.transform.SetNavigation<Transform>(NavigationDirection.Left, ((Component)this).transform);
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		OnMouseEnter(this);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		OnMouseLeave(this);
	}

	void ISelectHandler.OnSelect(BaseEventData eventData)
	{
		OnSelected(this);
	}
}
