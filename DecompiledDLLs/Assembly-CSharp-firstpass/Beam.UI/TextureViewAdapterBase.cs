using UnityEngine;

namespace Beam.UI;

public abstract class TextureViewAdapterBase<TTexture> : ViewAdapterBase, ITextureViewAdapter, IViewAdapter where TTexture : MonoBehaviour
{
	private TTexture _rawImage;

	protected TTexture RawImage
	{
		get
		{
			if ((Object)(object)_rawImage == (Object)null)
			{
				_rawImage = ((Component)this).GetComponent<TTexture>();
			}
			return _rawImage;
		}
	}

	public override bool Enabled
	{
		get
		{
			return ((Behaviour)(object)RawImage).enabled;
		}
		set
		{
			((Behaviour)(object)RawImage).enabled = value;
		}
	}

	public abstract Texture Texture { get; set; }

	public abstract Color Color { get; set; }
}
