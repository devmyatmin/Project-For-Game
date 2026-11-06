using System;
using UnityEngine;

namespace Ceto;

[Serializable]
public class OverlayNormalTexture
{
	public Texture tex;

	public Vector2 scaleUV = Vector2.one;

	public Vector2 offsetUV;

	[Range(0f, 4f)]
	public float alpha = 1f;

	public Texture mask;

	public OVERLAY_MASK_MODE maskMode = OVERLAY_MASK_MODE.WAVES_AND_OVERLAY_BLEND;

	[Range(0f, 1f)]
	public float maskAlpha = 1f;

	public bool IsDrawable
	{
		get
		{
			if (alpha == 0f || !((Object)(object)tex != (Object)null))
			{
				if (maskAlpha != 0f)
				{
					return (Object)(object)mask != (Object)null;
				}
				return false;
			}
			return true;
		}
	}

	public OverlayNormalTexture()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
