using UnityEngine;

namespace Beam.UI.Crafting;

public abstract class CombinationButtonViewAdapterBase : ViewAdapterBase
{
	public abstract ILabelViewAdapter NameLabel { get; }

	public abstract IImageViewAdapter Icon { get; }

	public abstract IImageViewAdapter LockedIcon { get; }

	public abstract IImageViewAdapter CraftedIcon { get; }

	public abstract ICanvasGroupViewAdapter CanvasGroup { get; }

	public abstract IButtonViewAdapter Button { get; }

	public abstract CombinationMaterialPresenter MaterialPrefab { get; }

	public abstract GameObject MaterialsContainer { get; }

	public abstract GameObject CraftsmanshipLevelGroup { get; }

	public abstract ILabelViewAdapter CraftsmanshipLevelLabel { get; }
}
