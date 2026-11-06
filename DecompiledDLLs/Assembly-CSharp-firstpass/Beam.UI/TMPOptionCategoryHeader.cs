using UnityEngine;

namespace Beam.UI;

public class TMPOptionCategoryHeader : ViewAdapterBase, IOptionCategoryHeader
{
	[SerializeField]
	private TMPLabelViewAdapter _label;

	[SerializeField]
	private Transform _elementsContainer;

	public ILabelViewAdapter Label => _label;

	public Transform ElementsContainer => _elementsContainer;

	public int Id { get; set; }
}
