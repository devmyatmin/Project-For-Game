using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI;

public abstract class SelectableViewAdapterBase<TSelectable> : ViewAdapterBase, ISelectableViewAdapter, IViewAdapter, ISelectHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler where TSelectable : MonoBehaviour
{
	private TSelectable _selectable;

	protected TSelectable Selectable
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_selectable))
			{
				_selectable = ((Component)this).GetComponent<TSelectable>();
			}
			return _selectable;
		}
	}

	public abstract bool Interactable { get; set; }

	public event Action<ISelectableViewAdapter> Selected;

	public event Action<ISelectableViewAdapter> MouseEnter;

	public event Action<ISelectableViewAdapter> MouseLeave;

	public abstract TSelectable GetUnderlyingElement();

	protected virtual void OnSelected(ISelectableViewAdapter sender)
	{
		Selected?.Invoke(sender);
	}

	protected virtual void OnMouseEnter(ISelectableViewAdapter sender)
	{
		MouseEnter?.Invoke(sender);
	}

	protected virtual void OnMouseLeave(ISelectableViewAdapter sender)
	{
		MouseLeave?.Invoke(sender);
	}

	public virtual void Select()
	{
		OnSelected(this);
	}

	public virtual void ClearEventSubscribers()
	{
		Selected = null;
		MouseEnter = null;
		MouseLeave = null;
	}

	protected virtual void OnDestroy()
	{
		ClearEventSubscribers();
	}

	void ISelectHandler.OnSelect(BaseEventData eventData)
	{
		OnSelected(this);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		OnMouseEnter(this);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		OnMouseLeave(this);
	}
}
