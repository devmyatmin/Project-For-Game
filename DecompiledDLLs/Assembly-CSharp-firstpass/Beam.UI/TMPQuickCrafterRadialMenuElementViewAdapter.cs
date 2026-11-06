using UnityEngine;

namespace Beam.UI;

public class TMPQuickCrafterRadialMenuElementViewAdapter : ViewAdapterBase, IRadialMenuElementViewAdapter
{
	[SerializeField]
	private SelectableBase _selectable;

	[SerializeField]
	private UFilledImageViewAdapter _image;

	[SerializeField]
	private UImageViewAdapter _elementTypeImage;

	[SerializeField]
	private UImageViewAdapter _dynamicImage;

	public SelectableBase Selectable => _selectable;

	public IFilledImageViewAdapter Image => _image;

	public IImageViewAdapter ElementTypeImage => _elementTypeImage;

	public void SetElementType(QuickCrafterRadialElementType type)
	{
		_dynamicImage.Enabled = type == QuickCrafterRadialElementType.Pile;
		if (type == QuickCrafterRadialElementType.Base)
		{
			((Component)((Component)_elementTypeImage).transform.parent).gameObject.SetActive(false);
			return;
		}
		_elementTypeImage.Sprite = Resources.Load<Sprite>($"Sprites/Crafter_{type}_Icon");
		((Component)((Component)_elementTypeImage).transform.parent).gameObject.SetActive(true);
	}
}
