using System;
using Beam.Crafting;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI.Crafting;

public class CombinationButtonPresenter : MonoBehaviour, ISelectHandler, IEventSystemHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private CombinationButtonViewAdapterBase _view;

	private CraftingCombination _combination;

	private CraftingMaterialTooltipPresenter _tooltipPresenter;

	private bool _enabled;

	private bool _valid;

	public CraftingCombination Combination => _combination;

	public event Action<CombinationButtonPresenter> Click;

	public event Action<CombinationButtonPresenter> SecondaryClick;

	public event Action<CombinationButtonPresenter, PointerEventData> MouseEnter;

	public event Action<CombinationButtonPresenter, PointerEventData> MouseLeave;

	public event Action<CombinationButtonPresenter> Selected;

	public void Initialize(CraftingCombination combination, CraftingMaterialTooltipPresenter tooltipPresenter, bool enabled)
	{
		_tooltipPresenter = tooltipPresenter;
		_combination = combination;
		_enabled = enabled;
		_view.NameLabel.Text = _combination.Term;
		_view.Icon.Sprite = _combination.Sprite;
		bool isLocked = _combination.IsLocked;
		bool isLimited = combination.IsLimited;
		_view.Icon.Enabled = !isLocked;
		_view.LockedIcon.Enabled = isLocked;
		_view.CraftedIcon.Enabled = _combination.CraftedCount > 0;
		_view.CanvasGroup.Interactable = !isLocked && !isLimited;
		_view.Button.Click += Button_Click;
		CreateMaterials();
	}

	public void SetCraftsmanshipLevel(int level)
	{
		if (_combination.CraftsmanshipLevelRequired > level)
		{
			_view.CraftsmanshipLevelLabel.Text = _combination.CraftsmanshipLevelRequired.ToString();
			_view.CraftsmanshipLevelGroup.SetActive(true);
			_view.CanvasGroup.Interactable = false;
		}
		else
		{
			_view.CraftsmanshipLevelGroup.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		_tooltipPresenter.Hide();
		Click = null;
		SecondaryClick = null;
		Selected = null;
		MouseEnter = null;
		MouseLeave = null;
	}

	private void CreateMaterials()
	{
		BeamTeam.DestroyAllChildren(_view.MaterialsContainer.transform);
		CombinationInfo info = _combination.GetInfo();
		foreach (MaterialInfo material in info.Materials)
		{
			CombinationMaterialPresenter combinationMaterialPresenter = Object.Instantiate<CombinationMaterialPresenter>(_view.MaterialPrefab);
			((Component)combinationMaterialPresenter).transform.SetParent(_view.MaterialsContainer.transform, false);
			((Component)combinationMaterialPresenter).transform.SetAsFirstSibling();
			combinationMaterialPresenter.Initialize(material);
			combinationMaterialPresenter.Click += CombinationButtonMaterial_Click;
			combinationMaterialPresenter.MouseEnter += CombinationButtonMaterial_MouseEnter;
			combinationMaterialPresenter.MouseLeave += CombinationButtonMaterial_MouseLeave;
		}
		_valid = Beam.Utilities.ExtensionMethods.All_NonAlloc(info.Materials, (MaterialInfo m) => m.Available.Value >= m.Required);
	}

	public void OnSelect(BaseEventData eventData)
	{
		Selected?.Invoke(this);
	}

	public void Button_Click(object sender)
	{
		if (_enabled && _valid && _view.CanvasGroup.Interactable && !_combination.IsLocked && !_combination.IsLimited)
		{
			Click?.Invoke(this);
		}
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)eventData.button == 1)
		{
			SecondaryClick?.Invoke(this);
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		MouseEnter?.Invoke(this, eventData);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		MouseLeave?.Invoke(this, eventData);
	}

	private void CombinationButtonMaterial_Click(CombinationMaterialPresenter sender)
	{
		Button_Click(sender);
	}

	private void CombinationButtonMaterial_MouseEnter(CombinationMaterialPresenter sender, PointerEventData eventData)
	{
		_tooltipPresenter.Show(sender.Name, CraftingTooltipType.Material, eventData);
	}

	private void CombinationButtonMaterial_MouseLeave(CombinationMaterialPresenter sender, PointerEventData eventData)
	{
		_tooltipPresenter.Hide(CraftingTooltipType.Material);
	}
}
