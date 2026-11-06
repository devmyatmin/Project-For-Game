using Beam.AccountServices;
using Beam.Events;
using Beam.Obfuscated.Serialization;
using Beam.UI;
using Beam.UI.MapEditor;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam.Obfuscated.Users;

public class RemoteStatsEventsListener : MonoBehaviour
{
	private static RemoteStatsEventsListener _Instance;

	private void Awake()
	{
		if ((Object)(object)_Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		_Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Game.Quitting += Game_Quitting;
		SaveManager.PreSave += SaveManager_PreSave;
		SaveManager.DeletedSaveFile += SaveManager_DeletedSaveFile;
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
		EventManager.AddListener<CompletedEndGameEvent>(EventManager_CompletedEndGameEvent);
	}

	private void OnDestroy()
	{
		if (!((Object)(object)_Instance != (Object)(object)this))
		{
			SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
			SaveManager.PreSave -= SaveManager_PreSave;
			SaveManager.DeletedSaveFile -= SaveManager_DeletedSaveFile;
			Game.Quitting -= Game_Quitting;
			EventManager.RemoveListener<CompletedEndGameEvent>(EventManager_CompletedEndGameEvent);
			_Instance = null;
		}
	}

	private static void SaveStats()
	{
		SaveSessionStats();
		SaveGlobalStats();
	}

	private static void SaveAndPushStats()
	{
		SaveStats();
		PushGlobalStatsToRemotes();
	}

	private void SaveManager_PreSave()
	{
		SaveAndPushStats();
	}

	private void Game_Quitting()
	{
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 1)
		{
			Object.FindObjectOfType<CartographerMenuPresenter>().WorldApplied += Cartographer_WorldApplied;
		}
		else if (scene.buildIndex == 4)
		{
			Object.FindObjectOfType<MapEditorSaveMenuPresenter>().Saved += MapEditorSaveMenuPresenter_Saved;
		}
	}

	private void Cartographer_WorldApplied(CartographerMenuPresenter sender, CartographerEventArgs args)
	{
		int mapsInserted = args.MapsInserted;
		GAccountService.Get().LocalUser.UserStats.SessionStats.IslandsInserted += mapsInserted;
		SaveStats();
	}

	private void MapEditorSaveMenuPresenter_Saved()
	{
		GAccountService.Get().LocalUser.UserStats.SessionStats.IslandsCreated++;
		SaveStats();
	}

	private void EventManager_CompletedEndGameEvent(CompletedEndGameEvent e)
	{
		SaveAndPushStats();
	}

	private void SaveManager_DeletedSaveFile(GameMode mode)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		SavingUtilities.DeleteFromPrefs(FilePath.GetPrefsSessionKey(Options.GeneralSettings.LastSaveSlotUsed, mode));
		SavingUtilities.DeleteFromPrefs(FilePath.GetPrefsTotalKey(Options.GeneralSettings.LastSaveSlotUsed, mode));
		Debug.Log((object)$"Deleted slot {Options.GeneralSettings.LastSaveSlotUsed} [{mode}] stats.");
	}

	public static void SaveSessionStats()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		AccountUserStats userStats = GAccountService.Get().LocalUser.UserStats;
		if ((int)Game.State == 2 || (int)Game.State == 3)
		{
			userStats.SessionStats.TimePlayed = Mathf.FloorToInt(Singleton<GameTime>.Instance.SessionDuration);
		}
		SavingUtilities.SaveToPrefs(FilePath.GetPrefsSessionKey(Options.GeneralSettings.LastSaveSlotUsed, Game.Mode), userStats.SessionStats);
		Debug.Log((object)("Saved session stats:\n" + userStats.SessionStats));
	}

	public static void SaveGlobalStats()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		AccountUserStats userStats = GAccountService.Get().LocalUser.UserStats;
		userStats.Rebase();
		SavingUtilities.SaveToPrefs(FilePath.GetPrefsTotalKey(Options.GeneralSettings.LastSaveSlotUsed, Game.Mode), userStats.TotalStats);
		Debug.Log((object)("Saved global stats:\n" + userStats.TotalStats));
	}

	public static void PushGlobalStatsToRemotes(bool immediate = false)
	{
		AccountUserStats userStats = GAccountService.Get().LocalUser.UserStats;
		userStats.Rebase();
		GAchievementsHandler.Get().SubmitStats(userStats, immediate);
		GLeaderboardHandler.Get().SubmitScores(userStats);
		Debug.Log((object)"Pushed global stats to remotes.");
	}
}
