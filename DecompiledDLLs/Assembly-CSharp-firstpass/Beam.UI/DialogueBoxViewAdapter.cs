using System;
using System.Collections.Generic;
using Beam.Events;
using Beam.Utilities;
using Beam.Utilities.UGUI;
using Funlabs;
using Rewired;
using UnityEngine;

namespace Beam.UI;

public class DialogueBoxViewAdapter : MenuViewAdapterBase
{
	[SerializeField]
	private TMPLabelButtonViewAdapter _dialogueButtonPrefab;

	[SerializeField]
	private GameObject _dialogueButtonContainer;

	public TMPLabelViewAdapter Title;

	public UImageViewAdapter TitleHeaderImage;

	public TMPLabelViewAdapter Message;

	public TMPInputFieldViewAdapter InputField;

	public string Action1;

	public string Action2;

	public string Action3;

	private List<TMPLabelButtonViewAdapter> _buttons = new List<TMPLabelButtonViewAdapter>();

	private DialogueType _dialogueType;

	private DialogueButton _dialogueButtons;

	public override bool Active => !BeamTeam.IsNullOrDestroyed(((Component)this).gameObject);

	public event EmptyEventHandler Accept;

	public event EmptyEventHandler Decline;

	public event EmptyEventHandler Cancel;

	protected virtual void OnAccept(object sender)
	{
		Accept?.Invoke();
	}

	protected virtual void OnDecline(object sender)
	{
		Decline?.Invoke();
	}

	protected virtual void OnCancel(object sender)
	{
		Cancel?.Invoke();
	}

	public override IButtonViewAdapter GetFirstButton()
	{
		return Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_buttons);
	}

	public override void SelectFirstButton()
	{
		if (!RewiredUtilities.HasJoysticks() && ((Component)InputField).gameObject.activeSelf)
		{
			InputField.Select();
		}
		else
		{
			base.SelectFirstButton();
		}
	}

	private void Awake()
	{
		Accept += Destroy;
		Decline += Destroy;
		Cancel += Destroy;
	}

	private void Start()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int playerId = RewiredUtilities.GetPlayerFromInput(Input)?.Id ?? Input.id;
		if (MPExtensions.IsClient(Game.Mode))
		{
			playerId = 1;
		}
		EventManager.RaiseEvent(new DialogueOpenedEvent(playerId, opened: true));
		ControllerFocus.Instance?.Subscribe(this);
		SelectFirstButton();
	}

	private void OnDestroy()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int playerId = RewiredUtilities.GetPlayerFromInput(Input)?.Id ?? Input.id;
		if (MPExtensions.IsClient(Game.Mode))
		{
			playerId = 1;
		}
		EventManager.RaiseEvent(new DialogueOpenedEvent(playerId, opened: false));
		ControllerFocus.Instance?.Unsubscribe(this);
		Player input = Input;
		if (input != null)
		{
			input.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_BackButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 31);
		}
		Player input2 = Input;
		if (input2 != null)
		{
			input2.RemoveInputEventDelegate((Action<InputActionEventData>)Rewired_BackButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 39);
		}
		Accept = null;
		Decline = null;
		Cancel = null;
	}

	public void Destroy()
	{
		BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
	}

	public override void SetInput(Player input)
	{
		base.SetInput(input);
		Input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_BackButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 31);
		Input.AddInputEventDelegate((Action<InputActionEventData>)Rewired_BackButtonPressed, (UpdateLoopType)0, (InputActionEventType)3, 39);
	}

	public void SetType(DialogueType type)
	{
		_dialogueType = type;
		bool active = _dialogueType == DialogueType.Input;
		((Component)InputField).gameObject.SetActive(active);
		SelectFirstButton();
	}

	public void SetIcon(DialogueIcon icon)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (icon == DialogueIcon.Warning)
		{
			Title.Color = Color32.op_Implicit(UGUIColor.TEXT_RED);
			TitleHeaderImage.Color = Color32.op_Implicit(UGUIColor.TEXT_RED);
		}
	}

	public void SetButtons(DialogueButton buttons)
	{
		DestroyButtons();
		_dialogueButtons = buttons;
		switch (_dialogueButtons)
		{
		case DialogueButton.Ok:
		{
			ILabelButtonViewAdapter labelButtonViewAdapter7 = CreateButton();
			labelButtonViewAdapter7.Click += OnAccept;
			labelButtonViewAdapter7.Text = "BUTTON_OK";
			break;
		}
		case DialogueButton.OkCancel:
		{
			ILabelButtonViewAdapter labelButtonViewAdapter8 = CreateButton();
			labelButtonViewAdapter8.Click += OnAccept;
			labelButtonViewAdapter8.Text = "BUTTON_OK";
			ILabelButtonViewAdapter labelButtonViewAdapter9 = CreateButton();
			labelButtonViewAdapter9.Click += OnDecline;
			labelButtonViewAdapter9.Text = "BUTTON_CANCEL";
			break;
		}
		case DialogueButton.YesNo:
		{
			ILabelButtonViewAdapter labelButtonViewAdapter10 = CreateButton();
			labelButtonViewAdapter10.Click += OnAccept;
			labelButtonViewAdapter10.Text = "BUTTON_YES";
			ILabelButtonViewAdapter labelButtonViewAdapter11 = CreateButton();
			labelButtonViewAdapter11.Click += OnDecline;
			labelButtonViewAdapter11.Text = "BUTTON_NO";
			break;
		}
		case DialogueButton.YesNoCancel:
		{
			ILabelButtonViewAdapter labelButtonViewAdapter4 = CreateButton();
			labelButtonViewAdapter4.Click += OnAccept;
			labelButtonViewAdapter4.Text = "BUTTON_YES";
			ILabelButtonViewAdapter labelButtonViewAdapter5 = CreateButton();
			labelButtonViewAdapter5.Click += OnDecline;
			labelButtonViewAdapter5.Text = "BUTTON_NO";
			ILabelButtonViewAdapter labelButtonViewAdapter6 = CreateButton();
			labelButtonViewAdapter6.Click += OnCancel;
			labelButtonViewAdapter6.Text = "BUTTON_CANCEL";
			break;
		}
		case DialogueButton.Custom:
			if (!string.IsNullOrEmpty(Action1))
			{
				ILabelButtonViewAdapter labelButtonViewAdapter = CreateButton();
				labelButtonViewAdapter.Click += OnAccept;
				labelButtonViewAdapter.Text = Action1;
			}
			if (!string.IsNullOrEmpty(Action2))
			{
				ILabelButtonViewAdapter labelButtonViewAdapter2 = CreateButton();
				labelButtonViewAdapter2.Click += OnDecline;
				labelButtonViewAdapter2.Text = Action2;
			}
			if (!string.IsNullOrEmpty(Action3))
			{
				ILabelButtonViewAdapter labelButtonViewAdapter3 = CreateButton();
				labelButtonViewAdapter3.Click += OnCancel;
				labelButtonViewAdapter3.Text = Action3;
			}
			break;
		case DialogueButton.None:
			return;
		}
		SetNavigation();
	}

	private void SetNavigation()
	{
		TMPLabelButtonViewAdapter[] componentsInChildren = ((Component)_dialogueButtonContainer.transform).GetComponentsInChildren<TMPLabelButtonViewAdapter>();
		if (((Component)InputField).gameObject.activeSelf)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				TMPLabelButtonViewAdapter tMPLabelButtonViewAdapter = componentsInChildren[i];
				if (i == 0)
				{
					((Component)InputField).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)tMPLabelButtonViewAdapter).transform);
				}
				((Component)tMPLabelButtonViewAdapter).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)InputField).transform);
			}
		}
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			TMPLabelButtonViewAdapter component = componentsInChildren[j];
			if (j - 1 >= 0)
			{
				TMPLabelButtonViewAdapter element = componentsInChildren[j - 1];
				component.SetNavigation<TMPLabelButtonViewAdapter>(NavigationDirection.Left, element);
			}
			if (j + 1 < componentsInChildren.Length)
			{
				TMPLabelButtonViewAdapter element2 = componentsInChildren[j + 1];
				component.SetNavigation<TMPLabelButtonViewAdapter>(NavigationDirection.Right, element2);
			}
		}
	}

	private ILabelButtonViewAdapter CreateButton()
	{
		TMPLabelButtonViewAdapter tMPLabelButtonViewAdapter = Object.Instantiate<TMPLabelButtonViewAdapter>(_dialogueButtonPrefab);
		((Component)tMPLabelButtonViewAdapter).transform.SetParent(_dialogueButtonContainer.transform, false);
		_buttons.Add(tMPLabelButtonViewAdapter);
		return tMPLabelButtonViewAdapter;
	}

	private void DestroyButtons()
	{
		for (int i = 0; i < _buttons.Count; i++)
		{
			BeamTeam.Destroy<GameObject>(((Component)_buttons[i]).gameObject);
		}
		_buttons.Clear();
	}

	private void Rewired_BackButtonPressed(InputActionEventData data)
	{
		switch (_dialogueButtons)
		{
		case DialogueButton.OkCancel:
		case DialogueButton.YesNo:
			_buttons[1].PerformClick();
			break;
		case DialogueButton.YesNoCancel:
		case DialogueButton.Custom:
			_buttons[2].PerformClick();
			break;
		}
	}
}
