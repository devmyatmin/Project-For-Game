using UnityEngine;

namespace Beam.UI.Crafting;

public class UCombinationButtonViewAdapter : CombinationButtonViewAdapterBase
{
	[SerializeField]
	private TMPLabelViewAdapter _nameLabel;

	[SerializeField]
	private UImageViewAdapter _icon;

	[SerializeField]
	private UImageViewAdapter _lockedIcon;

	[SerializeField]
	private UImageViewAdapter _craftedIcon;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private UButtonViewAdapter _button;

	[SerializeField]
	private CombinationMaterialPresenter _materialPrefab;

	[SerializeField]
	private GameObject _materialsContainer;

	[SerializeField]
	private GameObject _craftsmanshipLevelGroup;

	[SerializeField]
	private TMPLabelViewAdapter _craftsmanshipLevelLabel;

	public override ILabelViewAdapter NameLabel => _nameLabel;

	public override IImageViewAdapter Icon => _icon;

	public override IImageViewAdapter LockedIcon => _lockedIcon;

	public override IImageViewAdapter CraftedIcon => _craftedIcon;

	public override ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public override IButtonViewAdapter Button => _button;

	public override CombinationMaterialPresenter MaterialPrefab => _materialPrefab;

	public override GameObject MaterialsContainer => _materialsContainer;

	public override GameObject CraftsmanshipLevelGroup => _craftsmanshipLevelGroup;

	public override ILabelViewAdapter CraftsmanshipLevelLabel => _craftsmanshipLevelLabel;
}
