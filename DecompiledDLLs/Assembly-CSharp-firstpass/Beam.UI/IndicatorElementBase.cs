using UnityEngine;

namespace Beam.UI;

public abstract class IndicatorElementBase : MonoBehaviour
{
	[SerializeField]
	private IndicatorElementViewBase _view;

	private IndicatorDataBase _data;

	protected IndicatorElementViewBase View => _view;

	public IndicatorDataBase Data => _data;

	public void Initialize(IndicatorDataBase data)
	{
		_data = data;
	}
}
