using UnityEngine;

namespace Beam.UI;

public class GraphicsGroup : MonoBehaviour
{
	[SerializeField]
	private GraphicViewAdapterBase[] _graphics;

	public void SetColor(Color color)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (_graphics == null)
		{
			_graphics = ((Component)this).GetComponentsInChildren<GraphicViewAdapterBase>(true);
		}
		GraphicViewAdapterBase[] graphics = _graphics;
		for (int i = 0; i < graphics.Length; i++)
		{
			graphics[i].Color = color;
		}
	}
}
