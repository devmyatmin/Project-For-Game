using System;
using System.Collections.Generic;
using Beam.Events;
using Beam.Utilities;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class OptionsMenuPresenter : MonoBehaviour
{
	[SerializeField]
	private GeneralMenuPresenter _generalMenuPresenter;

	[SerializeField]
	private GraphicsMenuPresenter _graphicsMenuPresenter;

	[SerializeField]
	private AudioMenuPresenter _audioMenuPresenter;

	[SerializeField]
	private InputMenuPresenter _inputMenuPresenter;

	[SerializeField]
	private AboutMenuPresenter _aboutMenuPresenter;

	[SerializeField]
	private OptionsMenuViewAdapterBase _view;

	private IViewAdapter _parent;

	private List<IOptionPanel> _menuPanels;

	private Dictionary<IOptionPanel, IButtonViewAdapter> _panelButtons;

	private Player _currentPlayerInput;

	private IOptionPanel _currentPanel;

	private bool _isDirty;

	public Player CurrentPlayerInput => _currentPlayerInput;

	public IOptionPanel CurrentPanel => _currentPanel;

	public event EmptyEventHandler ShowView;

	public event EmptyEventHandler HideView;

	private void View_ShowView()
	{
		ShowView?.Invoke();
	}

	private void View_HideView()
	{
		HideView?.Invoke();
	}

	public void Initialize(IViewAdapter parent)
	{
		_parent = parent;
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView += Hide;
		}
	}

	private void Awake()
	{
		_view.ShowView += View_ShowView;
		_view.HideView += View_HideView;
		_menuPanels = new List<IOptionPanel> { _generalMenuPresenter, _graphicsMenuPresenter, _audioMenuPresenter, _inputMenuPresenter, _aboutMenuPresenter };
		_panelButtons = new Dictionary<IOptionPanel, IButtonViewAdapter>
		{
			{ _generalMenuPresenter, _view.GeneralButton },
			{ _graphicsMenuPresenter, _view.GraphicsButton },
			{ _audioMenuPresenter, _view.AudioButton },
			{ _inputMenuPresenter, _view.InputButton },
			{ _aboutMenuPresenter, _view.AboutButton }
		};
		SetControlEvents();
	}

	private void OnDestroy()
	{
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView -= Hide;
		}
		_view.ShowView -= View_ShowView;
		_view.HideView -= View_HideView;
		ShowView = null;
		HideView = null;
	}

	public void Show(Player input)
	{
		_currentPlayerInput = input;
		ControllerFocus.Instance?.Subscribe(_view);
		_view.SetInput(_currentPlayerInput);
		_view.Show();
		IButtonViewAdapter firstButton = _view.GetFirstButton();
		firstButton.Select();
		firstButton.PerformClick();
		SubscribeToInputEvents();
	}

	public void Hide()
	{
		UnsubscribeFromInputEvents();
		ControllerFocus.Instance?.Unsubscribe(_view);
		_currentPanel?.Hide();
		_currentPanel = null;
		_view.CurrentPanelButton = null;
		_view.Hide();
		ResetDirtySettings();
		_currentPlayerInput = null;
	}

	public void ResetControls(Player input)
	{
		if (_currentPlayerInput == null || _currentPlayerInput.id == input.id || !_view.Visible)
		{
			return;
		}
		Action resetControls = () =>
		{
			IOptionPanel currentPanel = _currentPanel;
			Hide();
			Show(input);
			if (currentPanel != null)
			{
				UpdateNavigation();
				SelectPanel(currentPanel);
			}
		};
		if (_isDirty)
		{
			DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(input, "OPTIONS_MENU_DB_SAVE_CHANGES_TITLE", "OPTIONS_MENU_DB_SAVE_CHANGES_DESC", DialogueButton.YesNo, DialogueIcon.Warning);
			dialogueBoxViewAdapter.Accept += () =>
			{
				_currentPanel.Apply();
				resetControls?.Invoke();
			};
			dialogueBoxViewAdapter.Decline += () =>
			{
				resetControls?.Invoke();
			};
		}
		else
		{
			resetControls?.Invoke();
		}
	}

	private void SubscribeToInputEvents()
	{
		_currentPlayerInput.AddInputEventDelegate((Action<InputActionEventData>)Rewired_BackButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 39);
		_currentPlayerInput.AddInputEventDelegate((Action<InputActionEventData>)Rewired_BackButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 31);
	}

	private void UnsubscribeFromInputEvents()
	{
		Player currentPlayerInput = _currentPlayerInput;
		if (currentPlayerInput != null)
		{
			currentPlayerInput.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_BackButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 39);
		}
		Player currentPlayerInput2 = _currentPlayerInput;
		if (currentPlayerInput2 != null)
		{
			currentPlayerInput2.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_BackButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 31);
		}
	}

	public void Save()
	{
		Options.Save();
		ResetDirtySettings();
	}

	private void SetControlEvents()
	{
		_view.BackButton.Click += BackButton_Click;
		_view.GeneralButton.Click += GeneralButton_Click;
		_view.GraphicsButton.Click += GraphicsButton_Click;
		_view.AudioButton.Click += AudioButton_Click;
		_view.InputButton.Click += InputButton_Click;
		_view.AboutButton.Click += AboutButton_Click;
	}

	public void SetApplyButtonEvent(Action<object> callback)
	{
		_view.ApplyButton.ClearEventSubscribers();
		_view.ApplyButton.Click += callback;
	}

	public IOptionCategoryHeader CreateOptionCategoryHeader()
	{
		return _view.CreateOptionCategoryHeader(_currentPanel);
	}

	public void DestroyOptionCategoryHeaders()
	{
		BeamTeam.DestroyAllChildren(_currentPanel.GetCategoriesContainer());
	}

	private void GeneralButton_Click(object sender)
	{
		UpdateNavigation();
		SelectPanel(_generalMenuPresenter);
	}

	private void GraphicsButton_Click(object sender)
	{
		UpdateNavigation();
		SelectPanel(_graphicsMenuPresenter);
	}

	private void AudioButton_Click(object sender)
	{
		UpdateNavigation();
		SelectPanel(_audioMenuPresenter);
	}

	private void InputButton_Click(object sender)
	{
		SelectPanel(_inputMenuPresenter);
	}

	private void AboutButton_Click(object sender)
	{
		UpdateNavigation();
		SelectPanel(_aboutMenuPresenter);
	}

	private void SelectPanel(IOptionPanel panel)
	{
		if (_currentPanel == panel)
		{
			return;
		}
		if (_isDirty)
		{
			DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(_currentPlayerInput, "OPTIONS_MENU_DB_SAVE_CHANGES_TITLE", "OPTIONS_MENU_DB_SAVE_CHANGES_DESC", DialogueButton.YesNo, DialogueIcon.Warning);
			dialogueBoxViewAdapter.Accept += () =>
			{
				_currentPanel.Apply();
				ShowPanel(panel);
			};
			dialogueBoxViewAdapter.Decline += () =>
			{
				ShowPanel(panel);
			};
		}
		else
		{
			ShowPanel(panel);
		}
	}

	private void ShowPanel(IOptionPanel panel)
	{
		_view.ApplyButton.ClearEventSubscribers();
		foreach (IOptionPanel menuPanel in _menuPanels)
		{
			if (menuPanel != panel)
			{
				menuPanel.Hide();
			}
		}
		SetTooltip(string.Empty);
		_currentPanel = panel;
		_currentPanel.Show();
		SelectPanelButton(_currentPanel);
		ResetScrollbar();
		ResetDirtySettings();
	}

	public void SelectPanelButton(IOptionPanel panel)
	{
		if (_panelButtons.TryGetValue(panel, out var value))
		{
			_view.CurrentPanelButton = value;
			_view.CurrentPanelButton.Select();
		}
	}

	public void SetDirtySettings()
	{
		if (!_isDirty)
		{
			_view.ApplyButton.Enabled = true;
			UpdateNavigation();
			_isDirty = true;
		}
	}

	public void ResetDirtySettings()
	{
		if (_isDirty)
		{
			EventManager.RaiseEvent(new OptionsAppliedEvent());
		}
		_view.ApplyButton.Enabled = false;
		_isDirty = false;
	}

	private void Rewired_BackButtonPressed(InputActionEventData data)
	{
		if (!DialogueBox.Notifying)
		{
			BackButton_Click(null);
		}
	}

	public void UpdateNavigation(Selectable target = null)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		if (_currentPanel == null)
		{
			return;
		}
		if (_currentPanel.Equals(_inputMenuPresenter) && (Object)(object)target == (Object)null)
		{
			if (_view.ApplyButton.Interactable)
			{
				Navigation navigation = ((Selectable)((Component)_view.AboutButton.transform).GetComponent<Button>()).navigation;
				navigation.mode = (Mode)4;
				navigation.selectOnDown = (Selectable)(object)((Component)_view.ApplyButton.transform).GetComponent<Button>();
				((Selectable)((Component)_view.AboutButton.transform).GetComponent<Button>()).navigation = navigation;
			}
			else
			{
				Navigation navigation2 = ((Selectable)((Component)_view.AboutButton.transform).GetComponent<Button>()).navigation;
				navigation2.mode = (Mode)4;
				navigation2.selectOnDown = (Selectable)(object)((Component)_view.BackButton.transform).GetComponent<Button>();
				((Selectable)((Component)_view.AboutButton.transform).GetComponent<Button>()).navigation = navigation2;
			}
			return;
		}
		if (Object.op_Implicit((Object)(object)target))
		{
			foreach (KeyValuePair<IOptionPanel, IButtonViewAdapter> panelButton in _panelButtons)
			{
				Navigation navigation3 = ((Selectable)((Component)panelButton.Value.transform).GetComponent<Button>()).navigation;
				navigation3.mode = (Mode)4;
				navigation3.selectOnRight = target;
				if (panelButton.Value.Equals(_view.InputButton))
				{
					navigation3.selectOnDown = (Selectable)(object)((Component)_view.AboutButton.transform).GetComponent<Button>();
				}
				if (panelButton.Value.Equals(_view.AboutButton))
				{
					navigation3.selectOnUp = (Selectable)(object)((Component)_view.InputButton.transform).GetComponent<Button>();
					navigation3.selectOnDown = (Selectable)(object)((Component)_view.BackButton.transform).GetComponent<Button>();
				}
				((Selectable)((Component)panelButton.Value.transform).GetComponent<Button>()).navigation = navigation3;
			}
			return;
		}
		foreach (KeyValuePair<IOptionPanel, IButtonViewAdapter> panelButton2 in _panelButtons)
		{
			Navigation navigation4 = ((Selectable)((Component)panelButton2.Value.transform).GetComponent<Button>()).navigation;
			navigation4.mode = (Mode)3;
			((Selectable)((Component)panelButton2.Value.transform).GetComponent<Button>()).navigation = navigation4;
		}
	}

	private void BackButton_Click(object sender)
	{
		if (_isDirty)
		{
			DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(_currentPlayerInput, "OPTIONS_MENU_DB_SAVE_CHANGES_TITLE", "OPTIONS_MENU_DB_SAVE_CHANGES_EXIT_DESC", DialogueButton.YesNo, DialogueIcon.Warning);
			dialogueBoxViewAdapter.Accept += () =>
			{
				_view.ApplyButton.PerformClick();
				Hide();
			};
			dialogueBoxViewAdapter.Decline += () =>
			{
				_isDirty = false;
				Hide();
			};
		}
		else
		{
			Hide();
		}
	}

	private void ResetScrollbar()
	{
		_view.Scrollbar.Value = 1f;
	}

	public void SetTooltip(string tooltip)
	{
		_view.SetTooltip(tooltip);
	}
}
