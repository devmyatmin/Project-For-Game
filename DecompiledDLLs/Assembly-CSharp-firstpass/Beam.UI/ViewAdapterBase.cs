using System;
using UnityEngine;

namespace Beam.UI;

[Serializable]
public abstract class ViewAdapterBase : MonoBehaviour, IViewAdapter
{
	private bool _visible;

	public bool Visible
	{
		get
		{
			return _visible;
		}
		protected set
		{
			_visible = value;
		}
	}

	public virtual bool Enabled { get; set; }

	public event EmptyEventHandler ShowView;

	public event EmptyEventHandler HideView;

	protected virtual void OnViewShow()
	{
		ShowView?.Invoke();
	}

	protected virtual void OnViewHide()
	{
		HideView?.Invoke();
	}

	public virtual void Show()
	{
		if (!_visible)
		{
			OnViewShow();
			_visible = true;
		}
	}

	public virtual void Hide()
	{
		if (_visible)
		{
			OnViewHide();
			_visible = false;
		}
	}
}
