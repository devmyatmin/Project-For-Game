using System.Collections.Generic;
using Beam.Crafting;
using UnityEngine;

namespace Beam.UI.Crafting;

public abstract class CrafterMenuViewAdapterBase : ViewAdapterBase
{
	public abstract ICanvasGroupViewAdapter CanvasGroup { get; }

	public abstract ICanvasGroupViewAdapter CategoryCanvasGroup { get; }

	public abstract ICanvasGroupViewAdapter CombinationsCanvasGroup { get; }

	public abstract IScrollviewViewAdapter Scrollview { get; }

	public abstract IScrollviewViewAdapter SubcategoriesScrollview { get; }

	public abstract ILabelViewAdapter CategoryLabel { get; }

	public abstract IGameObjectViewAdapter InfoContainer { get; }

	public abstract ILabelViewAdapter InfoLabel { get; }

	public abstract ITransformViewAdapter CategoryButtonContainer { get; }

	public abstract ILabelButtonViewAdapter NextCategoryButton { get; }

	public abstract ILabelButtonViewAdapter PreviousCategoryButton { get; }

	public abstract Texture2D BlurMaskTexture { get; }

	public abstract IList<TMPCraftingSubcategoryButtonViewAdapter> SubcategoryButtons { get; }

	public abstract IList<CombinationButtonPresenter> CombinationButtons { get; }

	public abstract TMPCraftingSubcategoryButtonViewAdapter CreateSubcategoryButton();

	public abstract CombinationButtonPresenter CreateCombinationButton();

	public abstract void DestroySubcategoryButtons();

	public abstract void DestroyCombinationButtons();

	public abstract void SetNavigation(CraftingSubcategory currentSubcategory);
}
