using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam.UI;

public class NullButtonViewAdapter : NullViewAdapter<NullButtonViewAdapter>, IButtonViewAdapter, ISelectableViewAdapter, IViewAdapter
{
	public Color Color
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
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

	public event Action<ISelectableViewAdapter> Selected
	{
		add
		{
		}
		remove
		{
		}
	}

	public event Action<ISelectableViewAdapter> MouseEnter
	{
		add
		{
		}
		remove
		{
		}
	}

	public event Action<ISelectableViewAdapter> MouseLeave
	{
		add
		{
		}
		remove
		{
		}
	}

	public event Action<object> Click
	{
		add
		{
		}
		remove
		{
		}
	}

	public void PerformClick()
	{
	}

	public void Select()
	{
	}

	public void ClearEventSubscribers()
	{
	}
}
