using System;
using System.Collections.Generic;
using System.Linq;
using Beam.Crafting;
using Beam.Events;
using Beam.Language;
using Beam.PostEffects;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beam.UI.Crafting;

public class CrafterMenuPresenter : ControllerBase, INeedPlayerComponent, ICloseOnFallthrough
{
	private const float FADE_TIME = 0.15f;

	[SerializeField]
	private CrafterMenuViewAdapterBase _view;

	private PinnedComboViewBase _pinnedView;

	[SerializeField]
	private QuickCrafterMenuPresenter _quickCrafter;

	[SerializeField]
	private CraftingMaterialTooltipPresenter _tooltipPresenter;

	private IPlayer _player;

	private IList<CraftingCategory> _categories;

	private CraftingCategory _nextCategory;

	private CraftingCategory _previousCategory;

	private CraftingCategory _currentCategory;

	private CraftingSubcategory _currentSubcategory;

	private CraftingCombination _currentCombination;

	private CraftingCombination _pinnedCombination;

	private IDictionary<CraftingType, IList<IBase>> _availableMaterials;

	private IMenuEffectsController _menuEffects;

	private Crafter _crafter;

	private Holder _holder;

	private bool _switchingCategory;

	private bool _switchingSubCategory;

	[SerializeField]
	private UnityEvent _opened;

	[SerializeField]
	private UnityEvent _onHighlightedKnife;

	[SerializeField]
	private UnityEvent _onPinnedKnife;

	IPlayer INeedPlayerComponent.Player => _player;

	public event EmptyEventHandler Opened;

	public event EmptyEventHandler Closed;

	public event EmptyEventHandler CategoryIncremented;

	public event EmptyEventHandler CategoryDecremented;

	protected virtual void OnOpened()
	{
		this.Opened?.Invoke();
		_opened?.Invoke();
		EventManager.RaiseEvent(new CrafterMenuOpenedEvent());
	}

	protected virtual void OnClosed()
	{
		this.Closed?.Invoke();
	}

	protected virtual void OnCategoryIncremented()
	{
		this.CategoryIncremented?.Invoke();
	}

	protected virtual void OnCategoryDecremented()
	{
		this.CategoryDecremented?.Invoke();
	}

	private void Start()
	{
		if (_view == null)
		{
			Debug.LogError("CrafterMenuPresenter:: No view component assigned!");
			_view = NullCrafterMenuViewAdapter.Instance;
		}
		_categories = _crafter.CraftingCombinations.Categories;
		_currentCategory = _categories.FirstOrDefault_NonAlloc();
		_currentSubcategory = _currentCategory.Subcategories.FirstOrDefault_NonAlloc();
		SetControlEvents();
		_quickCrafter.CrafterKeyFellThrough += QuickCrafter_CraftingKeyFellThrough;
		_crafter.Crafted += Crafter_Crafted;
		_crafter.MaterialsUpdated += Crafter_MaterialsUpdated;
		Hide();
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as CrafterMenuViewAdapterBase;
		if (_view == null)
		{
			Debug.LogError("CrafterMenuPresenter:: No view component assigned!");
			_view = NullCrafterMenuViewAdapter.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_crafter = _player.Crafter;
		_crafter.StartedPlacing += Crafter_StartedPlacing;
		_crafter.StoppedPlacing += Crafter_StoppedPlacing;
		_holder = _player.Holder;
		IMenuEffectsController component = _player.PlayerCamera.Camera.GetComponent<MainMenuEffectsController>();
		_menuEffects = component ?? NullObject<NullMenuEffectsController>.Instance;
		_player.PlayerSkills.SkillLevelIncreased += PlayerSkills_SkillLevelIncreased;
	}

	private void OnDestroy()
	{
		if (!_player.IsNullOrDestroyed() && (bool)_player.PlayerSkills)
		{
			_player.PlayerSkills.SkillLevelIncreased -= PlayerSkills_SkillLevelIncreased;
		}
		if ((bool)_quickCrafter)
		{
			_quickCrafter.CrafterKeyFellThrough -= QuickCrafter_CraftingKeyFellThrough;
		}
		if ((bool)_crafter)
		{
			_crafter.StartedPlacing -= Crafter_StartedPlacing;
			_crafter.StoppedPlacing -= Crafter_StoppedPlacing;
			_crafter.Crafted -= Crafter_Crafted;
			_crafter.MaterialsUpdated -= Crafter_MaterialsUpdated;
		}
		ClearControlEvents();
	}

	private void SetControlEvents()
	{
		_view.NextCategoryButton.Click += NextCategoryButton_Click;
		_view.PreviousCategoryButton.Click += PreviousCategoryButton_Click;
	}

	private void ClearControlEvents()
	{
		_view.NextCategoryButton.Click -= NextCategoryButton_Click;
		_view.PreviousCategoryButton.Click -= PreviousCategoryButton_Click;
	}

	private void Update()
	{
		if (_view.Visible)
		{
			if (_view.Visible && _player.Statistics.IsKnockedOut)
			{
				Hide(refresh: true);
			}
			if (_player.Input.GetButtonDown(31))
			{
				Hide(refresh: true);
			}
			if (_player.Input.GetButtonDown(35))
			{
				PreviousCategoryButton_Click(null);
			}
			if (_player.Input.GetButtonDown(36))
			{
				NextCategoryButton_Click(null);
			}
			if (_player.Input.GetButtonDown(78))
			{
				PinCurrentCombination();
			}
		}
	}

	private void QuickCrafter_CraftingKeyFellThrough()
	{
		if (_view.Visible)
		{
			Hide(refresh: true);
		}
		else
		{
			Show();
		}
	}

	private void Crafter_StartedPlacing()
	{
		if ((bool)_pinnedView && _pinnedView.Visible && _pinnedCombination != null)
		{
			_pinnedView.Hide();
		}
	}

	private void Crafter_StoppedPlacing()
	{
		if ((bool)_pinnedView && !_pinnedView.Visible && _pinnedCombination != null)
		{
			_pinnedView.Show();
			_crafter.PollMaterials();
		}
	}

	private void Crafter_Crafted(CraftingCombination combination)
	{
		if (combination == _pinnedCombination)
		{
			_pinnedView?.Hide();
			_pinnedCombination = null;
		}
	}

	private void Crafter_MaterialsUpdated(IDictionary<CraftingType, IList<IBase>> materials)
	{
		if (_pinnedCombination == null)
		{
			return;
		}
		foreach (MaterialInfo material in _pinnedCombination.GetInfo().Materials)
		{
			material.Available.Value = (materials.TryGetValue(material.Type, out var value) ? value.Count : 0);
		}
	}

	public void Show()
	{
		if (_player.Statistics.IsKnockedOut || MainMenuPresenter.Instance.IsGamePaused || _player.Movement.IsInCutscene)
		{
			return;
		}
		CraftingCategory category = _currentCategory;
		if (_holder.CurrentObject.IsNullOrDestroyed())
		{
			category = _categories.FirstOrDefault_NonAlloc();
		}
		else if (_holder.CurrentObject.CraftingType != _currentCategory.ToolRequired)
		{
			category = _categories.FirstOrDefault((CraftingCategory cat) => cat.ToolRequired == _holder.CurrentObject.CraftingType) ?? _categories.FirstOrDefault_NonAlloc();
		}
		_menuEffects.SetMaskTexture(_view.BlurMaskTexture);
		_menuEffects.Show();
		OnOpened();
		Show(category);
		FadeCanvasGroup_OnBegin();
		FadeIn(FadeCanvasGroup_OnUpdate, FadeCanvasGroup_OnComplete, 0.25f);
	}

	public void Show(CraftingCategory category)
	{
		bool flag = _currentCategory != category;
		_currentCategory = category;
		bool flag2 = true;
		if (!category.ToolRequired.Equals(AttributeType.None, InteractiveType.None) && (_holder.CurrentObject.IsNullOrDestroyed() || (!_holder.CurrentObject.IsNullOrDestroyed() && _holder.CurrentObject.CraftingType != category.ToolRequired)))
		{
			flag2 = false;
			IPickupable pickupable = _holder.Storage.Get<IPickupable>(category.ToolRequired);
			if (!pickupable.IsNullOrDestroyed())
			{
				flag2 = _holder.ReplicatedSelect(pickupable);
			}
		}
		if (!flag2)
		{
			_view.InfoLabel.Text = Localization.GetLanguageHandler().Localize("CRAFTING_MENU_TOOL_REQUIRED", category.ToolRequired.ToString(), category.Term);
		}
		_availableMaterials = _crafter.ReplicatedGetCraftingMaterials();
		_view.CategoryLabel.Text = category.Term;
		_view.InfoContainer.Active = !flag2;
		_view.CombinationsCanvasGroup.Interactable = flag2;
		PopulateCategoryButtons();
		CreateSubcategoryButtons();
		if (flag)
		{
			ResetSubcategoryScrollview();
			ResetCombinationScrollview();
			_currentSubcategory = null;
		}
		_currentSubcategory = _currentSubcategory ?? _currentCategory.Subcategories.FirstOrDefault_NonAlloc();
		PopulateSubcategory();
		if (!_view.Visible)
		{
			_player.Input.SetAndCacheMapsEnabled(state: false, 0);
			_player.Input.SetAndCacheMapsEnabled(state: true, 4);
			_player.Input.SetCursorEnabled(enabled: true);
			_player.PlayerUI.Refresh(RefreshUIMode.Clear);
			_view.Show();
			_quickCrafter.CanOpen = false;
		}
		((IButtonViewAdapter)GetSubcategoryButton()).Select();
	}

	public void Hide(bool refresh = false)
	{
		if (_view.Visible)
		{
			_player.Input.SetAndCacheMapsEnabled(state: true, 0);
			_player.Input.SetAndCacheMapsEnabled(state: false, 4);
			_player.Input.SetCursorEnabled(enabled: false);
			if (refresh)
			{
				_player.PlayerUI.Refresh(RefreshUIMode.Refresh);
			}
			OnClosed();
			_tooltipPresenter.Hide();
			_menuEffects.Hide();
			_view.Hide();
			_quickCrafter.CanOpen = true;
		}
	}

	void ICloseOnFallthrough.Close()
	{
		Hide(refresh: true);
	}

	public TMPCraftingSubcategoryButtonViewAdapter GetSubcategoryButton()
	{
		foreach (TMPCraftingSubcategoryButtonViewAdapter subcategoryButton in _view.SubcategoryButtons)
		{
			if (subcategoryButton.Name == _currentSubcategory.Name)
			{
				return subcategoryButton;
			}
		}
		return _view.SubcategoryButtons.FirstOrDefault_NonAlloc();
	}

	private void PopulateCategoryButtons()
	{
		int num = _categories.IndexOf(_currentCategory);
		int num2 = num + 1;
		if (num2 > _categories.Count - 1)
		{
			num2 = 0;
		}
		int num3 = num - 1;
		if (num3 < 0)
		{
			num3 = _categories.Count - 1;
		}
		_nextCategory = _categories[num2];
		_previousCategory = _categories[num3];
		_view.NextCategoryButton.Text = _nextCategory.Term;
		_view.PreviousCategoryButton.Text = _previousCategory.Term;
	}

	private void CreateSubcategoryButtons()
	{
		_view.DestroySubcategoryButtons();
		using (new UserInterfaceContext(_player.PlayerUI.UserInterface))
		{
			foreach (CraftingSubcategory subcategory in _currentCategory.Subcategories)
			{
				TMPCraftingSubcategoryButtonViewAdapter tMPCraftingSubcategoryButtonViewAdapter = _view.CreateSubcategoryButton();
				tMPCraftingSubcategoryButtonViewAdapter.Initialize(subcategory);
				tMPCraftingSubcategoryButtonViewAdapter.Click += SubcategoryButton_Click;
				tMPCraftingSubcategoryButtonViewAdapter.Selected += SubcategoryButton_Selected;
				tMPCraftingSubcategoryButtonViewAdapter.Deselected += SubcategoryButton_Deselected;
				tMPCraftingSubcategoryButtonViewAdapter.MouseEnter += SubcategoryButton_MouseEnter;
				tMPCraftingSubcategoryButtonViewAdapter.MouseLeave += SubcategoryButton_MouseLeave;
			}
		}
	}

	private void PopulateSubcategory()
	{
		CreateCombinations(_currentSubcategory.Combinations);
		_view.SetNavigation(_currentSubcategory);
	}

	private void CreateCombinations(IEnumerable<CraftingCombination> combinations)
	{
		_currentCombination = null;
		_view.DestroyCombinationButtons();
		using (new UserInterfaceContext(_player.PlayerUI.UserInterface))
		{
			foreach (CraftingCombination combination in combinations)
			{
				if (combination.IsDynamic)
				{
					continue;
				}
				foreach (MaterialInfo material in combination.GetInfo().Materials)
				{
					material.Available.Value = (_availableMaterials.TryGetValue(material.Type, out var value) ? value.Count : 0);
				}
				CombinationButtonPresenter combinationButtonPresenter = _view.CreateCombinationButton();
				combinationButtonPresenter.Initialize(combination, _tooltipPresenter, _view.CombinationsCanvasGroup.Interactable);
				combinationButtonPresenter.SetCraftsmanshipLevel(_player.PlayerSkills.CraftsmanshipLevel);
				combinationButtonPresenter.Click += CombinationButton_Click;
				combinationButtonPresenter.Selected += CombinationButton_Selected;
				combinationButtonPresenter.MouseEnter += CombinationButton_MouseEnter;
				combinationButtonPresenter.MouseLeave += CombinationButton_MouseLeave;
			}
		}
	}

	private void CombinationButton_Click(CombinationButtonPresenter sender)
	{
		Hide();
		_crafter.ReplicatedCraft(sender.Combination);
	}

	private void CombinationButton_SecondaryClick(CombinationButtonPresenter sender)
	{
		PinCurrentCombination();
	}

	private void CombinationButton_Selected(CombinationButtonPresenter sender)
	{
		_currentCombination = sender.Combination;
		_view.Scrollview.FocusElement(sender.transform);
		if (_currentCombination.CraftingType.InteractiveType == InteractiveType.TOOLS_KNIFE)
		{
			_onHighlightedKnife?.Invoke();
		}
	}

	private void CombinationButton_MouseEnter(CombinationButtonPresenter sender, PointerEventData eventData)
	{
		_currentCombination = sender.Combination;
		string text = Localization.GetLanguageHandler().Localize("CRAFTING_MENU_PIN");
		text = PlatformLocalization.GetHandler().SubstituteControllerGlyphs(text, _player.Id, 78);
		_tooltipPresenter.Show(text, CraftingTooltipType.Info, eventData);
		if (_currentCombination.CraftingType.InteractiveType == InteractiveType.TOOLS_KNIFE)
		{
			_onHighlightedKnife?.Invoke();
		}
	}

	private void CombinationButton_MouseLeave(CombinationButtonPresenter sender, PointerEventData eventData)
	{
		_tooltipPresenter.Hide();
		_currentCombination = null;
	}

	private void SubcategoryButton_Selected(object sender)
	{
		TMPCraftingSubcategoryButtonViewAdapter tMPCraftingSubcategoryButtonViewAdapter = sender as TMPCraftingSubcategoryButtonViewAdapter;
		_tooltipPresenter.Show(tMPCraftingSubcategoryButtonViewAdapter.Subcategory.Term, CraftingTooltipType.Info, tMPCraftingSubcategoryButtonViewAdapter.transform.position);
	}

	private void SubcategoryButton_Deselected(TMPCraftingSubcategoryButtonViewAdapter sender)
	{
		_tooltipPresenter.Hide();
	}

	private void SubcategoryButton_MouseEnter(TMPCraftingSubcategoryButtonViewAdapter sender, PointerEventData eventData)
	{
		_tooltipPresenter.Show(sender.Subcategory.Term, CraftingTooltipType.Info, eventData);
	}

	private void SubcategoryButton_MouseLeave(TMPCraftingSubcategoryButtonViewAdapter sender, PointerEventData eventData)
	{
		_tooltipPresenter.Hide();
	}

	private void SubcategoryButton_Click(object sender)
	{
		if (!_switchingCategory)
		{
			FadeCombinationCanvasGroup_OnBegin();
			FadeOut(FadeCombinationCanvasGroup_OnUpdate, delegate
			{
				_currentSubcategory = (CraftingSubcategory)sender;
				PopulateSubcategory();
				FadeIn(FadeCombinationCanvasGroup_OnUpdate, FadeCombinationCanvasGroup_OnComplete, 0.15f);
			}, 0.15f);
		}
	}

	private void NextCategoryButton_Click(object sender)
	{
		if (!_switchingSubCategory)
		{
			FadeCategoryCanvasGroup_OnBegin();
			FadeOut(FadeCategoryCanvasGroup_OnUpdate, delegate
			{
				CraftingCategory nextCategory = _nextCategory;
				Show(nextCategory);
				FadeIn(FadeCategoryCanvasGroup_OnUpdate, FadeCategoryCanvasGroup_OnComplete, 0.15f);
			}, 0.15f);
			OnCategoryIncremented();
		}
	}

	private void PreviousCategoryButton_Click(object sender)
	{
		if (!_switchingSubCategory)
		{
			FadeCategoryCanvasGroup_OnBegin();
			FadeOut(FadeCategoryCanvasGroup_OnUpdate, delegate
			{
				CraftingCategory previousCategory = _previousCategory;
				Show(previousCategory);
				FadeIn(FadeCategoryCanvasGroup_OnUpdate, FadeCategoryCanvasGroup_OnComplete, 0.15f);
			}, 0.15f);
			OnCategoryDecremented();
		}
	}

	private void PinCurrentCombination()
	{
		if (_pinnedView == null)
		{
			_pinnedView = _player.PlayerUI.Canvas.GetComponentInChildren<PinnedComboViewBase>();
		}
		if (_pinnedView == null || _currentCombination == null || _currentCombination.IsLocked || _currentCombination.IsDynamic)
		{
			return;
		}
		_pinnedView.Hide();
		if (_pinnedCombination == _currentCombination)
		{
			_pinnedCombination = null;
			_crafter.EndPollMaterials();
			return;
		}
		_pinnedCombination = _currentCombination;
		if (_pinnedCombination != null)
		{
			_pinnedView.SetCombo(_pinnedCombination.GetInfo());
			_pinnedView.SetCurrentCraftsmanshipLevel(_player.PlayerSkills.CraftsmanshipLevel);
			_pinnedView.SetRequiredCraftsmanshipLevel(_pinnedCombination.CraftsmanshipLevelRequired);
			_pinnedView.Show();
			_crafter.BeginPollMaterials();
			if (_pinnedCombination.CraftingType.InteractiveType == InteractiveType.TOOLS_KNIFE)
			{
				_onPinnedKnife?.Invoke();
			}
		}
	}

	private void PlayerSkills_SkillLevelIncreased(object sender, EventArgs e)
	{
		if (_pinnedCombination != null && _pinnedView.Visible)
		{
			PlayerSkills playerSkills = sender as PlayerSkills;
			if (sender != null)
			{
				_pinnedView.SetCurrentCraftsmanshipLevel(playerSkills.CraftsmanshipLevel);
			}
		}
	}

	private void FadeIn(Action<float> onUpdate, Action onComplete, float time)
	{
		LeanTween.cancel(base.gameObject);
		LeanTween.value(base.gameObject, onUpdate, 0f, 1f, time).setDelay(0.1f).setEase(LeanTweenType.easeOutCubic)
			.setOnComplete(onComplete)
			.setIgnoreTimeScale(useUnScaledTime: true);
	}

	private void FadeOut(Action<float> onUpdate, Action onComplete, float time)
	{
		LeanTween.cancel(base.gameObject);
		LeanTween.value(base.gameObject, onUpdate, 1f, 0f, time).setEase(LeanTweenType.easeOutCubic).setOnComplete(onComplete)
			.setIgnoreTimeScale(useUnScaledTime: true);
	}

	private void FadeCanvasGroup_OnBegin()
	{
		_view.CanvasGroup.Alpha = 0f;
	}

	private void FadeCanvasGroup_OnUpdate(float value)
	{
		_view.CanvasGroup.Alpha = value;
	}

	private void FadeCanvasGroup_OnComplete()
	{
	}

	private void FadeCategoryCanvasGroup_OnBegin()
	{
		_switchingCategory = true;
	}

	private void FadeCategoryCanvasGroup_OnUpdate(float value)
	{
		_view.CategoryCanvasGroup.Alpha = value;
	}

	private void FadeCategoryCanvasGroup_OnComplete()
	{
		_switchingCategory = false;
	}

	private void FadeCombinationCanvasGroup_OnBegin()
	{
		_switchingSubCategory = true;
	}

	private void FadeCombinationCanvasGroup_OnUpdate(float value)
	{
		_view.CombinationsCanvasGroup.Alpha = value;
	}

	private void FadeCombinationCanvasGroup_OnComplete()
	{
		_switchingSubCategory = false;
	}

	private void ResetCombinationScrollview()
	{
		_view.Scrollview.NormalizedVerticalPosition = 1f;
	}

	private void ResetSubcategoryScrollview()
	{
		_view.SubcategoriesScrollview.NormalizedHorizontalPosition = 1f;
	}
}
