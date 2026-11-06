using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beam.UI;

public abstract class OptionButtonDecoratorBase : IOptionButtonViewAdapter, IOptionWidgetViewAdapter, ISelectableViewAdapter, IViewAdapter, IButtonViewAdapter
{
	private readonly IOptionButtonViewAdapter _button;

	public Transform transform => _button.transform;

	public bool Visible => _button.Visible;

	public bool Enabled
	{
		get
		{
			return _button.Enabled;
		}
		set
		{
			_button.Enabled = value;
		}
	}

	public bool Interactable
	{
		get
		{
			return _button.Interactable;
		}
		set
		{
			_button.Interactable = value;
		}
	}

	public string Text
	{
		get
		{
			return _button.Text;
		}
		set
		{
			_button.Text = value;
		}
	}

	public Color Color
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _button.Color;
		}
	}

	public string SelectedText
	{
		get
		{
			return _button.SelectedText;
		}
		set
		{
			_button.SelectedText = value;
		}
	}

	public int SelectedIndex
	{
		get
		{
			return _button.SelectedIndex;
		}
		set
		{
			_button.SelectedIndex = value;
		}
	}

	public IOptionElementData SelectedElement => _button.SelectedElement;

	public IReadOnlyList<IOptionElementData> Elements => _button.Elements;

	public event EmptyEventHandler ShowView;

	public event EmptyEventHandler HideView;

	public event Action<object> Click;

	public event Action<ISelectableViewAdapter> Selected;

	public event Action<IOptionButtonViewAdapter> SelectionChanged;

	public event Action<ISelectableViewAdapter> MouseEnter;

	public event Action<ISelectableViewAdapter> MouseLeave;

	protected OptionButtonDecoratorBase(IOptionButtonViewAdapter button)
	{
		_button = button;
		_button.ShowView += () =>
		{
			ShowView?.Invoke();
		};
		_button.HideView += () =>
		{
			HideView?.Invoke();
		};
		_button.Click += (object sender) =>
		{
			Click?.Invoke(this);
		};
		_button.Selected += (ISelectableViewAdapter sender) =>
		{
			Selected?.Invoke(this);
		};
		_button.SelectionChanged += (IOptionButtonViewAdapter sender) =>
		{
			SelectionChanged?.Invoke(this);
		};
		_button.MouseEnter += (ISelectableViewAdapter sender) =>
		{
			MouseEnter?.Invoke(this);
		};
		_button.MouseLeave += (ISelectableViewAdapter sender) =>
		{
			MouseLeave?.Invoke(this);
		};
	}

	public string GetTooltip()
	{
		return _button.GetTooltip();
	}

	public void Initialize(IEnumerable<IOptionElementData> elements)
	{
		_button.Initialize(elements);
	}

	public void Show()
	{
		_button.Show();
	}

	public void Hide()
	{
		_button.Hide();
	}

	public void PerformClick()
	{
		_button.PerformClick();
	}

	public void Select()
	{
		_button.Select();
	}

	public void ClearEventSubscribers()
	{
		_button.ClearEventSubscribers();
		ShowView = null;
		HideView = null;
		Click = null;
		Selected = null;
		SelectionChanged = null;
		MouseEnter = null;
		MouseLeave = null;
	}
}
