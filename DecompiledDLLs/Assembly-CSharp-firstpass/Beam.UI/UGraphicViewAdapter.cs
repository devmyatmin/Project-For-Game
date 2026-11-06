using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class UGraphicViewAdapter : GraphicViewAdapterBase<Graphic>
{
	public override Color Color
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Graphic.color;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Graphic.color = value;
		}
	}
}
