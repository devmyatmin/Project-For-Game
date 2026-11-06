using System;
using UnityEngine;

namespace Beam.UI;

public class NullViewAdapter<TViewAdapter> : IViewAdapter where TViewAdapter : IViewAdapter
{
	private static readonly TViewAdapter _Instance = Activator.CreateInstance<TViewAdapter>();

	public static TViewAdapter Instance => _Instance;

	public Transform transform => NullTransform.Instance;

	public bool Visible { get; }

	public bool Enabled { get; set; }

	event EmptyEventHandler IViewAdapter.ShowView
	{
		add
		{
		}
		remove
		{
		}
	}

	event EmptyEventHandler IViewAdapter.HideView
	{
		add
		{
		}
		remove
		{
		}
	}

	public void Show()
	{
	}

	public void Hide()
	{
	}
}
