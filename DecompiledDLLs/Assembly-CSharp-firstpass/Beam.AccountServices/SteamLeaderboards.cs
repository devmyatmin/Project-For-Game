using System;
using System.Collections.Generic;
using Beam.Obfuscated.Users;
using Beam.Utilities;
using Steamworks;
using UnityEngine;

namespace Beam.AccountServices;

public class SteamLeaderboards : MonoBehaviour, ILeaderboardHandler
{
	private class SteamLeaderboard
	{
		public string Name;

		public ulong Id;

		public bool Valid;
	}

	private static readonly string DEBUG_HEADER = "<color=blue>SteamLeaderboards::</color>";

	private List<SteamLeaderboard> _leaderboards = new List<SteamLeaderboard>
	{
		new SteamLeaderboard
		{
			Name = "MOST_TIME_PLAYED"
		},
		new SteamLeaderboard
		{
			Name = "MOST_DAYS_SURVIVED"
		},
		new SteamLeaderboard
		{
			Name = "QUICKEST_ESCAPE"
		},
		new SteamLeaderboard
		{
			Name = "MOST_ISLANDS_HOPPED"
		},
		new SteamLeaderboard
		{
			Name = "MOST_ISLANDS_EXPLORED"
		},
		new SteamLeaderboard
		{
			Name = "MOST_FISH_FISHED"
		},
		new SteamLeaderboard
		{
			Name = "MOST_ANIMALS_KILLED"
		},
		new SteamLeaderboard
		{
			Name = "MOST_ANIMALS_TRAPPED"
		},
		new SteamLeaderboard
		{
			Name = "MOST_ITEMS_CRAFTED"
		},
		new SteamLeaderboard
		{
			Name = "MOST_ITEMS_LOOTED"
		},
		new SteamLeaderboard
		{
			Name = "MOST_SHIPS_SCAVENGED"
		},
		new SteamLeaderboard
		{
			Name = "MOST_STATUS_EFFECTS"
		},
		new SteamLeaderboard
		{
			Name = "MOST_ISLANDS_CREATED"
		}
	};

	private bool _leaderboardsRequested = true;

	private List<CallResult<LeaderboardFindResult_t>> _leaderboardFindResultCallResults = new List<CallResult<LeaderboardFindResult_t>>();

	private List<CallResult<LeaderboardScoreUploaded_t>> _leaderboardScoreUploadedCallResults = new List<CallResult<LeaderboardScoreUploaded_t>>();

	private CallResult<LeaderboardScoresDownloaded_t> _leaderboardScoresDownloadedCallResult;

	public static SteamLeaderboards Instance { get; private set; }

	bool ILeaderboardHandler.Connected => true;

	public event Action<LeaderboardData> LeaderboardDownloaded;

	public event Action<IEnumerable<LeaderboardEntryData>> LeaderboardEntriesDownloaded;

	public event Action<LeaderboardEntryData> LeaderboardEntryDownloadedForUser;

	private void AddCallResult<T>(IList<CallResult<T>> callResults, CallResult<T> callResult)
	{
		if (callResult != null)
		{
			callResults.Add(callResult);
		}
	}

	private void ReleaseCallResults<T>(IList<CallResult<T>> callbacks)
	{
		foreach (CallResult<T> callback in callbacks)
		{
			callback.Dispose();
		}
		callbacks.Clear();
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	private void OnDestroy()
	{
		LeaderboardDownloaded = null;
		LeaderboardEntriesDownloaded = null;
		LeaderboardEntryDownloadedForUser = null;
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		if (SteamManager.Instance.Initialized && _leaderboardsRequested)
		{
			Debug.Log((object)(DEBUG_HEADER + " Requesting leaderboards..."));
			FindLeaderboards();
			_leaderboardsRequested = false;
		}
	}

	private void FindLeaderboards()
	{
		ReleaseCallResults(_leaderboardFindResultCallResults);
		foreach (SteamLeaderboard leaderboard in _leaderboards)
		{
			AddCallResult(_leaderboardFindResultCallResults, FindLeaderboard(leaderboard.Name));
		}
	}

	private CallResult<LeaderboardFindResult_t> FindLeaderboard(string leaderboardName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		SteamAPICall_t val = SteamUserStats.FindLeaderboard(leaderboardName);
		CallResult<LeaderboardFindResult_t> val2 = CallResult<LeaderboardFindResult_t>.Create((APIDispatchDelegate<LeaderboardFindResult_t>)null);
		val2.Set(val, (APIDispatchDelegate<LeaderboardFindResult_t>)OnLeaderboardFindResult);
		return val2;
	}

	public void DownloadLeaderboards()
	{
		_leaderboardsRequested = true;
	}

	public void DownloadLeaderboardEntryForUser(string leaderboardName)
	{
	}

	public void DownloadLeaderboardEntries(string leaderboardName)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		SteamLeaderboard steamLeaderboard = _leaderboards.Find((SteamLeaderboard l) => l.Name == leaderboardName);
		if (steamLeaderboard == null)
		{
			Debug.LogError((object)(DEBUG_HEADER + "DownloadLeaderboardEntries: Leaderboard [" + leaderboardName + "] not found locally."));
			return;
		}
		SteamAPICall_t val = SteamUserStats.DownloadLeaderboardEntries(new SteamLeaderboard_t(steamLeaderboard.Id), (ELeaderboardDataRequest)2, 0, 0);
		if (_leaderboardScoresDownloadedCallResult != null)
		{
			_leaderboardScoresDownloadedCallResult.Dispose();
		}
		_leaderboardScoresDownloadedCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create((APIDispatchDelegate<LeaderboardScoresDownloaded_t>)null);
		_leaderboardScoresDownloadedCallResult.Set(val, (APIDispatchDelegate<LeaderboardScoresDownloaded_t>)OnLeaderboardScoresDownloaded);
	}

	void ILeaderboardHandler.SubmitScore(string leaderboardName, int score)
	{
		SubmitScore(leaderboardName, score);
	}

	private CallResult<LeaderboardScoreUploaded_t> SubmitScore(string leaderboardName, int score)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		SteamLeaderboard steamLeaderboard = _leaderboards.Find((SteamLeaderboard l) => l.Name == leaderboardName);
		if (steamLeaderboard == null)
		{
			Debug.LogError((object)(DEBUG_HEADER + "SubmitScore: Leaderboard [" + leaderboardName + "] not found locally."));
			return null;
		}
		if (!steamLeaderboard.Valid)
		{
			Debug.LogError((object)(DEBUG_HEADER + "SubmitScore: Leaderboard [" + leaderboardName + "] not valid. Does it exist on Steamworks?"));
			return null;
		}
		if (score == 0 || score < 0)
		{
			Debug.Log((object)(DEBUG_HEADER + "SubmitScore: Not submitting 0 score for leaderboard [" + steamLeaderboard.Name + "]."));
			return null;
		}
		Debug.Log((object)(DEBUG_HEADER + " Submitting score for leaderboard [" + steamLeaderboard.Name + "]..."));
		SteamAPICall_t val = SteamUserStats.UploadLeaderboardScore(new SteamLeaderboard_t(steamLeaderboard.Id), (ELeaderboardUploadScoreMethod)1, score, (int[])null, 0);
		CallResult<LeaderboardScoreUploaded_t> val2 = CallResult<LeaderboardScoreUploaded_t>.Create((APIDispatchDelegate<LeaderboardScoreUploaded_t>)null);
		val2.Set(val, (APIDispatchDelegate<LeaderboardScoreUploaded_t>)OnLeaderboardScoreUploaded);
		return val2;
	}

	public void SubmitScores(AccountUserStats stats)
	{
		ReleaseCallResults(_leaderboardScoreUploadedCallResults);
		Debug.Log((object)(DEBUG_HEADER + " Submitting scores for all leaderboards..."));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_TIME_PLAYED", stats.TotalStats.TimePlayed));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_DAYS_SURVIVED", stats.TotalStats.DaysSurvived));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_ISLANDS_HOPPED", stats.TotalStats.IslandsHopped));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_ISLANDS_EXPLORED", stats.TotalStats.IslandsExplored));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_FISH_FISHED", stats.TotalStats.FishFished));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_ANIMALS_KILLED", stats.TotalStats.AnimalsKilled));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_ANIMALS_TRAPPED", stats.TotalStats.AnimalsTrapped));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_ITEMS_CRAFTED", stats.TotalStats.ItemsCrafted));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_ITEMS_LOOTED", stats.TotalStats.ItemsLooted));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_SHIPS_SCAVENGED", stats.TotalStats.ShipsScavenged));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_STATUS_EFFECTS", stats.TotalStats.StatusEffects));
		AddCallResult(_leaderboardScoreUploadedCallResults, SubmitScore("MOST_ISLANDS_CREATED", stats.TotalStats.IslandsCreated));
	}

	private void OnLeaderboardFindResult(LeaderboardFindResult_t result, bool failed)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if (failed)
		{
			Debug.LogError((object)(DEBUG_HEADER + "OnLeaderboardFindResult: Request failed."));
			return;
		}
		if (result.m_bLeaderboardFound == 0)
		{
			Debug.LogError((object)(DEBUG_HEADER + "OnLeaderboardFindResult: Leaderboard not found on remote."));
			return;
		}
		string leaderboardName = SteamUserStats.GetLeaderboardName(result.m_hSteamLeaderboard);
		SteamLeaderboard steamLeaderboard = _leaderboards.Find((SteamLeaderboard l) => l.Name == leaderboardName);
		if (steamLeaderboard == null)
		{
			Debug.LogError((object)(DEBUG_HEADER + "OnLeaderboardFindResult: Leaderboard [" + leaderboardName + "] not found locally."));
		}
		steamLeaderboard.Name = leaderboardName;
		steamLeaderboard.Id = result.m_hSteamLeaderboard.m_SteamLeaderboard;
		if (!steamLeaderboard.Valid)
		{
			steamLeaderboard.Valid = true;
		}
		LeaderboardData obj = new LeaderboardData
		{
			Name = steamLeaderboard.Name,
			DisplayType = GetLeaderboardDisplayType(SteamUserStats.GetLeaderboardDisplayType(result.m_hSteamLeaderboard))
		};
		LeaderboardDownloaded?.Invoke(obj);
	}

	private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t result, bool failed)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (failed)
		{
			Debug.LogError((object)"SteamLeaderboards::OnLeaderboardScoreUploaded: Request failed.");
			return;
		}
		if (result.m_bSuccess == 0)
		{
			Debug.LogError((object)(DEBUG_HEADER + "OnLeaderboardScoreUploaded: " + result.m_hSteamLeaderboard.m_SteamLeaderboard));
			return;
		}
		string leaderboardName = SteamUserStats.GetLeaderboardName(result.m_hSteamLeaderboard);
		Debug.Log((object)(DEBUG_HEADER + $" Uploaded score [{result.m_nScore}] to leaderboard [{leaderboardName}]."));
	}

	private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t result, bool failed)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (failed)
		{
			Debug.LogError((object)(DEBUG_HEADER + "OnLeaderboardScoresDownloaded: Request failed."));
			return;
		}
		if (result.m_cEntryCount == 0)
		{
			Debug.Log((object)(DEBUG_HEADER + "OnLeaderboardScoresDownloaded: No scores yet for leaderboard [" + SteamUserStats.GetLeaderboardName(result.m_hSteamLeaderboard) + "]."));
			LeaderboardEntriesDownloaded?.Invoke(new List<LeaderboardEntryData>());
			return;
		}
		List<LeaderboardEntryData> list = new List<LeaderboardEntryData>(result.m_cEntryCount);
		LeaderboardEntry_t val = default;
		for (int i = 0; i < result.m_cEntryCount; i++)
		{
			if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, ref val, (int[])null, 0))
			{
				LeaderboardEntryData leaderboardEntryData = new LeaderboardEntryData
				{
					Username = SteamFriends.GetFriendPersonaName(val.m_steamIDUser),
					Rank = val.m_nGlobalRank,
					Value = val.m_nScore
				};
				list.Add(leaderboardEntryData);
				if (val.m_steamIDUser == SteamUser.GetSteamID())
				{
					Debug.Log((object)(DEBUG_HEADER + "OnLeaderboardScoresDownloaded: Retrieved score for user."));
					LeaderboardEntryDownloadedForUser?.Invoke(leaderboardEntryData);
				}
			}
		}
		LeaderboardEntriesDownloaded?.Invoke(list);
	}

	private void OnLeaderboardScoresDownloadedForUser(LeaderboardScoresDownloaded_t result, bool failed)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		LeaderboardEntry_t val = default;
		if (failed)
		{
			Debug.LogError((object)(DEBUG_HEADER + "OnLeaderboardScoresDownloadedForUser: Request failed."));
		}
		else if (result.m_cEntryCount == 0)
		{
			Debug.Log((object)(DEBUG_HEADER + "OnLeaderboardScoresDownloadedForUser: No scores yet for leaderboard [" + SteamUserStats.GetLeaderboardName(result.m_hSteamLeaderboard) + "]."));
		}
		else if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, 0, ref val, (int[])null, 0))
		{
			LeaderboardEntryData obj = new LeaderboardEntryData
			{
				Username = SteamFriends.GetFriendPersonaName(val.m_steamIDUser),
				Rank = val.m_nGlobalRank,
				Value = val.m_nScore
			};
			LeaderboardEntryDownloadedForUser?.Invoke(obj);
		}
	}

	private ELeaderboardDisplayType GetLeaderboardDisplayType(ELeaderboardDisplayType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)type != 1 && type - 2 <= 1)
		{
			return ELeaderboardDisplayType.Seconds;
		}
		return ELeaderboardDisplayType.Numeric;
	}
}
