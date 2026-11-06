using UnityEngine;

namespace Beam.UI;

public class TMPInputMapCategoryHeader : MonoBehaviour, IInputMapCategoryHeader
{
	[SerializeField]
	private TMPLabelViewAdapter _label;

	[SerializeField]
	private GameObjectViewAdapter _subheadingsContainer;

	public ILabelViewAdapter Label => _label;

	public IGameObjectViewAdapter SubheadingsContainer => _subheadingsContainer;
}
