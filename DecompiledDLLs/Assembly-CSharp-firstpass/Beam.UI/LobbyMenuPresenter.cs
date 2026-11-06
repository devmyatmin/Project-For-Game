using System;
using System.Collections;
using System.Collections.Generic;
using Beam.Events;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using Funlabs;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using Rewired;
using UdpKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI;

public class LobbyMenuPresenter : GlobalEventListener
{
	[SerializeField]
	private ULobbyMenuViewAdapter _view;

	[SerializeField]
	private MainMenuPresenter _mainMenuPresenter;

	private ILobbyElement _firstLobbyElement;

	private Player _currentPlayerInput;

	private string _joinSessionCodePayload;

	private int _regionIndex;

	private int _previousRegionIndex = -1;

	private bool _isJoinInProgress;

	private bool _isChangingRegions;

	private readonly float WAIT_FOR_SESSION_LIST_TIME = 3f;

	private readonly float SESSION_LIST_TIMEOUT = 30f;

	private readonly float JOIN_TIMEOUT = 30f;

	private readonly float SESSION_CHECK_TIME_THRESHOLD = 5f;

	private DialogueBoxViewAdapter _sessionDialog;

	private DialogueBoxViewAdapter _joinDialog;

	private DialogueBoxViewAdapter _enterCodeDialog;

	private Coroutine _connectingCoroutine;

	private Coroutine _waitForSessionListCoroutine;

	private Map<Guid, UdpSession> _phothonSessionList;

	private Session_Filter _selectedFilter;

	private TMPLabelButtonViewAdapter _selectedFilterWidget;

	private Guid _selectedSessionGuid;

	public event Action ShowView;

	public event Action HideView;

	private void Start()
	{
		EventManager.AddListener<PlayerConnectedEvent>(OnConnected);
		EventManager.AddListener<PlayerDisconnectedEvent>(OnDisconnect);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<PlayerConnectedEvent>(OnConnected);
		EventManager.RemoveListener<PlayerDisconnectedEvent>(OnDisconnect);
		ShowView = null;
		HideView = null;
	}

	private void OnError(string error)
	{
		if (_view.Visible)
		{
			if (Object.op_Implicit((Object)(object)_joinDialog))
			{
				_joinDialog.Destroy();
				_joinDialog = null;
			}
			if (Object.op_Implicit((Object)(object)_sessionDialog))
			{
				_sessionDialog.Destroy();
				_sessionDialog = null;
			}
			_isChangingRegions = false;
			_joinDialog = DialogueBox.Show(_currentPlayerInput, "PLAY_ONLINE_MENU_LOBBY_DIALOG_TIMEOUT_TITLE", error, DialogueButton.Ok, DialogueIcon.Warning);
			_joinDialog.Accept += () =>
			{
				BackButtonAction(ignoreDialogueBox: true);
			};
		}
	}

	private void View_ShowView()
	{
		ShowView?.Invoke();
	}

	private void View_HideView()
	{
		HideView?.Invoke();
	}

	public void Show(Player input)
	{
		_view.CleanButtons();
		if (_view.Visible)
		{
			return;
		}
		_currentPlayerInput = input;
		_phothonSessionList = new Map<Guid, UdpSession>();
		ShowSessionsDialog("PLAY_ONLINE_MENU_LOBBY_DIALOG_LOADING_TITLE", "PLAY_ONLINE_MENU_LOBBY_DIALOG_LOADING_MESSAGE");
		if (BoltNetwork.IsRunning)
		{
			Debug.LogError((object)"Lobby: BoltNetwork is already running");
			return;
		}
		Game.Mode = (GameMode)5;
		MultiplayerMng.AuthenticateUser(() =>
		{
			MultiplayerMng.ConnectToCloud(Game.MultiplayerSessionRegionIndex, BoltOnSuccess, OnError);
		}, () =>
		{
			OnError("AUTH_TOKEN_USER_FAILED");
		});
		_connectingCoroutine = ((MonoBehaviour)this).StartCoroutine(WaitAndTimeOut(SESSION_LIST_TIMEOUT));
		_view.ShowView += View_ShowView;
		_view.HideView += View_HideView;
		_view.EnterCodeButton.Click += EnterCodeButton_Click;
		_view.BackButton.Click += BackButton_Click;
		_view.ApplyButton.Click += ApplyButton_Click;
		CreateLobbyButtons();
		_view.Show();
		_selectedFilterWidget = _view.GetFirstButton() as TMPLabelButtonViewAdapter;
		_selectedFilterWidget.Select();
		EventSystem.main.SetSelectedGameObject(((Component)((Component)_selectedFilterWidget).transform).gameObject);
		SubscribeToInputEvents();
		_view.RegionOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(CloudRegion)));
		_view.RegionOptionButton.SelectedIndex = MPExtensions.RegionIndexToCloudRegionEnumIndex(Game.MultiplayerSessionRegionIndex);
		_view.RegionOptionButton.SelectionChanged += RegionOptionButton_Click;
	}

	public void Hide()
	{
		EventManager.RemoveListener<PlayerDisconnectedEvent>(OnDisconnect);
		if (_view.Visible)
		{
			_phothonSessionList = null;
			UnsubscribeFromInputEvents();
			_view.Hide();
			_view.ShowView -= View_ShowView;
			_view.HideView -= View_HideView;
			_view.EnterCodeButton.Click -= EnterCodeButton_Click;
			_view.BackButton.Click -= BackButton_Click;
			_view.ApplyButton.Click -= ApplyButton_Click;
			_firstLobbyElement = null;
			_currentPlayerInput = null;
			_phothonSessionList = new Map<Guid, UdpSession>();
		}
	}

	public void BoltOnSuccess()
	{
		_isChangingRegions = false;
		_view.RegionLabel.Text = MultiplayerMng.GetCloudRegion() ?? "";
		StopConnectingCoroutine();
		_waitForSessionListCoroutine = ((MonoBehaviour)this).StartCoroutine(WaitAndRemoveLoadingDialog(WAIT_FOR_SESSION_LIST_TIME));
	}

	private IEnumerator WaitAndTimeOut(float waitTime)
	{
		yield return (object)new WaitForSeconds(waitTime);
		if (Object.op_Implicit((Object)(object)_sessionDialog))
		{
			_sessionDialog.Destroy();
			_sessionDialog = null;
		}
		if (Object.op_Implicit((Object)(object)_joinDialog))
		{
			_joinDialog.Destroy();
			_joinDialog = null;
		}
		_sessionDialog = DialogueBox.Show(_currentPlayerInput, "PLAY_ONLINE_MENU_LOBBY_DIALOG_TIMEOUT_TITLE", "PLAY_ONLINE_MENU_LOBBY_DIALOG_TIMEOUT_MESSAGE", DialogueButton.Ok, DialogueIcon.Warning);
		_sessionDialog.Accept += () =>
		{
			BackButtonAction(ignoreDialogueBox: true);
		};
	}

	private IEnumerator WaitAndRemoveLoadingDialog(float waitTime)
	{
		yield return (object)new WaitForSeconds(waitTime);
		if (!string.IsNullOrEmpty(_joinSessionCodePayload))
		{
			_joinSessionCodePayload = null;
			NoRoomCodeDialog();
			yield break;
		}
		if (Object.op_Implicit((Object)(object)_sessionDialog))
		{
			_sessionDialog.Destroy();
			_sessionDialog = null;
		}
		_view.SelectFirstButton();
	}

	public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
	{
		if (BeamTeam.IsValid(_waitForSessionListCoroutine))
		{
			((MonoBehaviour)this).StopCoroutine(_waitForSessionListCoroutine);
		}
		bool flag = SameVisualSessions(_phothonSessionList, sessionList);
		_phothonSessionList = sessionList;
		if (_view.Visible)
		{
			if (flag)
			{
				UpdateSessionButtonsData(_phothonSessionList);
			}
			else
			{
				UpdateSessionButtons(_phothonSessionList);
			}
			if ((Object)(object)_sessionDialog != (Object)null)
			{
				_sessionDialog.Destroy();
			}
			if (!string.IsNullOrEmpty(_joinSessionCodePayload))
			{
				JoinSessionByCode(_joinSessionCodePayload);
				_joinSessionCodePayload = null;
			}
		}
	}

	private bool SameVisualSessions(Map<Guid, UdpSession> a, Map<Guid, UdpSession> b)
	{
		if (BeamTeam.IsNullOrDestroyed(a) != BeamTeam.IsNullOrDestroyed(b))
		{
			return false;
		}
		if (BeamTeam.IsNullOrDestroyed(a))
		{
			return false;
		}
		Dictionary<Guid, UdpSession> aSessions = new Dictionary<Guid, UdpSession>();
		Dictionary<Guid, UdpSession> bSessions = new Dictionary<Guid, UdpSession>();
		Beam.Utilities.ExtensionMethods.ForEach_NonAlloc((IEnumerable<KeyValuePair<Guid, UdpSession>>)a, (KeyValuePair<Guid, UdpSession> sessionPair) =>
		{
			if (BeamTeam.IsValid(MPExtensions.GetToken(sessionPair.Value)))
			{
				aSessions.Add(sessionPair.Key, sessionPair.Value);
			}
		});
		Beam.Utilities.ExtensionMethods.ForEach_NonAlloc((IEnumerable<KeyValuePair<Guid, UdpSession>>)b, (KeyValuePair<Guid, UdpSession> sessionPair) =>
		{
			if (BeamTeam.IsValid(MPExtensions.GetToken(sessionPair.Value)))
			{
				bSessions.Add(sessionPair.Key, sessionPair.Value);
			}
		});
		if (aSessions.Count != bSessions.Count)
		{
			return false;
		}
		foreach (KeyValuePair<Guid, UdpSession> item in aSessions)
		{
			if (!bSessions.TryGetValue(item.Key, out var _))
			{
				return false;
			}
		}
		return true;
	}

	private void UpdateSessionButtonsData(Map<Guid, UdpSession> sessionList)
	{
		_view.UpdateLobbydEntries(sessionList);
	}

	private void UpdateSessionButtons(Map<Guid, UdpSession> sessionList)
	{
		List<UdpSession> sessionList2 = FilterSessionList(sessionList);
		ULobbyEntryElement uLobbyEntryElement = _view.CreateLobbydEntries(sessionList2, _selectedFilter, _selectedSessionGuid, ButtonSession_Click, ButtonSession_Selected);
		if (Object.op_Implicit((Object)(object)uLobbyEntryElement))
		{
			_selectedSessionGuid = uLobbyEntryElement.GetGuid();
			((uLobbyEntryElement != null) ? ((Component)uLobbyEntryElement).gameObject.GetComponent<TMPLabelButtonViewAdapter>() : null)?.Select();
		}
	}

	public List<UdpSession> FilterSessionList(Map<Guid, UdpSession> sessionList)
	{
		List<UdpSession> list = new List<UdpSession>();
		foreach (KeyValuePair<Guid, UdpSession> session in sessionList)
		{
			UdpSession value = session.Value;
			if (MPExtensions.GetToken(value) != null)
			{
				list.Add(value);
			}
		}
		switch (_selectedFilter)
		{
		case Session_Filter.MOST_TIME_PLAYED:
			list.Sort((UdpSession a, UdpSession b) => MPExtensions.GetToken(b).TimePlayed.CompareTo(MPExtensions.GetToken(a).TimePlayed));
			break;
		case Session_Filter.MOST_DAYS_SURVIVED:
			list.Sort((UdpSession a, UdpSession b) => MPExtensions.GetToken(b).DaysSurvived.CompareTo(MPExtensions.GetToken(a).DaysSurvived));
			break;
		case Session_Filter.QUICKEST_ESCAPE:
			list.Sort((UdpSession a, UdpSession b) => MPExtensions.GetToken(b).QuickestScape.CompareTo(MPExtensions.GetToken(a).DaysSurvived));
			break;
		case Session_Filter.MOST_ISLANDS_HOPPED:
			list.Sort((UdpSession a, UdpSession b) => MPExtensions.GetToken(b).IslandsHopped.CompareTo(MPExtensions.GetToken(a).IslandsHopped));
			break;
		case Session_Filter.MOST_ISLANDS_EXPLORED:
			list.Sort((UdpSession a, UdpSession b) => MPExtensions.GetToken(b).IslandsExplored.CompareTo(MPExtensions.GetToken(a).IslandsExplored));
			break;
		case Session_Filter.MOST_FISH_FISHED:
			list.Sort((UdpSession a, UdpSession b) => MPExtensions.GetToken(b).FishFished.CompareTo(MPExtensions.GetToken(a).FishFished));
			break;
		case Session_Filter.MOST_ANIMALS_KILLED:
			list.Sort((UdpSession a, UdpSession b) => MPExtensions.GetToken(b).EnemiesKilled.CompareTo(MPExtensions.GetToken(a).EnemiesKilled));
			break;
		case Session_Filter.MOST_ANIMALS_TRAPPED:
			list.Sort((UdpSession a, UdpSession b) => MPExtensions.GetToken(b).AnimalsTrapped.CompareTo(MPExtensions.GetToken(a).AnimalsTrapped));
			break;
		case Session_Filter.MOST_ITEMS_CRAFTED:
			list.Sort((UdpSession a, UdpSession b) => MPExtensions.GetToken(b).ItemsCrafted.CompareTo(MPExtensions.GetToken(a).ItemsCrafted));
			break;
		}
		return list;
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

	private void LobbyButton_Click(object sender)
	{
		if (DialogueBox.Notifying)
		{
			return;
		}
		_selectedSessionGuid = Guid.Empty;
		if (sender is ILobbyElement { SessionFilter: var sessionFilter } lobbyElement)
		{
			Debug.Log((object)sessionFilter.ToString());
			_selectedFilter = lobbyElement.SessionFilter;
			if (!BeamTeam.IsNullOrDestroyed(_selectedFilterWidget))
			{
				((Component)_selectedFilterWidget).GetComponent<Animator>().Play("Normal");
			}
			_selectedFilterWidget = lobbyElement as TMPLabelButtonViewAdapter;
			if (_phothonSessionList != null)
			{
				UpdateSessionButtons(_phothonSessionList);
			}
		}
	}

	private void Rewired_BackButtonPressed(InputActionEventData data)
	{
		BackButton_Click(null);
	}

	private void EnterCodeButton_Click(object sender)
	{
		if (!DialogueBox.Notifying)
		{
			_enterCodeDialog = DialogueBox.Show(_currentPlayerInput, "BUTTON_ENTER_CODE", string.Empty, DialogueButton.OkCancel, DialogueIcon.None, DialogueType.Input);
			_enterCodeDialog.InputField.CharacterLimit = 20;
			_enterCodeDialog.InputField.Text = "";
			_enterCodeDialog.InputField.ValueChanged += EnterCodeDialog_ValueChanged;
			_enterCodeDialog.InputField.KeyDown += DialogueInputField_KeyDown;
			_enterCodeDialog.Accept += EnterCodeDialog_Accept;
			_enterCodeDialog.Decline += EnterCodeDialog_Hide;
		}
	}

	private void EnterCodeDialog_ValueChanged(string value)
	{
		_enterCodeDialog.InputField.Text = value.ToLower();
	}

	private void DialogueInputField_KeyDown(string value)
	{
		if (Input.GetKey((KeyCode)13))
		{
			EnterCodeDialog_Accept();
		}
	}

	private void EnterCodeDialog_Accept()
	{
		string text = _enterCodeDialog.InputField.Text;
		EnterCodeDialog_Hide();
		JoinSessionByCode(text);
	}

	private void JoinSessionByCode(string code)
	{
		int regionIndex = MPExtensions.GetRegionIndex(code);
		if (regionIndex == -1)
		{
			_view.SelectFirstButton();
			return;
		}
		if (regionIndex == Game.MultiplayerSessionRegionIndex)
		{
			FindAndJoinSessionByCode(code);
			return;
		}
		_joinSessionCodePayload = code;
		ChangeRegion(regionIndex);
	}

	private void FindAndJoinSessionByCode(string code)
	{
		UdpSession sessionByCode = GetSessionByCode(code);
		if (BeamTeam.IsNullOrDestroyed(sessionByCode))
		{
			NoRoomCodeDialog();
			_view.SelectFirstButton();
		}
		else
		{
			JoinSession(sessionByCode);
		}
	}

	private void NoRoomCodeDialog()
	{
		if (Object.op_Implicit((Object)(object)_sessionDialog))
		{
			_sessionDialog.Destroy();
			_sessionDialog = null;
		}
		if (Object.op_Implicit((Object)(object)_joinDialog))
		{
			_joinDialog.Destroy();
			_joinDialog = null;
		}
		_joinSessionCodePayload = null;
		DialogueBox.Show(ReInput.players.GetPlayer(0), "WRONG_CODE_TITLE", "WRONG_CODE_MESSAGE", DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
		{
			ChangeRegion(_previousRegionIndex);
		};
	}

	private void EnterCodeDialog_Hide()
	{
		_enterCodeDialog.InputField.ValueChanged -= EnterCodeDialog_ValueChanged;
		_enterCodeDialog.InputField.KeyDown -= DialogueInputField_KeyDown;
		_enterCodeDialog.Accept -= EnterCodeDialog_Accept;
		_enterCodeDialog.Decline -= EnterCodeDialog_Hide;
		_enterCodeDialog.Destroy();
		_enterCodeDialog = null;
		_view.SelectFirstButton();
	}

	private void BackButton_Click(object sender)
	{
		BackButtonAction();
	}

	private void BackButtonAction(bool ignoreDialogueBox = false)
	{
		if (!DialogueBox.Notifying || ignoreDialogueBox)
		{
			Game.Mode = (GameMode)0;
			EventManager.RemoveListener<PlayerDisconnectedEvent>(OnDisconnect);
			PlayerRegistry.LocalPeer.ReplicatedDisconnect(DisconnectDetail.Intended, null, forceLocalShutdown: true);
			Hide();
		}
	}

	private void ApplyButton_Click(object sender)
	{
		if (!DialogueBox.Notifying)
		{
			_phothonSessionList = new Map<Guid, UdpSession>();
			_view.CleanButtons();
			ChangeRegion(_regionIndex);
		}
	}

	private void ChangeRegion(int regionIndex)
	{
		_previousRegionIndex = _regionIndex;
		_regionIndex = regionIndex;
		if (_regionIndex == Game.MultiplayerSessionRegionIndex || !Enum.IsDefined(typeof(CloudRegion), _regionIndex))
		{
			NoRoomCodeDialog();
			return;
		}
		_isChangingRegions = true;
		((Component)_view.ApplyButton.transform).gameObject.SetActive(false);
		ShowSessionsDialog("PLAY_ONLINE_MENU_LOBBY_DIALOG_LOADING_TITLE", "PLAY_ONLINE_MENU_LOBBY_DIALOG_LOADING_MESSAGE");
		PlayerRegistry.LocalPeer.ReplicatedDisconnect(DisconnectDetail.Intended, null, forceLocalShutdown: true);
		Game.MultiplayerSessionRegionIndex = _regionIndex;
		Game.Mode = (GameMode)5;
		MultiplayerMng.ConnectToCloud(Game.MultiplayerSessionRegionIndex, BoltOnSuccess, (string error) =>
		{
			OnError(error);
		});
	}

	private void CreateLobbyButtons()
	{
		foreach (Session_Filter value in Enum.GetValues(typeof(Session_Filter)))
		{
			if (value != Session_Filter.QUICKEST_ESCAPE)
			{
				_view.CreateLobbyButton(value).Click += LobbyButton_Click;
			}
		}
		if (BeamTeam.IsNullOrDestroyed(_firstLobbyElement))
		{
			_firstLobbyElement = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_view.LobyButtons);
			_firstLobbyElement?.PerformClick();
			_view.SelectFirstButton();
		}
	}

	private void RegionOptionButton_Click(object obj)
	{
		if (!DialogueBox.Notifying)
		{
			int selectedIndex = _view.RegionOptionButton.SelectedIndex;
			_regionIndex = MPExtensions.CloudRegionEnumIndexToRegionIndex(selectedIndex);
			((Component)_view.ApplyButton.transform).gameObject.SetActive(_regionIndex != Game.MultiplayerSessionRegionIndex);
		}
	}

	private void ButtonSession_Click(object widget)
	{
		if (!DialogueBox.Notifying)
		{
			UdpSession session = ((Component)(widget as TMPLabelButtonViewAdapter)).gameObject.GetComponent<ULobbyEntryElement>().getSession();
			JoinSession(session);
		}
	}

	private void ButtonSession_Selected(ISelectableViewAdapter obj)
	{
		_view.Scrollview.FocusElement(obj.transform);
		ULobbyEntryElement component = ((Component)obj.transform).GetComponent<ULobbyEntryElement>();
		if (Object.op_Implicit((Object)(object)component))
		{
			_selectedSessionGuid = component.GetGuid();
		}
	}

	private void JoinSession(UdpSession session)
	{
		if (MPExtensions.GetToken(session).IsJoinable())
		{
			_connectingCoroutine = ((MonoBehaviour)this).StartCoroutine(JoinSession_Sequence(session));
		}
	}

	private IEnumerator JoinSession_Sequence(UdpSession session)
	{
		Guid id = session.Id;
		if (Object.op_Implicit((Object)(object)_sessionDialog))
		{
			_sessionDialog.Destroy();
			_sessionDialog = null;
		}
		_joinDialog = DialogueBox.Show(_currentPlayerInput, "PLAY_ONLINE_MENU_LOBBY_DIALOG_LOADING_TITLE", "PLAY_ONLINE_MENU_LOBBY_DIALOG_JOINING_MESSAGE", DialogueButton.None);
		yield return (object)new WaitForSeconds(SESSION_CHECK_TIME_THRESHOLD);
		session = GetSessionById(id);
		if (session == null || MPExtensions.GetToken(session) == null || !MPExtensions.GetToken(session).IsJoinable())
		{
			if (Object.op_Implicit((Object)(object)_joinDialog))
			{
				_joinDialog.Destroy();
				_joinDialog = null;
			}
			yield break;
		}
		_mainMenuPresenter.CheckOnlineAvailable(() =>
		{
			BoltMatchmaking.JoinSession(session, (IProtocolToken)(object)MultiplayerMng.JoinToken);
			_connectingCoroutine = ((MonoBehaviour)this).StartCoroutine(WaitAndTimeOut(JOIN_TIMEOUT));
		}, () =>
		{
			BackButtonAction(ignoreDialogueBox: true);
		}, () =>
		{
			StopConnectingCoroutine();
			if (Object.op_Implicit((Object)(object)_joinDialog))
			{
				_joinDialog.Destroy();
				_joinDialog = null;
			}
		});
	}

	public override void ConnectRefused(UdpEndPoint endpoint, IProtocolToken token)
	{
		if (_view.Visible)
		{
			OnError("HOST_NO_RESPONSE");
			StopConnectingCoroutine();
		}
	}

	public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (_view.Visible)
		{
			OnError(((object)(UdpSessionError)5/*cast due to constrained. prefix*/).ToString());
			StopConnectingCoroutine();
		}
	}

	public override void SessionConnectFailed(UdpSession session, IProtocolToken token, UdpSessionError errorReason)
	{
		if (_view.Visible)
		{
			OnError(((object)errorReason/*cast due to constrained. prefix*/).ToString());
			StopConnectingCoroutine();
		}
	}

	private void OnConnected(PlayerConnectedEvent @event)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (_view.Visible)
		{
			_isJoinInProgress = MPExtensions.IsGame(MultiplayerMng.ServerGameState);
		}
	}

	public override void SessionConnected(UdpSession session, IProtocolToken token)
	{
		if (BeamTeam.IsValid(_connectingCoroutine))
		{
			StopConnectingCoroutine();
		}
		Hide();
		if (Object.op_Implicit((Object)(object)_sessionDialog))
		{
			_sessionDialog.Destroy();
			_sessionDialog = null;
		}
		if (Object.op_Implicit((Object)(object)_joinDialog))
		{
			_joinDialog.Destroy();
			_joinDialog = null;
		}
		if (_isJoinInProgress)
		{
			_mainMenuPresenter.StartGame((GameMode)5, (GameState)3, 3);
			return;
		}
		PlayerRegistry.OnPeerPresent(0, () =>
		{
			_mainMenuPresenter.ShowNewGameMenuPresenter();
		});
	}

	public void StopConnectingCoroutine()
	{
		if (_connectingCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_connectingCoroutine);
			_connectingCoroutine = null;
		}
	}

	private UdpSession GetSessionById(Guid id)
	{
		UdpSession result = null;
		_phothonSessionList?.TryFind(id, ref result);
		return result;
	}

	private UdpSession GetSessionByCode(string code)
	{
		string a = code.ToLower();
		int cloudRegionIndex = MultiplayerMng.GetCloudRegionIndex();
		if (_phothonSessionList == null)
		{
			return null;
		}
		foreach (KeyValuePair<Guid, UdpSession> phothonSession in _phothonSessionList)
		{
			MultiplayerTokens.Session token = MPExtensions.GetToken(phothonSession.Value);
			object obj;
			if (token == null)
			{
				obj = null;
			}
			else
			{
				string sessionId = token.SessionId;
				obj = ((sessionId != null) ? MPExtensions.ToCode(sessionId, cloudRegionIndex) : null);
			}
			string text = (string)obj;
			if (!string.IsNullOrEmpty(text) && string.Equals(a, text.ToLower()))
			{
				return phothonSession.Value;
			}
		}
		return null;
	}

	private void ShowSessionsDialog(string title, string message)
	{
		_sessionDialog = DialogueBox.Show(_currentPlayerInput, title, message, DialogueButton.None);
	}

	private void Update()
	{
		if (_view.Visible)
		{
			if (Object.op_Implicit((Object)(object)_selectedFilterWidget))
			{
				((Component)((Component)_selectedFilterWidget).transform).GetComponent<Animator>().Play("Highlighted");
			}
			if (BeamTeam.IsNullOrDestroyed(_enterCodeDialog) && Input.GetKey((KeyCode)118))
			{
				EnterCodeButton_Click(null);
			}
		}
	}

	private void OnDisconnect(PlayerDisconnectedEvent @event)
	{
		if (_view.Visible && !_isChangingRegions)
		{
			DialogueBox.Show(_currentPlayerInput, "TEXT_ERROR", "PLAY_ONLINE_MENU_CREATE_SESSION_CREATING_ERROR_CONNECTING", DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				BackButtonAction(ignoreDialogueBox: true);
			};
		}
	}
}
