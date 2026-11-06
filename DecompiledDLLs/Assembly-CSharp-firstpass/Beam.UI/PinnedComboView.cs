using System.Collections.Generic;
using Beam.UI.Crafting;
using Beam.Utilities;
using Beam.Utilities.UGUI;
using UnityEngine;

namespace Beam.UI;

public class PinnedComboView : PinnedComboViewBase
{
	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private TMPLabelViewAdapter _nameLabel;

	[SerializeField]
	private GraphicsGroup _craftsmanshipLevelGroup;

	[SerializeField]
	private TMPLabelViewAdapter _craftsmanshipLevelLabel;

	[SerializeField]
	private UImageViewAdapter _comboImage;

	[SerializeField]
	private GameObject _materialContainer;

	[SerializeField]
	private CombinationMaterialPresenter _materialElement;

	private int _currentLvl;

	private int _requiredLvl;

	private void Awake()
	{
		Visible = _container.gameObject.activeSelf;
	}

	public override void SetCombo(CombinationInfo combinationInfo)
	{
		SetTitle(combinationInfo.Term);
		SetSprite(combinationInfo.SpriteAssetPath);
		SetMaterials(combinationInfo.Materials);
		SetCurrentCraftsmanshipLevel(0);
		SetRequiredCraftsmanshipLevel(0);
	}

	public override void SetCurrentCraftsmanshipLevel(int level)
	{
		_currentLvl = level;
		CheckCraftsmanshipLevel();
	}

	public override void SetRequiredCraftsmanshipLevel(int level)
	{
		_requiredLvl = level;
		_craftsmanshipLevelLabel.Text = _requiredLvl.ToString();
		((Component)_craftsmanshipLevelGroup).gameObject.SetActive(level > 0);
		CheckCraftsmanshipLevel();
	}

	private void CheckCraftsmanshipLevel()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		_craftsmanshipLevelGroup.SetColor((_currentLvl >= _requiredLvl) ? Color.white : Color32.op_Implicit(UGUIColor.TEXT_RED));
	}

	private void SetTitle(string title)
	{
		_nameLabel.Text = title;
	}

	private void SetSprite(string spriteAssetPath)
	{
		_comboImage.Sprite = Resources.Load<Sprite>(spriteAssetPath);
	}

	private void SetMaterials(IEnumerable<MaterialInfo> materials)
	{
		BeamTeam.DestroyAllChildren(_materialContainer.transform);
		foreach (MaterialInfo material in materials)
		{
			CombinationMaterialPresenter combinationMaterialPresenter = Object.Instantiate<CombinationMaterialPresenter>(_materialElement);
			((Component)combinationMaterialPresenter).transform.SetParent(_materialContainer.transform, false);
			((Component)combinationMaterialPresenter).transform.SetAsFirstSibling();
			combinationMaterialPresenter.SetMaterialQtyDisplayMode(EMaterialQtyDisplayMode.AvailableAndRequired);
			combinationMaterialPresenter.Initialize(material);
		}
	}

	public override void Show()
	{
		_container.SetActive(true);
		base.Show();
	}

	public override void Hide()
	{
		_container.SetActive(false);
		base.Hide();
	}
}
