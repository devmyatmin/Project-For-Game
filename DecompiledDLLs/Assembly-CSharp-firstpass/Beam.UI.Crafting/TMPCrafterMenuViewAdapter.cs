using System.Collections.Generic;
using System.Linq;
using Beam.Crafting;
using Beam.Utilities;
using UnityEngine;

namespace Beam.UI.Crafting;

public class TMPCrafterMenuViewAdapter : CrafterMenuViewAdapterBase
{
	[SerializeField]
	private GameObject _crafterPanel;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private UCanvasGroupViewAdapter _categoryCanvasGroup;

	[SerializeField]
	private UCanvasGroupViewAdapter _combinationsCanvasGroup;

	[SerializeField]
	private UScrollviewViewAdapter _scrollview;

	[SerializeField]
	private UScrollviewViewAdapter _subcategoriesScrollview;

	[SerializeField]
	private TMPLabelViewAdapter _categoryLabel;

	[SerializeField]
	private GameObjectViewAdapter _infoContainer;

	[SerializeField]
	private TMPLabelViewAdapter _infoLabel;

	[SerializeField]
	private TransformViewAdapter _categoryButtonContainer;

	[SerializeField]
	private TMPLabelButtonViewAdapter _nextCategoryButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _previousCategoryButton;

	[SerializeField]
	private TMPCraftingSubcategoryButtonViewAdapter _subcategoryButtonPrefab;

	[SerializeField]
	private Transform _subcategoriesContainer;

	[SerializeField]
	private CombinationButtonPresenter _combinationButtonPrefab;

	[SerializeField]
	private Transform _combinationsContainer;

	[SerializeField]
	private Texture2D _blurMaskTexture;

	private List<TMPCraftingSubcategoryButtonViewAdapter> _subcategoryButtons = new List<TMPCraftingSubcategoryButtonViewAdapter>();

	private List<CombinationButtonPresenter> _combinationButtons = new List<CombinationButtonPresenter>();

	public override ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public override ICanvasGroupViewAdapter CategoryCanvasGroup => _categoryCanvasGroup;

	public override ICanvasGroupViewAdapter CombinationsCanvasGroup => _combinationsCanvasGroup;

	public override IScrollviewViewAdapter Scrollview => _scrollview;

	public override IScrollviewViewAdapter SubcategoriesScrollview => _subcategoriesScrollview;

	public override ILabelViewAdapter CategoryLabel => _categoryLabel;

	public override IGameObjectViewAdapter InfoContainer => _infoContainer;

	public override ILabelViewAdapter InfoLabel => _infoLabel;

	public override ITransformViewAdapter CategoryButtonContainer => _categoryButtonContainer;

	public override ILabelButtonViewAdapter NextCategoryButton => _nextCategoryButton;

	public override ILabelButtonViewAdapter PreviousCategoryButton => _previousCategoryButton;

	public override Texture2D BlurMaskTexture => _blurMaskTexture;

	public override IList<TMPCraftingSubcategoryButtonViewAdapter> SubcategoryButtons => _subcategoryButtons;

	public override IList<CombinationButtonPresenter> CombinationButtons => _combinationButtons;

	public override void Show()
	{
		_crafterPanel.SetActive(true);
		base.Show();
	}

	public override void Hide()
	{
		_crafterPanel.SetActive(false);
		DestroySubcategoryButtons();
		DestroyCombinationButtons();
		base.Hide();
	}

	public override TMPCraftingSubcategoryButtonViewAdapter CreateSubcategoryButton()
	{
		TMPCraftingSubcategoryButtonViewAdapter tMPCraftingSubcategoryButtonViewAdapter = Object.Instantiate<TMPCraftingSubcategoryButtonViewAdapter>(_subcategoryButtonPrefab);
		((Component)tMPCraftingSubcategoryButtonViewAdapter).transform.SetParent(((Component)_subcategoriesContainer).transform, false);
		((Component)tMPCraftingSubcategoryButtonViewAdapter).transform.SetAsFirstSibling();
		_subcategoryButtons.Add(tMPCraftingSubcategoryButtonViewAdapter);
		return tMPCraftingSubcategoryButtonViewAdapter;
	}

	public override CombinationButtonPresenter CreateCombinationButton()
	{
		CombinationButtonPresenter combinationButtonPresenter = Object.Instantiate<CombinationButtonPresenter>(_combinationButtonPrefab);
		((Component)combinationButtonPresenter).transform.SetParent(((Component)_combinationsContainer).transform, false);
		_combinationButtons.Add(combinationButtonPresenter);
		return combinationButtonPresenter;
	}

	public override void DestroySubcategoryButtons()
	{
		BeamTeam.DestroyAllChildrenImmediate(((Component)_subcategoriesContainer).transform);
		_subcategoryButtons.Clear();
	}

	public override void DestroyCombinationButtons()
	{
		BeamTeam.DestroyAllChildren(((Component)_combinationsContainer).transform);
		_combinationButtons.Clear();
	}

	public override void SetNavigation(CraftingSubcategory currentSubcategory)
	{
		TMPCraftingSubcategoryButtonViewAdapter? tMPCraftingSubcategoryButtonViewAdapter = _subcategoryButtons.FirstOrDefault((TMPCraftingSubcategoryButtonViewAdapter subcategoryButton) => subcategoryButton.Subcategory.Name == currentSubcategory?.Name);
		Transform val = ((tMPCraftingSubcategoryButtonViewAdapter != null) ? ((Component)tMPCraftingSubcategoryButtonViewAdapter).transform : null);
		CombinationButtonPresenter? combinationButtonPresenter = _combinationButtons.FirstOrDefault((CombinationButtonPresenter combinationButton) => combinationButton.Combination.Unlocked);
		Transform val2 = ((combinationButtonPresenter != null) ? ((Component)combinationButtonPresenter).transform : null);
		for (int num = 0; num < _subcategoryButtons.Count; num++)
		{
			TMPCraftingSubcategoryButtonViewAdapter tMPCraftingSubcategoryButtonViewAdapter2 = _subcategoryButtons[num];
			((Component)tMPCraftingSubcategoryButtonViewAdapter2).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_nextCategoryButton).transform);
			((Component)tMPCraftingSubcategoryButtonViewAdapter2).transform.SetNavigation<Transform>(NavigationDirection.Down, (CombinationsCanvasGroup.Interactable && (Object)(object)val2 != (Object)null) ? val2 : null);
			int num2 = num + 1;
			TMPCraftingSubcategoryButtonViewAdapter element = ((num2 < _subcategoryButtons.Count) ? _subcategoryButtons[num2] : null);
			tMPCraftingSubcategoryButtonViewAdapter2.SetNavigation<TMPCraftingSubcategoryButtonViewAdapter>(NavigationDirection.Left, element);
			int num3 = num - 1;
			TMPCraftingSubcategoryButtonViewAdapter element2 = ((num3 >= 0) ? _subcategoryButtons[num3] : null);
			tMPCraftingSubcategoryButtonViewAdapter2.SetNavigation<TMPCraftingSubcategoryButtonViewAdapter>(NavigationDirection.Right, element2);
		}
		for (int num4 = 0; num4 < _combinationButtons.Count; num4++)
		{
			CombinationButtonPresenter combinationButtonPresenter2 = _combinationButtons[num4];
			int previousCombination = GetPreviousCombination(_combinationButtons, num4);
			global::ExtensionMethods.SetNavigation<Transform>(element: (previousCombination != -1) ? ((Component)_combinationButtons[previousCombination]).transform : val, component: ((Component)combinationButtonPresenter2).transform, direction: NavigationDirection.Up);
			int nextCombination = GetNextCombination(_combinationButtons, num4);
			global::ExtensionMethods.SetNavigation<Transform>(element: (nextCombination != -1) ? ((Component)_combinationButtons[nextCombination]).transform : null, component: ((Component)combinationButtonPresenter2).transform, direction: NavigationDirection.Down);
		}
		((Component)_nextCategoryButton).transform.SetNavigation<Transform>(NavigationDirection.Left, ((Component)_previousCategoryButton).transform);
		((Component)_nextCategoryButton).transform.SetNavigation<Transform>(NavigationDirection.Down, val);
		((Component)_previousCategoryButton).transform.SetNavigation<Transform>(NavigationDirection.Right, ((Component)_nextCategoryButton).transform);
		((Component)_previousCategoryButton).transform.SetNavigation<Transform>(NavigationDirection.Down, val);
	}

	private int GetNextCombination(IList<CombinationButtonPresenter> combinationButtons, int startingIndex)
	{
		int num = startingIndex + 1;
		if (num < combinationButtons.Count)
		{
			if (!combinationButtons[num].Combination.Unlocked)
			{
				return GetNextCombination(combinationButtons, num);
			}
			return num;
		}
		return -1;
	}

	private int GetPreviousCombination(IList<CombinationButtonPresenter> combinationButtons, int startingIndex)
	{
		int num = startingIndex - 1;
		if (num >= 0)
		{
			if (!combinationButtons[num].Combination.Unlocked)
			{
				return GetPreviousCombination(combinationButtons, num);
			}
			return num;
		}
		return -1;
	}
}
