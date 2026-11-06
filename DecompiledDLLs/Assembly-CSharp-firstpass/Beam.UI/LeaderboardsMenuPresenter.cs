using System;
using System.Collections.Generic;
using Beam.AccountServices;
using Beam.Language;
using Beam.Utilities;
using Rewired;
using Steamworks;
using UnityEngine;

namespace Beam.UI;

public class LeaderboardsMenuPresenter : MonoBehaviour
{
	[SerializeField]
	private LeaderboardsMenuViewAdapterBase _view;

	private ILeaderboardElement _firstLeaderboardElement;

	private Player _currentPlayerInput;

	public event Action ShowView;

	public event Action HideView;

	private void OnDestroy()
	{
		ShowView = null;
		HideView = null;
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
		if (!_view.Visible)
		{
			_currentPlayerInput = input;
			_view.ShowView += View_ShowView;
			_view.HideView += View_HideView;
			_view.BackButton.Click += BackButton_Click;
			_view.Show();
			if (!GLeaderboardHandler.Get().Connected)
			{
				DialogueBox.Show(_currentPlayerInput, "LEADERBOARDS_MENU_DB_SORRY_TITLE", "Not connected", DialogueButton.Ok, DialogueIcon.Warning).Accept += Hide;
				return;
			}
			GLeaderboardHandler.Get().LeaderboardDownloaded += LeaderboardHandler_LeaderboardDownloaded;
			GLeaderboardHandler.Get().LeaderboardEntriesDownloaded += LeaderboardHandler_LeaderboardEntriesDownloaded;
			GLeaderboardHandler.Get().LeaderboardEntryDownloadedForUser += LeaderboardHandler_LeaderboardEntryDownloadedForUser;
			GLeaderboardHandler.Get().DownloadLeaderboards();
			SubscribeToInputEvents();
		}
	}

	public void Hide()
	{
		if (_view.Visible)
		{
			UnsubscribeFromInputEvents();
			GLeaderboardHandler.Get().LeaderboardDownloaded -= LeaderboardHandler_LeaderboardDownloaded;
			GLeaderboardHandler.Get().LeaderboardEntriesDownloaded -= LeaderboardHandler_LeaderboardEntriesDownloaded;
			GLeaderboardHandler.Get().LeaderboardEntryDownloadedForUser -= LeaderboardHandler_LeaderboardEntryDownloadedForUser;
			_view.Hide();
			_view.ShowView -= View_ShowView;
			_view.HideView -= View_HideView;
			_view.BackButton.Click -= BackButton_Click;
			_firstLeaderboardElement = null;
			_currentPlayerInput = null;
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

	private void LeaderboardButton_Click(object sender)
	{
		if (SteamUser.BLoggedOn())
		{
			if (sender is ILeaderboardElement leaderboardElement)
			{
				_view.SetLeaderboard(leaderboardElement.LeaderboardData);
				_view.SetUserLeaderboardEntry(new LeaderboardEntryData
				{
					Username = GAccountService.Get().LocalUser.UserInfo.DisplayName,
					Rank = -1,
					Value = -1
				});
				GLeaderboardHandler.Get().DownloadLeaderboardEntries(leaderboardElement.LeaderboardData.Name);
				GLeaderboardHandler.Get().DownloadLeaderboardEntryForUser(leaderboardElement.LeaderboardData.Name);
			}
		}
		else
		{
			string title = "TEXT_ERROR";
			string message = Localization.GetLanguageHandler().Localize("LEADERBOARDS_MENU_DB_NO_INTERNET_DESC");
			DialogueBox.Show(ReInput.players.GetPlayer(0), title, message, DialogueButton.Ok, DialogueIcon.Information).Accept += () =>
			{
				Hide();
			};
		}
	}

	private void Rewired_BackButtonPressed(InputActionEventData data)
	{
		BackButton_Click(null);
	}

	private void BackButton_Click(object sender)
	{
		Hide();
	}

	private void LeaderboardHandler_LeaderboardDownloaded(LeaderboardData leaderboardData)
	{
		_view.CreateLeaderboardButton(leaderboardData).Click += LeaderboardButton_Click;
		if (BeamTeam.IsNullOrDestroyed(_firstLeaderboardElement))
		{
			_firstLeaderboardElement = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_view.LeaderboardButtons);
			_firstLeaderboardElement?.PerformClick();
			_view.SelectFirstButton();
		}
	}

	private void LeaderboardHandler_LeaderboardEntriesDownloaded(IEnumerable<LeaderboardEntryData> leaderboardEntries)
	{
		if (leaderboardEntries != null)
		{
			_view.CreateLeaderboardEntries(leaderboardEntries);
		}
	}

	private void LeaderboardHandler_LeaderboardEntryDownloadedForUser(LeaderboardEntryData leaderboardEntry)
	{
		_view.SetUserLeaderboardEntry(leaderboardEntry);
	}

	private bool CheckConnection()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Application.internetReachability == 0)
		{
			return false;
		}
		return true;
	}
}
