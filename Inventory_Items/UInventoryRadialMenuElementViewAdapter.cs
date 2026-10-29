using UnityEngine;

namespace Beam.UI;

public class UInventoryRadialMenuElementViewAdapter : ViewAdapterBase, IRadialMenuElementViewAdapter
{
	[SerializeField]
	private SelectableBase _selectable;

	[SerializeField]
	private UFilledImageViewAdapter _image;

	[SerializeField]
	private UImageViewAdapter _depletedImage;

	[SerializeField]
	private GameObject _quantityGroup;

	[SerializeField]
	private TMPLabelViewAdapter _quantityLabel;

	public SelectableBase Selectable => _selectable;

	public IFilledImageViewAdapter Image => _image;

	public IImageViewAdapter DepletedImage => _depletedImage;

	public GameObject QuantityGroup => _quantityGroup;

	public ILabelViewAdapter QuantityLabel => _quantityLabel;
}
