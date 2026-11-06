using System;
using System.Linq;
using System.Text.RegularExpressions;
using Beam.AccountServices;
using Beam.Language;
using Beam.Serialization.Json;
using Beam.Terrain;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using Funlabs;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI;

public class CreateSessionMenuPresenter : MonoBehaviour
{
	public enum HostVisibility
	{
		[Description("PLAY_ONLINE_MENU_BUTTON_HOST_VISIBILITY_PUBLIC", "PLAY_ONLINE_MENU_BUTTON_HOST_VISIBILITY_PUBLIC_DESC")]
		Public,
		[Description("PLAY_ONLINE_MENU_BUTTON_HOST_VISIBILITY_PRIVATE", "PLAY_ONLINE_MENU_BUTTON_HOST_VISIBILITY_PRIVATE_DESC")]
		Private
	}

	[SerializeField]
	private CreateSessionMenuViewAdapterBase _view;

	[SerializeField]
	private MainMenuPresenter _mainMenuPresenter;

	private Player _currentPlayerInput;

	private DialogueBoxViewAdapter errorDialog;

	private IViewAdapter _parent;

	public Action<GameSettings> CreateCallback;

	private JObject _badWordsData;

	public event Action ShowView;

	public event Action HideView;

	public void Initialize(IViewAdapter parent)
	{
		_parent = parent;
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView += Hide;
		}
		Player player = ReInput.players.GetPlayer(0);
		_currentPlayerInput = player;
	}

	private void Awake()
	{
		PopulateControls();
		SetControlsSelections();
		_view.ShowView += View_ShowView;
		_view.HideView += View_HideView;
		_view.CreateButton.Click += CreateButton_Click;
		_view.BackButton.Click += BackButton_Click;
		_view.VisibilityOptionButton.SelectionChanged += HostVisiblityButton_Click;
		_view.RegionOptionButton.SelectionChanged += HostRegionButton_Click;
		WireButtonEvents(_view.VisibilityOptionButton);
		WireButtonEvents(_view.RegionOptionButton);
		_badWordsData = LoadBadWordsData();
		_view.Hide();
	}

	private void OnDestroy()
	{
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView -= Hide;
		}
		_view.ShowView -= View_ShowView;
		_view.HideView -= View_HideView;
		_view.CreateButton.Click -= CreateButton_Click;
		_view.BackButton.Click -= BackButton_Click;
		UnwireButtonEvents(_view.VisibilityOptionButton);
		UnwireButtonEvents(_view.RegionOptionButton);
		ShowView = null;
		HideView = null;
		CreateCallback = null;
	}

	private void PopulateControls()
	{
		_view.VisibilityOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(HostVisibility)));
		_view.RegionOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(CloudRegion)));
	}

	private void SetControlsSelections()
	{
		_view.VisibilityOptionButton.SelectedIndex = (Game.MultiplayerSessionPrivate ? Reflection.GetIndex(HostVisibility.Private) : Reflection.GetIndex(HostVisibility.Public));
		_view.RegionOptionButton.SelectedIndex = MPExtensions.RegionIndexToCloudRegionEnumIndex(Game.MultiplayerSessionRegionIndex);
	}

	private void WireWidgetEvents(IOptionWidgetViewAdapter widget)
	{
		widget.Selected += OptionWidget_Selected;
		widget.MouseEnter += OptionWidget_MouseEnter;
		widget.MouseLeave += OptionWidget_MouseLeave;
	}

	private void WireButtonEvents(IOptionButtonViewAdapter button)
	{
		button.SelectionChanged += OptionButton_SelectionChanged;
		WireWidgetEvents(button);
	}

	private void UnwireWidgetEvents(IOptionWidgetViewAdapter widget)
	{
		widget.Selected -= OptionWidget_Selected;
		widget.MouseEnter -= OptionWidget_MouseEnter;
		widget.MouseLeave -= OptionWidget_MouseLeave;
	}

	private void UnwireButtonEvents(IOptionButtonViewAdapter button)
	{
		button.SelectionChanged -= OptionButton_SelectionChanged;
		UnwireWidgetEvents(button);
	}

	public void Show(Player input)
	{
		ControllerFocus.Instance?.Subscribe(_view);
		_currentPlayerInput = input;
		Game.MultiplayerSessionRegionIndex = 0;
		_view.RegionOptionButton.SelectedIndex = MPExtensions.RegionIndexToCloudRegionEnumIndex(Game.MultiplayerSessionRegionIndex);
		string defaultSessionName = GetDefaultSessionName();
		_view.InputField.Text = defaultSessionName;
		_view.Show();
		_view.InputField.Select();
	}

	public void Hide()
	{
		ControllerFocus.Instance?.Unsubscribe(_view);
		_view.Hide();
	}

	private void View_ShowView()
	{
		ShowView?.Invoke();
	}

	private void View_HideView()
	{
		HideView?.Invoke();
	}

	private void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		if ((player.GetButtonDown(39) || player.GetButtonDown(31)) && _view.Visible)
		{
			BackButton_Click(null);
		}
		if ((Object)(object)EventSystem.main.currentSelectedGameObject == (Object)(object)((Component)_view.InputField.transform).gameObject && player.GetButtonDown(30))
		{
			_view.VisibilityOptionButton.Select();
		}
	}

	private string GetDefaultSessionName()
	{
		return Localization.GetLanguageHandler().Localize("HOST_GAME_MENU_TEXT_LOBBY_NAME", GAccountService.Get().LocalUser.UserInfo.DisplayName);
	}

	private void CreateButton_Click(object sender)
	{
		_mainMenuPresenter.CheckOnlineAvailable(CreateSession, _view.SelectFirstButton);
	}

	private void CreateSession()
	{
		_view.SetAllButtonInteractability(state: false);
		string text = _view.InputField.Text;
		ValidateSessionName(text);
	}

	private void ValidateSessionName(string sessionName)
	{
		if (string.IsNullOrWhiteSpace(sessionName))
		{
			sessionName = GetDefaultSessionName();
		}
		sessionName = ValidateText(sessionName);
		_view.InputField.Text = sessionName;
		OnValidateSessionNameSucceeded(_view.InputField.Text);
	}

	private void OnValidateSessionNameSucceeded(string sessionName)
	{
		bool hasUgc = World.MapList.Any((Map map) => map.IsUserMap());
		MultiplayerMng.ClearBlockedClientList();
		Game.Mode = (GameMode)4;
		Game.MultiplayerSessionName = Guid.NewGuid().ToString();
		MultiplayerTokens.Session token = new MultiplayerTokens.Session
		{
			SessionId = Game.MultiplayerSessionName,
			Name = sessionName,
			IsPrivate = Game.MultiplayerSessionPrivate,
			HasUgc = hasUgc
		};
		MultiplayerMng.AuthenticateUser(() =>
		{
			MultiplayerMng.ConnectToCloud(Game.MultiplayerSessionRegionIndex, () =>
			{
				token.RegionIndex = MultiplayerMng.GetCloudRegionIndex();
				MultiplayerMng.ConnectToSession(token.SessionId, token, OnJoin, OnJoinError);
			}, OnJoinError);
		}, () =>
		{
			OnJoinError("AUTH_TOKEN_USER_FAILED");
		});
		_mainMenuPresenter.ShowJoinSessionDialog(OnJoinTimeout);
	}

	private void OnJoin()
	{
		MultiplayerMng.UpdateSessionToken_Player(SaveManager.ValidateSaveFile((GameMode)4) ? SaveManager.LoadSaveFile((GameMode)4) : null);
		_mainMenuPresenter.HideJoinSessionDialog();
		Hide();
		_mainMenuPresenter.ShowNewGameMenuPresenter();
	}

	private void OnJoinTimeout()
	{
		if (!Object.op_Implicit((Object)(object)errorDialog))
		{
			_mainMenuPresenter.HideJoinSessionDialog();
			errorDialog = DialogueBox.Show(_currentPlayerInput, "PLAY_ONLINE_MENU_CREATE_SESSION_CREATING_TIMEOUT", "PLAY_ONLINE_MENU_CREATE_SESSION_CREATING_ERROR_CONNECTING", DialogueButton.Ok, DialogueIcon.Warning);
			errorDialog.Accept += () =>
			{
				PlayerRegistry.LocalPeer.ReplicatedDisconnect(DisconnectDetail.Intended, null, forceLocalShutdown: true);
				_view.SetAllButtonInteractability(state: true);
			};
		}
	}

	private void OnJoinError(string error)
	{
		if (!Object.op_Implicit((Object)(object)errorDialog))
		{
			_mainMenuPresenter.HideJoinSessionDialog();
			errorDialog = DialogueBox.Show(_currentPlayerInput, "PLAY_ONLINE_MENU_CREATE_SESSION_CREATING_ERROR", error, DialogueButton.Ok, DialogueIcon.Warning);
			errorDialog.Accept += () =>
			{
				PlayerRegistry.LocalPeer.ReplicatedDisconnect(DisconnectDetail.Intended, null, forceLocalShutdown: true);
				_view.SetAllButtonInteractability(state: true);
			};
		}
	}

	private void BackButton_Click(object sender)
	{
		if (!DialogueBox.Notifying)
		{
			Hide();
		}
	}

	private void OptionWidget_Selected(ISelectableViewAdapter selectable)
	{
		IOptionWidgetViewAdapter optionWidgetViewAdapter = selectable as IOptionWidgetViewAdapter;
		if (!BeamTeam.IsNullOrDestroyed(optionWidgetViewAdapter))
		{
			string tooltip = optionWidgetViewAdapter.GetTooltip();
			_view.SetTooltip(tooltip);
		}
	}

	private void OptionWidget_MouseEnter(ISelectableViewAdapter selectable)
	{
		IOptionWidgetViewAdapter optionWidgetViewAdapter = selectable as IOptionWidgetViewAdapter;
		if (!BeamTeam.IsNullOrDestroyed(optionWidgetViewAdapter))
		{
			string tooltip = optionWidgetViewAdapter.GetTooltip();
			_view.SetTooltip(tooltip);
		}
	}

	private void OptionWidget_MouseLeave(ISelectableViewAdapter selectable)
	{
		_view.SetTooltip(string.Empty);
	}

	private void OptionButton_SelectionChanged(IOptionButtonViewAdapter button)
	{
		_view.SetTooltip(button.GetTooltip());
	}

	private void HostVisiblityButton_Click(object obj)
	{
		Game.MultiplayerSessionPrivate = _view.VisibilityOptionButton.SelectedIndex == 1;
	}

	private void HostRegionButton_Click(object obj)
	{
		Game.MultiplayerSessionRegionIndex = MPExtensions.CloudRegionEnumIndexToRegionIndex(_view.RegionOptionButton.SelectedIndex);
	}

	private string ValidateText(string value)
	{
		for (int i = 0; i < _badWordsData.Children.Count; i++)
		{
			JObject jObject = _badWordsData.Children[i];
			for (int j = 0; j < jObject.Children.Count; j++)
			{
				string text = jObject.Children[j].Data.ToString();
				int length = text.Length;
				if (value.Contains(text, StringComparison.OrdinalIgnoreCase))
				{
					string text2 = "";
					for (int k = 0; k < length; k++)
					{
						text2 += "*";
					}
					string pattern = "\\b" + text + "\\b";
					value = Regex.Replace(value, pattern, text2, RegexOptions.IgnoreCase);
				}
			}
		}
		return value;
	}

	private JObject LoadBadWordsData()
	{
		TextAsset val = Resources.Load<TextAsset>("bad-words");
		if ((Object)(object)val != (Object)null)
		{
			JObject jObject = new JObject(((object)val).ToString());
			Resources.UnloadAsset((Object)(object)val);
			if (!jObject.IsNull())
			{
				return jObject;
			}
			Debug.LogError((object)"CreateSessionMenuPresenter:: badWordsData data was empty");
			return null;
		}
		Debug.LogError((object)"CreateSessionMenuPresenter:: Error loading badWords file.");
		return null;
	}
}
