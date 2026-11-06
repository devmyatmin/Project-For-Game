using System;
using UnityEngine;

namespace Ceto;

[Serializable]
public class OverlayClipTexture
{
	public Texture tex;

	public Vector2 scaleUV = Vector2.one;

	public Vector2 offsetUV;

	[Range(0f, 4f)]
	public float alpha = 1f;

	public bool ignoreQuerys;

	public bool IsDrawable
	{
		get
		{
			if (alpha != 0f)
			{
				return (Object)(object)tex != (Object)null;
			}
			return false;
		}
	}

	public OverlayClipTexture()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
