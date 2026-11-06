using System.Collections.Generic;
using System.Linq;
using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class UInputMenuViewAdapter : ViewAdapterBase, IInputMenuViewAdapter, IOptionsPanelView, IViewAdapter
{
	[SerializeField]
	private GameObject _inputMenuGameObject;

	[SerializeField]
	private GameObject _inputMenuButton;

	[SerializeField]
	private Transform _categoriesContainer;

	[SerializeField]
	private TMPLabelButtonViewAdapter _defaultButton;

	[SerializeField]
	private UScrollviewViewAdapter _scrollview;

	[SerializeField]
	private TMPOptionButtonViewAdapter _inputOptionButtonPrefab;

	[SerializeField]
	private TMPOptionSliderViewAdapter _inputOptionSliderPrefab;

	[SerializeField]
	private TMPInputMapCategoryHeader _inputMapCategorySubheaderPrefab;

	[SerializeField]
	private TMPOptionInputFieldButtonViewAdapter _inputFieldPrefab;

	[SerializeField]
	private USelectableViewAdapter _controllerLayoutImagePrefab;

	private List<IOptionButtonViewAdapter> _inputOptionButtons = new List<IOptionButtonViewAdapter>();

	private List<IOptionSliderViewAdapter> _inputOptionSliders = new List<IOptionSliderViewAdapter>();

	private List<IOptionInputFieldButtonViewAdapter> _inputFieldButtons = new List<IOptionInputFieldButtonViewAdapter>();

	public Transform CategoriesContainer => _categoriesContainer;

	public ILabelButtonViewAdapter DefaultButton => _defaultButton;

	public IScrollviewViewAdapter Scrollview => _scrollview;

	public IList<IOptionButtonViewAdapter> Buttons => _inputOptionButtons;

	private void Awake()
	{
		Object.FindObjectOfType<InputMenuPresenter>().Initialize(this);
		Visible = _inputMenuGameObject.activeSelf;
	}

	public override void Show()
	{
		base.Show();
		_inputMenuGameObject.SetActive(Visible);
	}

	public override void Hide()
	{
		base.Hide();
		_inputMenuGameObject.SetActive(Visible);
	}

	public IInputMapCategoryHeader CreateInputCategorySubheader(IOptionCategoryHeader header)
	{
		return Object.Instantiate<TMPInputMapCategoryHeader>(_inputMapCategorySubheaderPrefab, header.ElementsContainer, false);
	}

	public IOptionButtonViewAdapter CreateInputOptionButton(IOptionCategoryHeader header)
	{
		IOptionButtonViewAdapter optionButtonViewAdapter = Object.Instantiate<TMPOptionButtonViewAdapter>(_inputOptionButtonPrefab, header.ElementsContainer, false);
		_inputOptionButtons.Add(optionButtonViewAdapter);
		return optionButtonViewAdapter;
	}

	public IOptionSliderViewAdapter CreateInputOptionSlider(IOptionCategoryHeader header)
	{
		IOptionSliderViewAdapter optionSliderViewAdapter = Object.Instantiate<TMPOptionSliderViewAdapter>(_inputOptionSliderPrefab, header.ElementsContainer, false);
		_inputOptionSliders.Add(optionSliderViewAdapter);
		return optionSliderViewAdapter;
	}

	public IOptionInputFieldButtonViewAdapter CreateInputField(IOptionCategoryHeader header)
	{
		IOptionInputFieldButtonViewAdapter optionInputFieldButtonViewAdapter = Object.Instantiate<TMPOptionInputFieldButtonViewAdapter>(_inputFieldPrefab, header.ElementsContainer, false);
		optionInputFieldButtonViewAdapter.transform.SetAsLastSibling();
		_inputFieldButtons.Add(optionInputFieldButtonViewAdapter);
		return optionInputFieldButtonViewAdapter;
	}

	public ISelectableViewAdapter CreateControllerLayoutImage(IOptionCategoryHeader header)
	{
		USelectableViewAdapter uSelectableViewAdapter = Object.Instantiate<USelectableViewAdapter>(_controllerLayoutImagePrefab);
		((Component)uSelectableViewAdapter).transform.SetParent(header.ElementsContainer, false);
		return uSelectableViewAdapter;
	}

	public void RemoveButton(IOptionButtonViewAdapter button)
	{
		BeamTeam.Destroy<GameObject>(((Component)button.transform).gameObject);
		_inputOptionButtons.Remove(button);
	}

	public void DestroyInputOptionButtons()
	{
		for (int i = 0; i < _inputOptionButtons.Count; i++)
		{
			BeamTeam.Destroy<GameObject>(((Component)_inputOptionButtons[i].transform).gameObject);
		}
		_inputOptionButtons.Clear();
	}

	public void DestroyInputOptionSliders()
	{
		for (int i = 0; i < _inputOptionSliders.Count; i++)
		{
			BeamTeam.Destroy<GameObject>(((Component)_inputOptionSliders[i].transform).gameObject);
		}
		_inputOptionSliders.Clear();
	}

	public void DestroyInputFields()
	{
		for (int i = 0; i < _inputFieldButtons.Count; i++)
		{
			BeamTeam.Destroy<GameObject>(((Component)_inputFieldButtons[i].transform).gameObject);
		}
		_inputFieldButtons.Clear();
	}

	public void SetNavigation(IList<IOptionCategoryHeader> categories)
	{
		IOptionCategoryHeader item = categories.FirstOrDefault((IOptionCategoryHeader category) => category.Id == "OPTIONS_INPUT_MENU_LABEL_CONTROL_MAPPING".GetHashCode());
		IOptionCategoryHeader optionCategoryHeader = null;
		int num = categories.IndexOf(item) - 1;
		if (num >= 0)
		{
			optionCategoryHeader = categories[num];
		}
		Transform val = null;
		if (optionCategoryHeader != null && optionCategoryHeader.ElementsContainer.childCount > 0)
		{
			val = optionCategoryHeader.ElementsContainer.GetChild(optionCategoryHeader.ElementsContainer.childCount - 1);
		}
		for (int num2 = 0; num2 < _inputFieldButtons.Count; num2++)
		{
			TMPOptionInputFieldButtonViewAdapter tMPOptionInputFieldButtonViewAdapter = (TMPOptionInputFieldButtonViewAdapter)_inputFieldButtons[num2];
			((Component)tMPOptionInputFieldButtonViewAdapter).transform.SetNavigation<Transform>(NavigationDirection.Left, _inputMenuButton.transform);
			int num3 = num2 - 1;
			Transform element = ((num3 >= 0) ? _inputFieldButtons[num3].transform : val);
			((Component)tMPOptionInputFieldButtonViewAdapter).transform.SetNavigation<Transform>(NavigationDirection.Up, element);
			int num4 = num2 + 1;
			Transform element2 = ((num4 < _inputFieldButtons.Count) ? _inputFieldButtons[num4].transform : ((Component)_defaultButton).transform);
			((Component)tMPOptionInputFieldButtonViewAdapter).transform.SetNavigation<Transform>(NavigationDirection.Down, element2);
			IInputFieldButtonViewAdapter inputFieldButtonViewAdapter = tMPOptionInputFieldButtonViewAdapter.Buttons[0];
			IInputFieldButtonViewAdapter inputFieldButtonViewAdapter2 = tMPOptionInputFieldButtonViewAdapter.Buttons[1];
			inputFieldButtonViewAdapter.transform.SetNavigation<Transform>(NavigationDirection.Up, element);
			inputFieldButtonViewAdapter.transform.SetNavigation<Transform>(NavigationDirection.Down, element2);
			inputFieldButtonViewAdapter2.transform.SetNavigation<Transform>(NavigationDirection.Up, element);
			inputFieldButtonViewAdapter2.transform.SetNavigation<Transform>(NavigationDirection.Down, element2);
		}
	}
}
