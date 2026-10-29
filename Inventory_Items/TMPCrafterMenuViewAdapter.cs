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
		_crafterPanel.SetActive(value: true);
		base.Show();
	}

	public override void Hide()
	{
		_crafterPanel.SetActive(value: false);
		DestroySubcategoryButtons();
		DestroyCombinationButtons();
		base.Hide();
	}

	public override TMPCraftingSubcategoryButtonViewAdapter CreateSubcategoryButton()
	{
		TMPCraftingSubcategoryButtonViewAdapter tMPCraftingSubcategoryButtonViewAdapter = Object.Instantiate(_subcategoryButtonPrefab);
		tMPCraftingSubcategoryButtonViewAdapter.transform.SetParent(_subcategoriesContainer.transform, worldPositionStays: false);
		tMPCraftingSubcategoryButtonViewAdapter.transform.SetAsFirstSibling();
		_subcategoryButtons.Add(tMPCraftingSubcategoryButtonViewAdapter);
		return tMPCraftingSubcategoryButtonViewAdapter;
	}

	public override CombinationButtonPresenter CreateCombinationButton()
	{
		CombinationButtonPresenter combinationButtonPresenter = Object.Instantiate(_combinationButtonPrefab);
		combinationButtonPresenter.transform.SetParent(_combinationsContainer.transform, worldPositionStays: false);
		_combinationButtons.Add(combinationButtonPresenter);
		return combinationButtonPresenter;
	}

	public override void DestroySubcategoryButtons()
	{
		BeamTeam.DestroyAllChildrenImmediate(_subcategoriesContainer.transform);
		_subcategoryButtons.Clear();
	}

	public override void DestroyCombinationButtons()
	{
		BeamTeam.DestroyAllChildren(_combinationsContainer.transform);
		_combinationButtons.Clear();
	}

	public override void SetNavigation(CraftingSubcategory currentSubcategory)
	{
		Transform transform = _subcategoryButtons.FirstOrDefault((TMPCraftingSubcategoryButtonViewAdapter subcategoryButton) => subcategoryButton.Subcategory.Name == currentSubcategory?.Name)?.transform;
		Transform transform2 = _combinationButtons.FirstOrDefault((CombinationButtonPresenter combinationButton) => combinationButton.Combination.Unlocked)?.transform;
		for (int i = 0; i < _subcategoryButtons.Count; i++)
		{
			TMPCraftingSubcategoryButtonViewAdapter tMPCraftingSubcategoryButtonViewAdapter = _subcategoryButtons[i];
			tMPCraftingSubcategoryButtonViewAdapter.transform.SetNavigation(NavigationDirection.Up, _nextCategoryButton.transform);
			tMPCraftingSubcategoryButtonViewAdapter.transform.SetNavigation(NavigationDirection.Down, (CombinationsCanvasGroup.Interactable && transform2 != null) ? transform2 : null);
			int num = i + 1;
			TMPCraftingSubcategoryButtonViewAdapter element = ((num < _subcategoryButtons.Count) ? _subcategoryButtons[num] : null);
			tMPCraftingSubcategoryButtonViewAdapter.SetNavigation(NavigationDirection.Left, element);
			int num2 = i - 1;
			TMPCraftingSubcategoryButtonViewAdapter element2 = ((num2 >= 0) ? _subcategoryButtons[num2] : null);
			tMPCraftingSubcategoryButtonViewAdapter.SetNavigation(NavigationDirection.Right, element2);
		}
		for (int j = 0; j < _combinationButtons.Count; j++)
		{
			CombinationButtonPresenter combinationButtonPresenter = _combinationButtons[j];
			int previousCombination = GetPreviousCombination(_combinationButtons, j);
			global::ExtensionMethods.SetNavigation(element: (previousCombination != -1) ? _combinationButtons[previousCombination].transform : transform, component: combinationButtonPresenter.transform, direction: NavigationDirection.Up);
			int nextCombination = GetNextCombination(_combinationButtons, j);
			global::ExtensionMethods.SetNavigation(element: (nextCombination != -1) ? _combinationButtons[nextCombination].transform : null, component: combinationButtonPresenter.transform, direction: NavigationDirection.Down);
		}
		_nextCategoryButton.transform.SetNavigation(NavigationDirection.Left, _previousCategoryButton.transform);
		_nextCategoryButton.transform.SetNavigation(NavigationDirection.Down, transform);
		_previousCategoryButton.transform.SetNavigation(NavigationDirection.Right, _nextCategoryButton.transform);
		_previousCategoryButton.transform.SetNavigation(NavigationDirection.Down, transform);
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
