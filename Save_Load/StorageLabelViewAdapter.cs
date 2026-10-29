using UnityEngine;

namespace Beam.UI;

public class StorageLabelViewAdapter : MonoBehaviour
{
	[SerializeField]
	private TMPLabelViewAdapter _label;

	public ILabelViewAdapter Label => _label;
}
