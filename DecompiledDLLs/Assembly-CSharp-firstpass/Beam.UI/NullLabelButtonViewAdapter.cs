using System;
using UnityEngine;

namespace Beam.UI;

public class NullLabelButtonViewAdapter : ViewAdapterBase, ILabelButtonViewAdapter, ILabelViewAdapter, IViewAdapter, IButtonViewAdapter, ISelectableViewAdapter
{
	private static NullLabelButtonViewAdapter _Instance = new NullLabelButtonViewAdapter();

	public static NullLabelButtonViewAdapter Instance => _Instance;

	public string Text
	{
		get
		{
			Debug.LogError((object)"NullLabelButtonViewAdapter::Text:: Not implemented!");
			return string.Empty;
		}
		set
		{
			Debug.LogError((object)"NullLabelButtonViewAdapter::Text:: Not implemented!");
		}
	}

	public Color Color
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			Debug.LogError((object)"NullLabelButtonViewAdapter::Color:: Not implemented!");
			return Color.black;
		}
		set
		{
			Debug.LogError((object)"NullLabelButtonViewAdapter::Color:: Not implemented!");
		}
	}

	public bool Interactable
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public event Action<object> Click;

	public event Action<ISelectableViewAdapter> Selected;

	public event Action<ISelectableViewAdapter> MouseEnter;

	public event Action<ISelectableViewAdapter> MouseLeave;

	public void PerformClick()
	{
		Debug.LogError((object)"NullLabelButtonViewAdapter::PerformClick:: Not implemented!");
	}

	public void Select()
	{
		Debug.LogError((object)"NullLabelButtonViewAdapter::Select:: Not implemented!");
	}

	public void ClearEventSubscribers()
	{
		Debug.LogError((object)"NullLabelButtonViewAdapter::ClearEventSubscribers:: Not implemented!");
	}
}
