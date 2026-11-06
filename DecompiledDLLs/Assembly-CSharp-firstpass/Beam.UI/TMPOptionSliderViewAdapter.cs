using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beam.UI;

public class TMPOptionSliderViewAdapter : TMPSliderViewAdapter, IOptionSliderViewAdapter, IOptionWidgetViewAdapter, ISelectableViewAdapter, IViewAdapter, ISliderViewAdapter, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler
{
	[SerializeField]
	private string _tooltip;

	public bool Interactable
	{
		get
		{
			return ((Selectable)Slider).interactable;
		}
		set
		{
			((Selectable)Slider).interactable = value;
		}
	}

	public event Action<ISelectableViewAdapter> MouseEnter;

	public event Action<ISelectableViewAdapter> MouseLeave;

	public event Action<ISelectableViewAdapter> Selected;

	public string GetTooltip()
	{
		return _tooltip;
	}

	protected virtual void OnMouseEnter(ISelectableViewAdapter sender)
	{
		MouseEnter?.Invoke(sender);
	}

	protected virtual void OnMouseLeave(ISelectableViewAdapter sender)
	{
		MouseLeave?.Invoke(sender);
	}

	protected virtual void OnSelected(ISelectableViewAdapter sender)
	{
		Selected?.Invoke(sender);
	}

	protected override void OnDestroy()
	{
		MouseEnter = null;
		MouseLeave = null;
		Selected = null;
		base.OnDestroy();
	}

	protected override void OnValueChanged(float value)
	{
		Refresh();
		base.OnValueChanged(value);
	}

	public void Refresh()
	{
		ValueText = $"{NormalizedValue:P0}";
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
