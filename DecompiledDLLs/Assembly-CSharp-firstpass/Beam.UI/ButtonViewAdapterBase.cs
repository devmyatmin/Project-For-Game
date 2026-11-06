using System;
using UnityEngine;

namespace Beam.UI;

public abstract class ButtonViewAdapterBase<TButton> : SelectableViewAdapterBase<TButton>, IButtonViewAdapter, ISelectableViewAdapter, IViewAdapter where TButton : MonoBehaviour
{
	private TButton _button;

	protected TButton Button
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_button))
			{
				_button = ((Component)this).GetComponent<TButton>();
			}
			return _button;
		}
	}

	public abstract Color Color { get; set; }

	public event Action<object> Click;

	protected virtual void OnClick(object sender)
	{
		Click?.Invoke(sender);
	}

	public override void Show()
	{
		((Component)(object)Button).gameObject.SetActive(true);
		base.Show();
	}

	public override void Hide()
	{
		((Component)(object)Button).gameObject.SetActive(false);
		base.Hide();
	}

	public override void ClearEventSubscribers()
	{
		Click = null;
		base.ClearEventSubscribers();
	}

	public virtual void PerformClick()
	{
		OnClick(this);
	}

	protected override void OnDestroy()
	{
		Click = null;
		base.OnDestroy();
	}
}
