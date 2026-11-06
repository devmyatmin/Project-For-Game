using UnityEngine;

namespace Beam.UI;

public abstract class ScrollbarViewAdapterBase<TScrollbar> : ViewAdapterBase, IScrollbarViewAdapter, IViewAdapter where TScrollbar : MonoBehaviour
{
	private TScrollbar _scrollbar;

	protected TScrollbar Scrollbar
	{
		get
		{
			if ((Object)(object)_scrollbar == (Object)null)
			{
				_scrollbar = ((Component)this).GetComponent<TScrollbar>();
			}
			return _scrollbar;
		}
	}

	public abstract float Value { get; set; }
}
