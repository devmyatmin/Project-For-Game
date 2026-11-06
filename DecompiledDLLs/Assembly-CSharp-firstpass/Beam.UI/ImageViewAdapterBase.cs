using UnityEngine;

namespace Beam.UI;

public abstract class ImageViewAdapterBase<TImage> : ViewAdapterBase, IImageViewAdapter, IViewAdapter where TImage : MonoBehaviour
{
	private TImage _image;

	protected TImage Image
	{
		get
		{
			if ((Object)(object)_image == (Object)null)
			{
				_image = ((Component)this).GetComponent<TImage>();
			}
			return _image;
		}
	}

	public abstract Color Color { get; set; }

	public abstract Sprite Sprite { get; set; }
}
