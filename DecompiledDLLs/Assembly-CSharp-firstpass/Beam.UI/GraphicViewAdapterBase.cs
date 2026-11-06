using UnityEngine;

namespace Beam.UI;

public abstract class GraphicViewAdapterBase : ViewAdapterBase
{
	public abstract Color Color { get; set; }
}
public abstract class GraphicViewAdapterBase<TGraphic> : GraphicViewAdapterBase where TGraphic : Component
{
	private TGraphic _graphic;

	protected TGraphic Graphic
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_graphic))
			{
				_graphic = ((Component)this).GetComponent<TGraphic>();
			}
			return _graphic;
		}
	}
}
