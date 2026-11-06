using System;
using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beam.UI;

public class TMPOptionButtonViewAdapter : UButtonViewAdapter, IOptionButtonViewAdapter, IOptionWidgetViewAdapter, ISelectableViewAdapter, IViewAdapter, IButtonViewAdapter, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler
{
	[SerializeField]
	private TMPLabelViewAdapter _label;

	[SerializeField]
	private TMPLabelViewAdapter _selectionLabel;

	private List<IOptionElementData> _elements = new List<IOptionElementData>();

	private int _selectedIndex = -1;

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

	public override bool Interactable
	{
		get
		{
			return ((Selectable)Button).interactable;
		}
		set
		{
			((Selectable)Button).interactable = value;
		}
	}

	public IReadOnlyList<IOptionElementData> Elements => _elements;

	public int SelectedIndex
	{
		get
		{
			return _selectedIndex;
		}
		set
		{
			if (_selectedIndex != value && _elements != null && _elements.Count > 1)
			{
				_selectedIndex = value;
				if (_selectedIndex > _elements.Count - 1)
				{
					_selectedIndex = 0;
				}
				if (_selectedIndex < 0)
				{
					_selectedIndex = 0;
				}
				_selectionLabel.Text = _elements[_selectedIndex].Text;
				OnSelectionChanged(this);
			}
		}
	}

	public string SelectedText
	{
		get
		{
			return _selectionLabel.Text;
		}
		set
		{
			_selectionLabel.Text = value;
		}
	}

	public IOptionElementData SelectedElement
	{
		get
		{
			if (_elements.Count <= 0)
			{
				return null;
			}
			return _elements[_selectedIndex];
		}
	}

	public event Action<IOptionButtonViewAdapter> SelectionChanged;

	protected virtual void OnSelectionChanged(IOptionButtonViewAdapter sender)
	{
		SelectionChanged?.Invoke(sender);
	}

	public void Initialize(IEnumerable<IOptionElementData> items)
	{
		_elements.Clear();
		_elements.AddRange(items);
		IOptionElementData optionElementData = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_elements);
		if (optionElementData != null)
		{
			_selectionLabel.Text = optionElementData.Text;
			_selectedIndex = 0;
		}
	}

	protected override void OnDestroy()
	{
		SelectionChanged = null;
		base.OnDestroy();
	}

	public string GetTooltip()
	{
		if (_elements == null || _elements.Count == 0)
		{
			return string.Empty;
		}
		return _elements[SelectedIndex].Tooltip;
	}

	protected override void OnClick(object sender)
	{
		SelectedIndex++;
		base.OnClick(sender);
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
