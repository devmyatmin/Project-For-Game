using System.Collections.Generic;
using Beam.Crafting;
using UnityEngine;

namespace Beam.UI.Crafting;

public class NullCrafterMenuViewAdapter : CrafterMenuViewAdapterBase
{
	public static NullCrafterMenuViewAdapter Instance { get; } = new NullCrafterMenuViewAdapter();


	public override ICanvasGroupViewAdapter CanvasGroup => NullViewAdapter<NullCanvasGroupViewAdapter>.Instance;

	public override ICanvasGroupViewAdapter CategoryCanvasGroup => NullViewAdapter<NullCanvasGroupViewAdapter>.Instance;

	public override ICanvasGroupViewAdapter CombinationsCanvasGroup => NullViewAdapter<NullCanvasGroupViewAdapter>.Instance;

	public override IScrollviewViewAdapter Scrollview => NullScrollviewViewAdapter.Instance;

	public override IScrollviewViewAdapter SubcategoriesScrollview => NullScrollviewViewAdapter.Instance;

	public override ILabelViewAdapter CategoryLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;

	public override IGameObjectViewAdapter InfoContainer => NullObject<NullGameObjectViewAdapter>.Instance;

	public override ILabelViewAdapter InfoLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;

	public override ITransformViewAdapter CategoryButtonContainer => NullObject<NullTransformViewAdapter>.Instance;

	public override ILabelButtonViewAdapter NextCategoryButton => NullLabelButtonViewAdapter.Instance;

	public override ILabelButtonViewAdapter PreviousCategoryButton => NullLabelButtonViewAdapter.Instance;

	public override Texture2D BlurMaskTexture => Texture2D.blackTexture;

	public override IList<TMPCraftingSubcategoryButtonViewAdapter> SubcategoryButtons => new List<TMPCraftingSubcategoryButtonViewAdapter>();

	public override IList<CombinationButtonPresenter> CombinationButtons => new List<CombinationButtonPresenter>();

	public override void SetNavigation(CraftingSubcategory currentSubcategory)
	{
		Debug.LogError("NullCrafterMenuViewAdapter::SetNavigation:: Not implemented.");
	}

	public override TMPCraftingSubcategoryButtonViewAdapter CreateSubcategoryButton()
	{
		Debug.LogError("NullCrafterMenuViewAdapter::CreateSubcategoryButton:: Not implemented.");
		return null;
	}

	public override CombinationButtonPresenter CreateCombinationButton()
	{
		Debug.LogError("NullCrafterMenuViewAdapter::CreateCombinationButton:: Not implemented.");
		return null;
	}

	public override void DestroySubcategoryButtons()
	{
		Debug.LogError("NullCrafterMenuViewAdapter::DestroySubcategoryButtons:: Not implemented.");
	}

	public override void DestroyCombinationButtons()
	{
		Debug.LogError("NullCrafterMenuViewAdapter::DestroyCombinationButtons:: Not implemented.");
	}
}
