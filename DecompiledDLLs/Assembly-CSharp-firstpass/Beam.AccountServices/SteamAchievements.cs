using System;
using System.Collections.Generic;
using System.Linq;
using Beam.Obfuscated.Users;
using Beam.Utilities;
using Steamworks;
using UnityEngine;

namespace Beam.AccountServices;

public class SteamAchievements : MonoBehaviour, IAchievementsHandler
{
	private static readonly string DEBUG_HEADER = "<color=blue>SteamAchievements::</color>";

	private CGameID _gameId;

	private bool _statsRequested;

	private bool _statsValid;

	private bool _storeStats;

	private Callback<UserStatsReceived_t> _userStatsReceived;

	private Callback<UserStatsStored_t> _userStatsStored;

	private Callback<UserAchievementStored_t> _userAchievementStored;

	private List<Achievement> _achievements;

	public static SteamAchievements Instance { get; private set; }

	public event Action<IEnumerable<Achievement>> AchievementsReceived;

	protected void OnAchievementsReceived(IEnumerable<Achievement> achievements)
	{
		AchievementsReceived?.Invoke(achievements);
	}

	private void Awake()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		Instance = this;
		if (SteamManager.Instance.Initialized)
		{
			_gameId = new CGameID(SteamUtils.GetAppID());
			_userStatsReceived = Callback<UserStatsReceived_t>.Create((DispatchDelegate<UserStatsReceived_t>)OnUserStatsReceived);
			_userStatsStored = Callback<UserStatsStored_t>.Create((DispatchDelegate<UserStatsStored_t>)OnUserStatsStored);
			_userAchievementStored = Callback<UserAchievementStored_t>.Create((DispatchDelegate<UserAchievementStored_t>)OnAchievementStored);
			_achievements = new List<Achievement>();
			string[] names = Enum.GetNames(typeof(EAchievement));
			foreach (string value in names)
			{
				EAchievement achievementID = (EAchievement)Enum.Parse(typeof(EAchievement), value, ignoreCase: true);
				_achievements.Add(new Achievement(achievementID));
			}
		}
	}

	private void OnDestroy()
	{
		AchievementsReceived = null;
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		if (SteamManager.Instance.Initialized)
		{
			if (!_statsRequested)
			{
				_statsRequested = RequestStats();
			}
			if (_statsValid && _storeStats)
			{
				StoreStats();
			}
		}
	}

	public bool RequestStats()
	{
		Debug.Log((object)(DEBUG_HEADER + " Requesting stats and achievements..."));
		return SteamUserStats.RequestCurrentStats();
	}

	public void SubmitStats(AccountUserStats stats, bool immediate = false)
	{
		if (_statsValid)
		{
			SteamUserStats.SetStat("TIME_PLAYED_SESSION", stats.SessionStats.TimePlayed);
			SteamUserStats.SetStat("DAYS_SURVIVED_SESSION", stats.SessionStats.DaysSurvived);
			SteamUserStats.SetStat("ISLANDS_HOPPED", stats.SessionStats.IslandsHopped);
			SteamUserStats.SetStat("ISLANDS_EXPLORED", stats.SessionStats.IslandsExplored);
			SteamUserStats.SetStat("FISH_FISHED", stats.SessionStats.FishFished);
			SteamUserStats.SetStat("ANIMALS_KILLED", stats.SessionStats.AnimalsKilled);
			SteamUserStats.SetStat("ANIMALS_TRAPPED", stats.SessionStats.AnimalsTrapped);
			SteamUserStats.SetStat("ITEMS_CRAFTED", stats.SessionStats.ItemsCrafted);
			SteamUserStats.SetStat("ITEMS_LOOTED", stats.SessionStats.ItemsLooted);
			SteamUserStats.SetStat("SHIPS_SCAVENGED", stats.SessionStats.ShipsScavenged);
			SteamUserStats.SetStat("STATUS_EFFECTS", stats.SessionStats.StatusEffects);
			SteamUserStats.SetStat("ISLANDS_INSERTED", stats.SessionStats.IslandsInserted);
			SteamUserStats.SetStat("ISLANDS_CREATED", stats.SessionStats.IslandsCreated);
			SteamUserStats.SetStat("TIME_PLAYED_TOTAL", stats.TotalStats.TimePlayed);
			SteamUserStats.SetStat("DAYS_SURVIVED_TOTAL", stats.TotalStats.DaysSurvived);
			_storeStats = true;
			if (immediate)
			{
				StoreStats();
			}
		}
	}

	public void StoreStats()
	{
		Debug.Log((object)(DEBUG_HEADER + " Storing stats..."));
		bool flag = SteamUserStats.StoreStats();
		_storeStats = !flag;
	}

	public void UnlockAchievement(EAchievement achId)
	{
		Achievement achievement = _achievements.Find((Achievement a) => a.Id == achId);
		if (achievement != null && !achievement.Achieved)
		{
			if (!SteamUserStats.SetAchievement(achievement.Id.ToString()))
			{
				Debug.LogError((object)(DEBUG_HEADER + $" SetAchievement failed for achievement '{achId}'."));
				return;
			}
			achievement.Achieved = true;
			_storeStats = true;
		}
	}

	public bool IsAchievementUnlocked(EAchievement achId)
	{
		return _achievements.FirstOrDefault((Achievement a) => a.Id == achId)?.Achieved ?? false;
	}

	private void OnUserStatsReceived(UserStatsReceived_t callback)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		if (!SteamManager.Instance.Initialized || callback.m_nGameID != (ulong)_gameId)
		{
			return;
		}
		if ((int)callback.m_eResult == 1)
		{
			Debug.Log((object)(DEBUG_HEADER + " Received stats and achievements."));
			_statsValid = true;
			foreach (Achievement achievement in _achievements)
			{
				if (!SteamUserStats.GetAchievement(achievement.Id.ToString(), ref achievement.Achieved))
				{
					Debug.LogWarning((object)(DEBUG_HEADER + $" GetAchievement failed for achievement '{achievement.Id}'."));
				}
			}
			OnAchievementsReceived(_achievements);
		}
		else
		{
			Debug.LogError((object)(DEBUG_HEADER + "  Request stats failed.\n" + ((object)callback.m_eResult/*cast due to constrained. prefix*/).ToString()));
		}
	}

	private void OnUserStatsStored(UserStatsStored_t callback)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (callback.m_nGameID == (ulong)_gameId)
		{
			if ((int)callback.m_eResult == 1)
			{
				Debug.Log((object)(DEBUG_HEADER + " Stats stored."));
			}
			else if ((int)callback.m_eResult == 8)
			{
				Debug.LogWarning((object)(DEBUG_HEADER + " Some stats failed to validate."));
				OnUserStatsReceived(new UserStatsReceived_t
				{
					m_eResult = (EResult)1,
					m_nGameID = (ulong)_gameId
				});
			}
			else
			{
				Debug.LogError((object)(DEBUG_HEADER + " Failed to store stats.\n" + ((object)callback.m_eResult/*cast due to constrained. prefix*/).ToString()));
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (pCallback.m_nGameID == (ulong)_gameId)
		{
			if (pCallback.m_nMaxProgress == 0)
			{
				Debug.Log((object)(DEBUG_HEADER + " <color=green>Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!</color>"));
				return;
			}
			Debug.Log((object)(DEBUG_HEADER + " <color=cyan>Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")</color>"));
		}
	}
}
