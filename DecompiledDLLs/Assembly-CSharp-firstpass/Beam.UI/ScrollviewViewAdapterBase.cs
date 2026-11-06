using UnityEngine;

namespace Beam.UI;

public abstract class ScrollviewViewAdapterBase<TScrollview> : ViewAdapterBase, IScrollviewViewAdapter, IViewAdapter where TScrollview : MonoBehaviour
{
	private TScrollview _scrollview;

	protected TScrollview Scrollview
	{
		get
		{
			if ((Object)(object)_scrollview == (Object)null)
			{
				_scrollview = ((Component)this).GetComponent<TScrollview>();
			}
			return _scrollview;
		}
	}

	public abstract ITransformViewAdapter Content { get; }

	public abstract float NormalizedHorizontalPosition { get; set; }

	public abstract float NormalizedVerticalPosition { get; set; }
}
