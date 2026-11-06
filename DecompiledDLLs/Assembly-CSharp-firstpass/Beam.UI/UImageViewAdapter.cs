using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

[RequireComponent(typeof(Image))]
public class UImageViewAdapter : ImageViewAdapterBase<Image>
{
	public override Color Color
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Graphic)Image).color;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Graphic)Image).color = value;
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return Image.sprite;
		}
		set
		{
			Image.sprite = value;
		}
	}

	public override bool Enabled
	{
		get
		{
			return ((Behaviour)Image).enabled;
		}
		set
		{
			((Behaviour)Image).enabled = value;
		}
	}
}
