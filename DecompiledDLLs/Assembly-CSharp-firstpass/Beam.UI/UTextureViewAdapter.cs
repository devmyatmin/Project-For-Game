using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

[RequireComponent(typeof(RawImage))]
public class UTextureViewAdapter : TextureViewAdapterBase<RawImage>
{
	public override Texture Texture
	{
		get
		{
			return RawImage.texture;
		}
		set
		{
			RawImage.texture = value;
		}
	}

	public override Color Color
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Graphic)RawImage).color;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Graphic)RawImage).color = value;
		}
	}
}
