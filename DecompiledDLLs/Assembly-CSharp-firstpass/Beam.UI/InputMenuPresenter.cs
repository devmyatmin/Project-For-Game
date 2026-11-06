using System;
using System.Collections.Generic;
using System.Linq;
using Beam.Language;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using MEC;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class InputMenuPresenter : MonoBehaviour, IOptionPanel
{
	private enum AssignedPlayer
	{
		[Description("ASSIGNED_PLAYER_NONE", "")]
		None,
		[Description("ASSIGNED_PLAYER_P1", "")]
		P1,
		[Description("ASSIGNED_PLAYER_P2", "")]
		P2
	}

	[SerializeField]
	private OptionsMenuPresenter _optionsMenuPresenter;

	private IInputMenuViewAdapter _view;

	private IInputFieldButtonViewAdapter _currentButton;

	private DialogueBoxViewAdapter _dialogue;

	private CoroutineHandle _timeoutSequence;

	private bool _initialized;

	private List<IOptionCategoryHeader> _optionCategories = new List<IOptionCategoryHeader>();

	private IOptionButtonViewAdapter _toggleRadialMenusButton;

	private IOptionButtonViewAdapter _crouchToggleButton;

	private IOptionButtonViewAdapter _sprintToggleButton;

	private IOptionButtonViewAdapter _autowalkButton;

	private IOptionSliderViewAdapter _smoothnessSlider;

	private IOptionSliderViewAdapter _sensitivitySlider;

	private IOptionButtonViewAdapter _invertYButton;

	private Dictionary<string, ControllerAssignment> _controllerAssignments = new Dictionary<string, ControllerAssignment>();

	private bool _dirtyMappings;

	public void Initialize(IInputMenuViewAdapter view)
	{
		if (!_initialized)
		{
			_view = view;
			SetControlEvents();
			InputMapper.Default.options.timeout = 5f;
			InputMapper.Default.options.allowKeyboardKeysWithModifiers = false;
			InputMapper.Default.options.allowKeyboardModifierKeyAsPrimary = true;
			InputMapper.Default.InputMappedEvent += InputMapper_InputMapped;
			InputMapper.Default.ConflictFoundEvent += InputMapper_ConflictFound;
			InputMapper.Default.StartedEvent += InputMapper_Started;
			InputMapper.Default.StoppedEvent += InputMapper_Stopped;
			InputMapper.Default.CanceledEvent += InputMapper_Cancelled;
			InputMapper.Default.ErrorEvent += InputMapper_Error;
			InputMapper.Default.TimedOutEvent += InputMapper_TimedOut;
			_initialized = true;
		}
	}

	private void OnDestroy()
	{
		if (_initialized)
		{
			InputMapper.Default.InputMappedEvent -= InputMapper_InputMapped;
			InputMapper.Default.ConflictFoundEvent -= InputMapper_ConflictFound;
			InputMapper.Default.StartedEvent -= InputMapper_Started;
			InputMapper.Default.StoppedEvent -= InputMapper_Stopped;
			InputMapper.Default.CanceledEvent -= InputMapper_Cancelled;
			InputMapper.Default.ErrorEvent -= InputMapper_Error;
			InputMapper.Default.TimedOutEvent -= InputMapper_TimedOut;
		}
	}

	public Transform GetCategoriesContainer()
	{
		return _view.CategoriesContainer;
	}

	public void Show()
	{
		if (!_view.Visible)
		{
			_view.Show();
			_view.DefaultButton.Show();
			_optionsMenuPresenter.SetApplyButtonEvent(ApplyButton_Click);
			PopulateControls();
			_optionsMenuPresenter.ResetDirtySettings();
			ReInput.ControllerConnectedEvent += Rewired_OnControllerConnected;
			ReInput.ControllerPreDisconnectEvent += Rewired_OnControllerPreDisconnect;
		}
	}

	public void Hide()
	{
		if (_view.Visible)
		{
			if (_dirtyMappings)
			{
				Options.Load();
				_dirtyMappings = false;
			}
			_view.DefaultButton.Hide();
			_view.Hide();
			ReInput.ControllerConnectedEvent -= Rewired_OnControllerConnected;
			ReInput.ControllerPreDisconnectEvent -= Rewired_OnControllerPreDisconnect;
		}
	}

	void IOptionPanel.Apply()
	{
		ApplyButton_Click(null);
	}

	private void SetControlEvents()
	{
		_view.DefaultButton.Click += DefaultButton_Click;
	}

	private void PopulateControls()
	{
		_controllerAssignments.Clear();
		foreach (KeyValuePair<string, ControllerAssignment> controllerAssignment in Options.InputSettings.ControllerAssignments)
		{
			if (!_controllerAssignments.ContainsKey(controllerAssignment.Key))
			{
				_controllerAssignments.Add(controllerAssignment.Key, null);
			}
			_controllerAssignments[controllerAssignment.Key] = controllerAssignment.Value;
		}
		CreateOptionCategories();
	}

	private void RefreshControls()
	{
	}

	private void CreateOptionCategories()
	{
		_optionCategories.Clear();
		_optionsMenuPresenter.DestroyOptionCategoryHeaders();
		_view.DestroyInputOptionButtons();
		_view.DestroyInputOptionSliders();
		_view.DestroyInputFields();
		CreateInputOptionButtons();
		CreateControllerCategory();
		CreateInputFieldButtons();
		_view.SetNavigation(_optionCategories);
	}

	private void CreateInputOptionButtons()
	{
		IOptionCategoryHeader header = CreateInputCategoryHeader("OPTIONS_INPUT_MENU_LABEL_GENERAL");
		_toggleRadialMenusButton = CreateInputButton(header, "OPTIONS_INPUT_MENU_BUTTON_TOGGLE_RADIAL_MENUS", Reflection.GetAttributes<DescriptionAttribute>(typeof(Quality_OFF_ON)));
		_toggleRadialMenusButton.SelectedIndex = Reflection.GetIndex(Options.InputSettings.ToggleRadialMenus);
		_optionsMenuPresenter.UpdateNavigation((Selectable)(object)((Component)_toggleRadialMenusButton.transform).GetComponent<Button>());
		_crouchToggleButton = CreateInputButton(header, "OPTIONS_INPUT_MENU_BUTTON_CROUCH_TOGGLE", Reflection.GetAttributes<DescriptionAttribute>(typeof(Quality_OFF_ON)));
		_crouchToggleButton.SelectedIndex = Reflection.GetIndex(Options.InputSettings.CrouchToggle);
		_sprintToggleButton = CreateInputButton(header, "OPTIONS_INPUT_MENU_BUTTON_SPRINT_TOGGLE", Reflection.GetAttributes<DescriptionAttribute>(typeof(Quality_OFF_ON)));
		_sprintToggleButton.SelectedIndex = Reflection.GetIndex(Options.InputSettings.SprintToggle);
		_autowalkButton = CreateInputButton(header, "OPTIONS_INPUT_MENU_BUTTON_AUTO_WALK", Reflection.GetAttributes<DescriptionAttribute>(typeof(Quality_OFF_ON)));
		_autowalkButton.SelectedIndex = Reflection.GetIndex(Options.InputSettings.AutoWalk);
		IOptionCategoryHeader header2 = CreateInputCategoryHeader("OPTIONS_INPUT_MENU_LABEL_CAMERA");
		_smoothnessSlider = CreateInputSlider(header2, "OPTIONS_INPUT_MENU_BUTTON_SMOOTHNESS");
		_smoothnessSlider.Value = Options.InputSettings.MouseSmoothness;
		_smoothnessSlider.Refresh();
		_sensitivitySlider = CreateInputSlider(header2, "OPTIONS_INPUT_MENU_BUTTON_SENSITIVITY");
		_sensitivitySlider.Value = Options.InputSettings.MouseSensitivity;
		_sensitivitySlider.Refresh();
		_invertYButton = CreateInputButton(header2, "OPTIONS_INPUT_MENU_BUTTON_INVERT_Y", Reflection.GetAttributes<DescriptionAttribute>(typeof(Quality_OFF_ON)));
		_invertYButton.SelectedIndex = Reflection.GetIndex(Options.InputSettings.InvertY);
	}

	private IOptionButtonViewAdapter CreateInputButton(IOptionCategoryHeader header, string name, IEnumerable<IOptionElementData> values)
	{
		IOptionButtonViewAdapter optionButtonViewAdapter = _view.CreateInputOptionButton(header);
		optionButtonViewAdapter.Text = name;
		optionButtonViewAdapter.Initialize(values);
		optionButtonViewAdapter.Selected += OptionWidget_Selected;
		optionButtonViewAdapter.SelectionChanged += OptionWidget_SelectionChanged;
		return optionButtonViewAdapter;
	}

	private IOptionSliderViewAdapter CreateInputSlider(IOptionCategoryHeader header, string name)
	{
		IOptionSliderViewAdapter optionSliderViewAdapter = _view.CreateInputOptionSlider(header);
		((IOptionWidgetViewAdapter)optionSliderViewAdapter).Text = name;
		optionSliderViewAdapter.Selected += OptionWidget_Selected;
		optionSliderViewAdapter.ValueChanged += OptionSlider_ValueChanged;
		return optionSliderViewAdapter;
	}

	private void CreateInputFieldButtons()
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Invalid comparison between Unknown and I4
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(0);
		IOptionCategoryHeader header = CreateInputCategoryHeader("OPTIONS_INPUT_MENU_LABEL_CONTROL_MAPPING");
		foreach (InputMapCategory userAssignableMapCategory in ReInput.mapping.UserAssignableMapCategories)
		{
			InputCategory actionCategory = ReInput.mapping.GetActionCategory(((InputCategory)userAssignableMapCategory).name);
			if (actionCategory == null)
			{
				continue;
			}
			CreateInputCategorySubheader(header, LocalizationTerms.GetActionCategoryTerm(((InputCategory)userAssignableMapCategory).name));
			ControllerMap firstMapInCategory = player.controllers.maps.GetFirstMapInCategory((ControllerType)1, 0, ((InputCategory)userAssignableMapCategory).id);
			ControllerMap firstMapInCategory2 = player.controllers.maps.GetFirstMapInCategory((ControllerType)0, 0, ((InputCategory)userAssignableMapCategory).id);
			foreach (InputAction item in ReInput.mapping.UserAssignableActionsInCategory(actionCategory.id, true))
			{
				string actionName = (string.IsNullOrEmpty(item.descriptiveName) ? item.name : item.descriptiveName);
				if ((int)item.type == 1)
				{
					IOptionInputFieldButtonViewAdapter optionInputFieldButtonViewAdapter = CreateInputFieldControl(header, LocalizationTerms.GetActionTerm(actionName));
					CreateInputButton(optionInputFieldButtonViewAdapter, firstMapInCategory2, item.id, ActionElementPredicate);
					CreateInputButton(optionInputFieldButtonViewAdapter, firstMapInCategory, item.id, ActionElementPredicate);
					optionInputFieldButtonViewAdapter.SetNavigation();
					Navigation navigation = ((Selectable)((Component)_view.DefaultButton.transform).GetComponent<Button>()).navigation;
					navigation.mode = (Mode)4;
					navigation.selectOnUp = ((Component)optionInputFieldButtonViewAdapter.transform).GetComponent<Selectable>();
					((Selectable)((Component)_view.DefaultButton.transform).GetComponent<Button>()).navigation = navigation;
				}
				else if ((int)item.type == 0)
				{
					IOptionInputFieldButtonViewAdapter optionInputFieldButtonViewAdapter2 = CreateInputFieldControl(header, LocalizationTerms.GetActionTerm(actionName));
					CreateInputButton(optionInputFieldButtonViewAdapter2, null, item.id, (AxisRange)0, AxisActionElementPredicate);
					CreateInputButton(optionInputFieldButtonViewAdapter2, firstMapInCategory, item.id, (AxisRange)0, AxisActionElementPredicate, onlyAxes: true);
					optionInputFieldButtonViewAdapter2.SetNavigation();
					string actionName2 = (string.IsNullOrEmpty(item.positiveDescriptiveName) ? (item.descriptiveName + " +") : item.positiveDescriptiveName);
					IOptionInputFieldButtonViewAdapter optionInputFieldButtonViewAdapter3 = CreateInputFieldControl(header, LocalizationTerms.GetActionTerm(actionName2));
					CreateInputButton(optionInputFieldButtonViewAdapter3, firstMapInCategory2, item.id, (AxisRange)1, PositiveActionElementPredicate);
					CreateInputButton(optionInputFieldButtonViewAdapter3, firstMapInCategory, item.id, (AxisRange)1, PositiveActionElementPredicate);
					optionInputFieldButtonViewAdapter3.SetNavigation();
					string actionName3 = (string.IsNullOrEmpty(item.negativeDescriptiveName) ? (item.descriptiveName + " -") : item.negativeDescriptiveName);
					IOptionInputFieldButtonViewAdapter optionInputFieldButtonViewAdapter4 = CreateInputFieldControl(header, LocalizationTerms.GetActionTerm(actionName3));
					CreateInputButton(optionInputFieldButtonViewAdapter4, firstMapInCategory2, item.id, (AxisRange)2, NegativeActionElementPredicate);
					CreateInputButton(optionInputFieldButtonViewAdapter4, firstMapInCategory, item.id, (AxisRange)2, NegativeActionElementPredicate);
					optionInputFieldButtonViewAdapter4.SetNavigation();
				}
			}
		}
	}

	private IOptionCategoryHeader CreateInputCategoryHeader(string name)
	{
		IOptionCategoryHeader optionCategoryHeader = _optionsMenuPresenter.CreateOptionCategoryHeader();
		optionCategoryHeader.Label.Text = name;
		optionCategoryHeader.Id = name.GetHashCode();
		_optionCategories.Add(optionCategoryHeader);
		return optionCategoryHeader;
	}

	private IInputMapCategoryHeader CreateInputCategorySubheader(IOptionCategoryHeader header, string name)
	{
		IInputMapCategoryHeader inputMapCategoryHeader = _view.CreateInputCategorySubheader(header);
		inputMapCategoryHeader.Label.Text = name;
		return inputMapCategoryHeader;
	}

	private IOptionInputFieldButtonViewAdapter CreateInputFieldControl(IOptionCategoryHeader header, string name)
	{
		IOptionInputFieldButtonViewAdapter optionInputFieldButtonViewAdapter = _view.CreateInputField(header);
		optionInputFieldButtonViewAdapter.Text = name;
		optionInputFieldButtonViewAdapter.Selected += OptionWidget_Selected;
		return optionInputFieldButtonViewAdapter;
	}

	private IInputFieldButtonViewAdapter CreateInputButton(IOptionInputFieldButtonViewAdapter inputFieldOptionButton, ControllerMap controllerMap, int actionId, Func<ActionElementMap, int, bool> predicate)
	{
		return CreateInputButton(inputFieldOptionButton, controllerMap, actionId, (AxisRange)0, predicate);
	}

	private IInputFieldButtonViewAdapter CreateInputButton(IOptionInputFieldButtonViewAdapter inputFieldOptionButton, ControllerMap controllerMap, int actionId, AxisRange axisRange, Func<ActionElementMap, int, bool> predicate, bool onlyAxes = false)
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		ActionElementMap val = null;
		bool flag = controllerMap != null;
		if (flag)
		{
			foreach (ActionElementMap allMap in controllerMap.AllMaps)
			{
				if (predicate(allMap, actionId))
				{
					val = allMap;
					break;
				}
			}
		}
		IInputFieldButtonViewAdapter inputFieldButtonViewAdapter;
		if (val != null)
		{
			bool flag2 = val.elementIdentifierId > -1 && !val.elementIdentifierName.Equals("NONE", StringComparison.InvariantCultureIgnoreCase);
			inputFieldButtonViewAdapter = inputFieldOptionButton.CreateButton();
			inputFieldButtonViewAdapter.Text = (flag2 ? val.elementIdentifierName : "...");
			inputFieldButtonViewAdapter.Type = (flag2 ? InputFieldButtonType.ReassignOrRemove : InputFieldButtonType.Add);
			inputFieldButtonViewAdapter.Info = new InputFieldButtonInfo
			{
				ControllerMap = controllerMap,
				ActionId = actionId,
				ActionRange = axisRange,
				ActionElementMapId = val.id,
				AllowOnlyAxesMapping = onlyAxes
			};
		}
		else
		{
			inputFieldButtonViewAdapter = inputFieldOptionButton.CreateButton();
			if (flag)
			{
				inputFieldButtonViewAdapter.Text = "...";
				inputFieldButtonViewAdapter.Type = InputFieldButtonType.Add;
				inputFieldButtonViewAdapter.Info = new InputFieldButtonInfo
				{
					ControllerMap = controllerMap,
					ActionId = actionId,
					ActionRange = axisRange,
					AllowOnlyAxesMapping = onlyAxes
				};
			}
			else
			{
				inputFieldButtonViewAdapter.Text = string.Empty;
				inputFieldButtonViewAdapter.Enabled = false;
			}
		}
		inputFieldButtonViewAdapter.Click += InputFieldButton_Click;
		return inputFieldButtonViewAdapter;
	}

	private void CreateControllerCategory()
	{
		IOptionCategoryHeader header = CreateInputCategoryHeader("OPTIONS_INPUT_MENU_LABEL_CONTROLLERS");
		IEnumerable<IOptionElementData> attributes = Reflection.GetAttributes<DescriptionAttribute>(typeof(AssignedPlayer));
		IOptionButtonViewAdapter optionButtonViewAdapter = CreateInputButton(header, "OPTIONS_INPUT_MENU_BUTTON_KEYBOARD_MOUSE", attributes);
		optionButtonViewAdapter.SelectedIndex = Reflection.GetIndex(AssignedPlayer.P1);
		optionButtonViewAdapter.Enabled = false;
		Joystick[] joysticks = ReInput.controllers.GetJoysticks();
		Joystick[] array = joysticks;
		foreach (Joystick joystick in array)
		{
			JoystickOptionButton joystickOptionButton = CreateJoystickOptionButton(joystick);
			if (joysticks.Length == 1 && _optionsMenuPresenter.CurrentPlayerInput.id == 1)
			{
				joystickOptionButton.Enabled = false;
			}
		}
		IOptionCategoryHeader header2 = CreateInputCategoryHeader("OPTIONS_INPUT_MENU_LABEL_CONTROLLER_MAPPING");
		_view.CreateControllerLayoutImage(header2).Selected += OptionWidget_Selected;
	}

	private JoystickOptionButton CreateJoystickOptionButton(Joystick joystick)
	{
		IOptionCategoryHeader header = _optionCategories.First((IOptionCategoryHeader x) => x.Id == "OPTIONS_INPUT_MENU_LABEL_CONTROLLERS".GetHashCode());
		IEnumerable<IOptionElementData> enumerable = Reflection.GetAttributes<DescriptionAttribute>(typeof(AssignedPlayer));
		if (_optionsMenuPresenter.CurrentPlayerInput.id == 1 && ReInput.controllers.joystickCount > 1)
		{
			enumerable = enumerable.Where((IOptionElementData x) => x.Text != "ASSIGNED_PLAYER_NONE");
		}
		IOptionButtonViewAdapter optionButtonViewAdapter = CreateInputButton(header, ((Controller)joystick).name, enumerable);
		string text = ((Controller)joystick).deviceInstanceGuid.ToString();
		JoystickOptionButton joystickOptionButton = new JoystickOptionButton(optionButtonViewAdapter, text);
		joystickOptionButton.SelectionChanged += JoystickOptionButton_SelectionChanged;
		AssignedPlayer assignedPlayer = AssignedPlayer.None;
		if (_controllerAssignments.TryGetValue(text, out var value))
		{
			assignedPlayer = (AssignedPlayer)(value.PlayerId + 1);
		}
		joystickOptionButton.SelectedIndex = Reflection.GetIndex(assignedPlayer);
		_view.Buttons.Remove(optionButtonViewAdapter);
		_view.Buttons.Add(joystickOptionButton);
		return joystickOptionButton;
	}

	private void Rewired_OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)args.controllerType == 2)
		{
			Controller controller = args.controller;
			Joystick joystick = (Joystick)(object)((controller is Joystick) ? controller : null);
			CreateJoystickOptionButton(joystick);
		}
	}

	private void Rewired_OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)args.controllerType != 2)
		{
			return;
		}
		string value = args.controller.deviceInstanceGuid.ToString();
		foreach (IOptionButtonViewAdapter button in _view.Buttons)
		{
			if (button is JoystickOptionButton joystickOptionButton && joystickOptionButton.DeviceInstanceId.Equals(value))
			{
				_view.RemoveButton(joystickOptionButton);
				break;
			}
		}
	}

	private void ApplyButton_Click(object sender)
	{
		Options.InputSettings.ToggleRadialMenus = (Quality_OFF_ON)_toggleRadialMenusButton.SelectedIndex;
		Options.InputSettings.CrouchToggle = (Quality_OFF_ON)_crouchToggleButton.SelectedIndex;
		Options.InputSettings.SprintToggle = (Quality_OFF_ON)_sprintToggleButton.SelectedIndex;
		Options.InputSettings.AutoWalk = (Quality_OFF_ON)_autowalkButton.SelectedIndex;
		Options.InputSettings.MouseSmoothness = _smoothnessSlider.Value;
		Options.InputSettings.MouseSensitivity = _sensitivitySlider.Value;
		Options.InputSettings.InvertY = (Quality_OFF_ON)_invertYButton.SelectedIndex;
		Options.InputSettings.ControllerAssignments = new Dictionary<string, ControllerAssignment>(_controllerAssignments);
		_optionsMenuPresenter.Save();
	}

	private void DefaultButton_Click(object sender)
	{
		DialogueBox.Show(ReInput.players.GetPlayer(0), "OPTIONS_INPUT_MENU_DB_RESET_TITLE", "OPTIONS_INPUT_MENU_DB_RESET_DESC", DialogueButton.YesNo).Accept += () =>
		{
			LoadDefaults();
			PopulateControls();
			_optionsMenuPresenter.SetDirtySettings();
			_optionsMenuPresenter.ResetDirtySettings();
		};
	}

	private void OptionWidget_Selected(ISelectableViewAdapter widget)
	{
		_view.Scrollview.FocusElement(widget.transform);
	}

	private void OptionWidget_SelectionChanged(IOptionWidgetViewAdapter widget)
	{
		_optionsMenuPresenter.SetDirtySettings();
	}

	private void OptionSlider_ValueChanged(float value)
	{
		_optionsMenuPresenter.SetDirtySettings();
	}

	private void JoystickOptionButton_SelectionChanged(IOptionButtonViewAdapter button)
	{
		if (!(button is JoystickOptionButton joystickOptionButton))
		{
			return;
		}
		Joystick val = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(ReInput.controllers.Joysticks, (Joystick joy, string arg) => ((Controller)joy).isConnected && ((Controller)joy).deviceInstanceGuid.ToString() == arg, joystickOptionButton.DeviceInstanceId);
		if (val == null)
		{
			Debug.LogError((object)("JoystickOptionButton_SelectionChanged:: No matching joystick found for [" + joystickOptionButton.Text + "]."));
			return;
		}
		AssignedPlayer selectedIndex = (AssignedPlayer)button.SelectedIndex;
		if (selectedIndex == AssignedPlayer.None)
		{
			UnassignController(((Controller)val).id);
			return;
		}
		int playerId = (int)(selectedIndex - 1);
		AssignController(playerId, ((Controller)val).id);
	}

	private void InputFieldButton_Click(object sender)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		IInputFieldButtonViewAdapter inputFieldButton = sender as IInputFieldButtonViewAdapter;
		if (inputFieldButton == null || (Object)(object)_dialogue != (Object)null)
		{
			return;
		}
		Context context = new Context();
		context.controllerMap = inputFieldButton.Info.ControllerMap;
		context.actionId = inputFieldButton.Info.ActionId;
		context.actionRange = inputFieldButton.Info.ActionRange;
		context.actionElementMapToReplace = inputFieldButton.Info.ControllerMap.GetElementMap(inputFieldButton.Info.ActionElementMapId);
		bool allowOnlyAxes = inputFieldButton.Info.AllowOnlyAxesMapping;
		InputMapper.Default.options.allowButtons = !allowOnlyAxes;
		_currentButton = inputFieldButton;
		switch (_currentButton.Type)
		{
		case InputFieldButtonType.Add:
			StartMapping(context, allowOnlyAxes);
			break;
		case InputFieldButtonType.ReassignOrRemove:
		{
			DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(ReInput.players.GetPlayer(0), "OPTIONS_INPUT_MENU_DB_REASSIGN_TITLE", "OPTIONS_INPUT_MENU_DB_REASSIGN_DESC", "OPTIONS_INPUT_MENU_DB_BUTTON_REASSIGN", "OPTIONS_INPUT_MENU_DB_BUTTON_REMOVE", "OPTIONS_INPUT_MENU_DB_BUTTON_CANCEL");
			dialogueBoxViewAdapter.Accept += () =>
			{
				StartMapping(context, allowOnlyAxes);
			};
			dialogueBoxViewAdapter.Decline += () =>
			{
				ClearMapping(inputFieldButton);
				_currentButton.Select();
				_currentButton = null;
			};
			break;
		}
		}
	}

	private void StartMapping(Context context, bool allowOnlyAxes)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		string remappingDialogueMessage = GetRemappingDialogueMessage(context.controllerMap.controllerType, allowOnlyAxes);
		_dialogue = DialogueBox.Show(ReInput.players.GetPlayer(0), "OPTIONS_INPUT_MENU_DB_ASSIGN_TITLE", remappingDialogueMessage, DialogueButton.None);
		InputMapper.Default.Start(context);
	}

	private void ClearMapping(IInputFieldButtonViewAdapter inputFieldButton)
	{
		ActionElementMap elementMap = inputFieldButton.Info.ControllerMap.GetElementMap(inputFieldButton.Info.ActionElementMapId);
		if (ClearElementMapAssignment(elementMap))
		{
			inputFieldButton.Text = "...";
			inputFieldButton.Type = InputFieldButtonType.Add;
			ClearMatchingElementMapsFromControllers(elementMap);
			_optionsMenuPresenter.SetDirtySettings();
		}
	}

	private void BeginRemapping()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_timeoutSequence = Timing.RunCoroutine(InputMapper_TimeoutSequence());
		RewiredUtilities.SetAndCacheMapsEnabled(ReInput.players.GetPlayer(0), state: false, 8);
	}

	private void EndMapping()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (_timeoutSequence.IsRunning)
		{
			Timing.KillCoroutines(_timeoutSequence);
		}
		if ((Object)(object)_dialogue != (Object)null)
		{
			_dialogue.Destroy();
			_dialogue = null;
		}
		if (_currentButton != null)
		{
			_currentButton.Select();
			_currentButton = null;
		}
		RewiredUtilities.SetAndCacheMapsEnabled(ReInput.players.GetPlayer(0), state: true, 8);
	}

	private void InputMapper_Started(StartedEventData eventData)
	{
		BeginRemapping();
	}

	private void InputMapper_Stopped(StoppedEventData eventData)
	{
		EndMapping();
	}

	private void InputMapper_Cancelled(CanceledEventData eventData)
	{
		EndMapping();
	}

	private void InputMapper_Error(ErrorEventData eventData)
	{
		EndMapping();
	}

	private void InputMapper_TimedOut(TimedOutEventData eventData)
	{
	}

	private void InputMapper_InputMapped(InputMappedEventData eventData)
	{
		_currentButton.Text = eventData.actionElementMap.elementIdentifierName;
		_currentButton.Type = InputFieldButtonType.ReassignOrRemove;
		_currentButton.Info.ActionElementMapId = eventData.actionElementMap.id;
		_dirtyMappings = true;
		_optionsMenuPresenter.SetDirtySettings();
		RemapMatchingElementMapsInControllers(eventData.actionElementMap);
	}

	private void RemapMatchingElementMapsInControllers(ActionElementMap newActionElementMap)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		ElementAssignment val = new ElementAssignment(newActionElementMap.controllerMap.controllerType, newActionElementMap.elementType, newActionElementMap.elementIdentifierId, newActionElementMap.axisRange, newActionElementMap.keyCode, newActionElementMap.modifierKeyFlags, newActionElementMap.actionId, newActionElementMap.axisContribution, false);
		ActionElementMap val2 = default;
		foreach (ControllerMap allMap in newActionElementMap.controllerMap.player.controllers.maps.GetAllMaps(newActionElementMap.controllerMap.controllerType))
		{
			foreach (ActionElementMap item in allMap.GetElementMapsWithAction(newActionElementMap.actionId).Where((ActionElementMap aem) =>
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				return aem.elementIdentifierId != newActionElementMap.elementIdentifierId && aem.axisContribution == newActionElementMap.axisContribution;
			}))
			{
				val.elementMapId = item.id;
				_ = item.elementIdentifierName;
				allMap.ReplaceElementMap(val, ref val2);
			}
		}
	}

	private void ClearMatchingElementMapsFromControllers(ActionElementMap removedActionElementMap)
	{
		foreach (ControllerMap item in removedActionElementMap.controllerMap.player.controllers.maps.GetAllMaps().Where((ControllerMap map) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return map.controllerType == removedActionElementMap.controllerMap.controllerType;
		}))
		{
			foreach (ActionElementMap item2 in item.GetElementMapsWithAction(removedActionElementMap.actionId).Where((ActionElementMap aem) =>
			{
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				return aem.elementIdentifierId != 0 && aem.axisContribution == removedActionElementMap.axisContribution;
			}))
			{
				ClearElementMapAssignment(item2);
			}
		}
	}

	private bool ClearElementMapAssignment(ActionElementMap actionElementMap)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		_ = ReInput.mapping.GetAction(actionElementMap.actionId).descriptiveName;
		_ = actionElementMap.elementIdentifierName;
		_ = ((InputCategory)ReInput.mapping.GetMapCategory(actionElementMap.controllerMap.categoryId)).name;
		ElementAssignment val = new ElementAssignment(actionElementMap.controllerMap.controllerType, actionElementMap.elementType, -1, actionElementMap.axisRange, (KeyCode)0, (ModifierKeyFlags)0, actionElementMap.actionId, actionElementMap.axisContribution, false);
		val.elementMapId = actionElementMap.id;
		ActionElementMap val2 = default;
		if (!actionElementMap.controllerMap.ReplaceElementMap(val, ref val2))
		{
			return false;
		}
		return true;
	}

	private void InputMapper_ConflictFound(ConflictFoundEventData eventData)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogError((object)"InputMenu::InputMapper_ConflictFound::");
		eventData.responseCallback((ConflictResponse)0);
		((EventData)eventData).inputMapper.Stop();
		string text = Localization.GetLanguageHandler().Localize("OPTIONS_INPUT_MENU_DB_CONFLICT_DESC", eventData.assignment.elementDisplayName, eventData.assignment.action.descriptiveName);
		ElementAssignmentConflictInfo val = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(eventData.conflicts);
		text = text + "[" + val.elementDisplayName + " - " + val.action.descriptiveName + "]";
		DialogueBox.Show(ReInput.players.GetPlayer(0), "OPTIONS_INPUT_MENU_DB_CONFLICT_TITLE", text, DialogueButton.Ok);
	}

	private void LoadDefaults()
	{
		Player player = ReInput.players.GetPlayer(0);
		player.controllers.maps.LoadDefaultMaps((ControllerType)1);
		player.controllers.maps.LoadDefaultMaps((ControllerType)0);
		Options.InputSettings = new InputSettings();
		Options.Save();
		Options.Load();
	}

	private void AssignController(int playerId, int controllerId)
	{
		Joystick joystick = ReInput.controllers.GetJoystick(controllerId);
		if (joystick == null)
		{
			Debug.LogError((object)"Something went wrong. Controller id not found. Disconnected?");
			return;
		}
		ControllerAssignment controllerAssignment = new ControllerAssignment();
		controllerAssignment.ControllerName = ((Controller)joystick).name;
		controllerAssignment.ControllerGuid = ((Controller)joystick).deviceInstanceGuid.ToString();
		controllerAssignment.PlayerId = playerId;
		if (!_controllerAssignments.ContainsKey(controllerAssignment.ControllerGuid))
		{
			_controllerAssignments.Add(controllerAssignment.ControllerGuid, null);
		}
		_controllerAssignments[controllerAssignment.ControllerGuid] = controllerAssignment;
	}

	private void UnassignController(int controllerId)
	{
		Joystick joystick = ReInput.controllers.GetJoystick(controllerId);
		if (joystick == null)
		{
			Debug.LogError((object)"Something went wrong. Controller id not found. Disconnected?");
		}
		else
		{
			_controllerAssignments.Remove(((Controller)joystick).deviceInstanceGuid.ToString());
		}
	}

	public bool ActionElementPredicate(ActionElementMap actionElementMap, int actionId)
	{
		if (actionElementMap != null)
		{
			return actionElementMap.actionId == actionId;
		}
		return false;
	}

	public bool AxisActionElementPredicate(ActionElementMap actionElementMap, int actionId)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if (actionElementMap != null && actionElementMap.actionId == actionId && (int)actionElementMap.elementType != 1)
		{
			return (int)actionElementMap.axisType != 2;
		}
		return false;
	}

	public bool PositiveActionElementPredicate(ActionElementMap actionElementMap, int actionId)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if (actionElementMap != null && actionElementMap.actionId == actionId && (int)actionElementMap.axisType != 1)
		{
			return (int)actionElementMap.axisContribution == 0;
		}
		return false;
	}

	public bool NegativeActionElementPredicate(ActionElementMap actionElementMap, int actionId)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if (actionElementMap != null && actionElementMap.actionId == actionId && (int)actionElementMap.axisType != 1)
		{
			return (int)actionElementMap.axisContribution == 1;
		}
		return false;
	}

	private string GetRemappingDialogueMessage(ControllerType controllerType, bool onlyAxesAllowed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		if ((int)controllerType != 0)
		{
			if ((int)controllerType != 1)
			{
				return "OPTIONS_INPUT_MENU_DB_ASSIGN_DESC_BUTTON_OR_AXIS";
			}
			if (!onlyAxesAllowed)
			{
				return "OPTIONS_INPUT_MENU_DB_ASSIGN_DESC_MOUSE_BUTTON_OR_AXIS";
			}
			return "OPTIONS_INPUT_MENU_DB_ASSIGN_DESC_AXIS";
		}
		return "OPTIONS_INPUT_MENU_DB_ASSIGN_DESC_ANY_KEY";
	}

	private IEnumerator<float> InputMapper_TimeoutSequence()
	{
		string message = _dialogue.Message.Text;
		while (true)
		{
			_dialogue.Message.Text = $"{message} ({Mathf.CeilToInt(InputMapper.Default.timeRemaining)})";
			yield return 0f;
		}
	}

	private Player GetPlayer(AssignedPlayer value)
	{
		try
		{
			switch (value)
			{
			case AssignedPlayer.P1:
				return RewiredUtilities.Players[0];
			case AssignedPlayer.P2:
				return RewiredUtilities.Players[1];
			}
		}
		catch (IndexOutOfRangeException ex)
		{
			Debug.LogException((Exception)ex);
			return null;
		}
		return null;
	}
}
