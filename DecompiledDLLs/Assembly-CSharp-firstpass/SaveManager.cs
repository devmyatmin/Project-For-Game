using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beam;
using Beam.AccountServices;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class SaveManager
{
	private class ReplicateSavingStateMsg : MultiplayerMessage
	{
		[Replicate]
		public bool Saving;

		public override void OnPeer()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			MPExtensions.IsMultiplayer(Game.Mode);
			UpdateSavingSpinner(Saving);
		}
	}

	private class ReplicateSavedNotificationMsg : MultiplayerMessage
	{
		public override void OnPeer()
		{
			LocalizedNotification.Post(PlayerRegistry.LocalPlayer, NotificationPriority.Immediate, 4f, "BED_NTF_SAVED_DESC");
		}
	}

	public delegate void SaveCallback(int result);

	private class ReplicateSaveErrorDialogueMsg : MultiplayerMessage
	{
		[Replicate]
		public IPlayer InvokingPlayer;

		[Replicate]
		public int ResultCode;

		public override void OnPeer()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			MPExtensions.IsClient(Game.Mode);
			ReplicateSaveErrorDialogue(InvokingPlayer, ResultCode);
		}
	}

	private static bool _isSaving;

	public static bool IsSaving => _isSaving;

	public static event Action PreSave;

	public static event Action<JObject> BeforeWrite;

	public static event Action<GameMode> DeletedSaveFile;

	public static JObject GetPersistentData(JObject root)
	{
		if (root == null || root.IsNull())
		{
			Debug.LogError((object)"SaveManager::LoadSaveFile:: Root was null or empty.");
			return null;
		}
		JObject field = root.GetField("Persistent");
		if (field != null && !field.IsNull())
		{
			return field;
		}
		Debug.LogError((object)"SaveManager::LoadSaveFile:: [Persistent] node is null, empty or missing.");
		return null;
	}

	public static string GetSaveFolderPath()
	{
		return PathTools.Combine(new string[2]
		{
			FilePath.SAVE_FOLDER,
			FilePath.GetSlotFolderName(Options.GeneralSettings.LastSaveSlotUsed)
		});
	}

	public static string GetSaveFilePath(GameMode mode)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return PathTools.Combine(new string[2]
		{
			GetSaveFolderPath(),
			"data" + FilePath.GetSaveFileExtension(mode)
		});
	}

	public static bool TryGetSaveFilePath(GameMode mode, out string saveFilePath)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		saveFilePath = string.Empty;
		if (!GAccountService.Get().Initialized)
		{
			Debug.LogError((object)"SaveManager::CheckSaveFileExists:: DRM not initialized.");
			return false;
		}
		string saveFolderPath = GetSaveFolderPath();
		if (string.IsNullOrEmpty(saveFolderPath) || !Directory.Exists(saveFolderPath))
		{
			return false;
		}
		string saveFilePath2 = GetSaveFilePath(mode);
		if (string.IsNullOrEmpty(saveFilePath2))
		{
			return false;
		}
		saveFilePath = saveFilePath2;
		return File.Exists(saveFilePath);
	}

	public static bool CheckSaveFileVersion(GameMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Version saveFileVersion = GetSaveFileVersion(mode);
			if (saveFileVersion == null)
			{
				return false;
			}
			if (!ValidateVersionNumber(saveFileVersion))
			{
				Debug.LogError((object)$"SaveManager::CheckSaveFileVersion:: {mode} save file is an older version.");
				return false;
			}
			Debug.Log((object)($"SaveManager::CheckSaveFileVersion:: Valid {mode} save file version [" + saveFileVersion?.ToString() + "]"));
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("SaveManager::CheckSaveFileVersion:: There was an error parsing save file version:\n" + ex.Message));
			return false;
		}
	}

	public static VersionStatus CompareSaveFileVersion(GameMode mode, Version otherVersion)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (otherVersion == null)
		{
			return VersionStatus.Newer;
		}
		Version saveFileVersion = GetSaveFileVersion(mode);
		if (saveFileVersion == null)
		{
			return VersionStatus.Older;
		}
		return (VersionStatus)saveFileVersion.CompareTo(otherVersion);
	}

	public static Version GetSaveFileVersion(GameMode mode)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!GAccountService.Get().Initialized)
		{
			Debug.LogError((object)"SaveManager::GetSaveFileVersion:: Steam not initialized.");
			return null;
		}
		if (!TryGetSaveFilePath(mode, out var saveFilePath))
		{
			Debug.Log((object)$"SaveManager::GetSaveFileVersion:: {mode} save file does not exist.");
			return null;
		}
		try
		{
			using FileStream stream = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read);
			using StreamReader streamReader = new StreamReader(stream);
			streamReader.Read();
			bool flag = streamReader.Peek() == 13;
			if (flag)
			{
				streamReader.Read();
			}
			bool flag2 = streamReader.Peek() == 10;
			if (flag2)
			{
				streamReader.Read();
			}
			string version;
			if (flag | flag2)
			{
				version = streamReader.ReadLine().Trim().Substring(12, 7);
			}
			else
			{
				char[] array = new char[20];
				streamReader.Read(array, 0, array.Length);
				version = new string(array).Substring(11, 7);
			}
			return new Version(version);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("SaveManager::GetSaveFileVersion:: Error loading version data from save file:\n\n" + ex.Message));
			return null;
		}
	}

	public static bool ValidateVersionNumber(Version version)
	{
		VersionStatus versionStatus = (VersionStatus)version.CompareTo(BuildInformation.MinimumVersion);
		if (versionStatus != VersionStatus.Equal)
		{
			return versionStatus == VersionStatus.Newer;
		}
		return true;
	}

	public static bool ValidateSaveFile(GameMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (CheckSaveFileVersion(mode))
		{
			return ValidateSaveFileFast(mode);
		}
		return false;
	}

	private static bool ValidateSaveFileFast(GameMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected Obj, but got Unknown
		if (!TryGetSaveFilePath(mode, out var saveFilePath))
		{
			Debug.LogError((object)"SaveManager:: Error loading save file. File does not exist.");
			return false;
		}
		try
		{
			using FileStream stream = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read);
			using StreamReader streamReader = new StreamReader(stream);
			JsonTextReader val = new JsonTextReader((TextReader)streamReader);
			try
			{
				JToken val2 = JToken.ReadFrom((JsonReader)(object)val);
				if (val2 == null || !val2.HasValues)
				{
					Debug.LogError((object)"SaveManager:: Error loading save file. Data was null or empty.");
					return false;
				}
				JToken val3 = val2.SelectToken("Persistent.StrandedWorld");
				if (BeamTeam.IsNullOrDestroyed(val3) || !val3.HasValues)
				{
					Debug.LogError((object)"SaveManager:: Error loading save file. Stranded world data was null or empty.");
					return false;
				}
				return true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("SaveManager:: Error loading save file:\n\n" + ex));
			return false;
		}
	}

	public static JObject LoadSaveFile(GameMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetSaveFilePath(mode, out var saveFilePath))
		{
			Debug.LogError((object)$"SaveManager::LoadSaveFile:: Error loading {mode} save file. File does not exist.");
			return null;
		}
		JObject jObject = null;
		using (FileStream stream = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read))
		{
			using StreamReader streamReader = new StreamReader(stream);
			jObject = new JObject(streamReader.ReadToEnd());
		}
		if (jObject.IsNull())
		{
			Debug.LogError((object)"SaveManager::LoadSaveFile:: Error loading save file. Data was null or empty.");
			return null;
		}
		JObject jObject2 = jObject.Get("Persistent/StrandedWorld");
		if (BeamTeam.IsNullOrDestroyed(jObject2) || jObject2.IsNull())
		{
			Debug.LogError((object)"SaveManager::LoadSaveFile:: Error loading save file. Stranded world data was null or empty.");
			return null;
		}
		JObject field = jObject.GetField("CustomSettings");
		if (field != null)
		{
			GameSettings gameSettings = new GameSettings();
			if (!SavingUtilities.Load(gameSettings, field))
			{
				Debug.LogError((object)"SaveManager::Load:: Could not load [CustomSettings] data.");
			}
			Options.CustomSettings = gameSettings;
		}
		else
		{
			Debug.LogError((object)"SaveManager::Load:: Old Save Detected. No [CustomSettings] data was found.");
		}
		return jObject;
	}

	public static JToken LoadSaveFileFast(string path)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected Obj, but got Unknown
		try
		{
			using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
			using StreamReader streamReader = new StreamReader(stream);
			JsonTextReader val = new JsonTextReader((TextReader)streamReader);
			try
			{
				JToken val2 = JToken.ReadFrom((JsonReader)(object)val);
				if (val2 == null)
				{
					throw new Exception("Root data null.");
				}
				if (!val2.HasValues)
				{
					throw new Exception("Root data empty.");
				}
				return val2;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("SaveManager:: Error loading save file:\n\n" + ex));
			return null;
		}
	}

	public static void BackupSaveFiles(string suffix = "")
	{
		if (string.IsNullOrEmpty(suffix))
		{
			suffix = "_BACKUP";
		}
		if (TryGetSaveFilePath((GameMode)0, out var saveFilePath))
		{
			string destFileName = saveFilePath + suffix;
			File.Copy(saveFilePath, destFileName, overwrite: true);
		}
		if (TryGetSaveFilePath((GameMode)1, out saveFilePath))
		{
			string destFileName2 = saveFilePath + suffix;
			File.Copy(saveFilePath, destFileName2, overwrite: true);
		}
		if (TryGetSaveFilePath((GameMode)4, out saveFilePath))
		{
			string destFileName3 = saveFilePath + suffix;
			File.Copy(saveFilePath, destFileName3, overwrite: true);
		}
	}

	public static void DeleteSaveFile(GameMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetSaveFilePath(mode, out var saveFilePath) && File.Exists(saveFilePath))
		{
			File.Delete(saveFilePath);
			DeletedSaveFile?.Invoke(mode);
		}
	}

	public static void DeleteSaveFiles()
	{
		DeleteSaveFile((GameMode)0);
		DeleteSaveFile((GameMode)1);
		DeleteSaveFile((GameMode)4);
	}

	public static void SaveGame(IPlayer invokingPlayer)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (_isSaving)
		{
			return;
		}
		_isSaving = true;
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: true);
		ReplicateSavingState(saving: true);
		Save(invokingPlayer, (int result) =>
		{
			MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
			ReplicateSavingState(saving: false);
			if (result == 0)
			{
				LocalizedNotification.Post(PlayerRegistry.LocalPlayer, NotificationPriority.Immediate, 4f, "BED_NTF_SAVED_DESC");
				new ReplicateSavedNotificationMsg().Post();
			}
			else
			{
				ReplicateSaveErrorDialogue(invokingPlayer, result);
			}
			_isSaving = false;
		});
	}

	private static void ReplicateSavingState(bool saving)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		MPExtensions.IsMultiplayer(Game.Mode);
		UpdateSavingSpinner(saving);
		ReplicateSavingStateMsg replicateSavingStateMsg = new ReplicateSavingStateMsg();
		replicateSavingStateMsg.Saving = saving;
		replicateSavingStateMsg.Post();
	}

	private static void UpdateSavingSpinner(bool active)
	{
		if (active)
		{
			SpinnerWidget uIComponent = GameUI.GetUIComponent<SpinnerWidget>();
			if ((Object)(object)uIComponent != (Object)null)
			{
				uIComponent.Show();
			}
		}
		else
		{
			SpinnerWidget uIComponent2 = GameUI.GetUIComponent<SpinnerWidget>();
			if ((Object)(object)uIComponent2 != (Object)null)
			{
				uIComponent2.Hide();
			}
		}
	}

	private static CoroutineHandle Save(IPlayer player, SaveCallback callback)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(Save_Sequence(player, callback));
	}

	private static IEnumerator<float> Save_Sequence(IPlayer player, SaveCallback callback)
	{
		bool succeeded = true;
		JObject save = new JObject();
		JObject persistent = new JObject();
		yield return Timing.WaitUntilDone(BeamTeam.RunCoroutine(Timing.Instance, GenerateSave_Sequence(save, persistent), (Exception ex) =>
		{
			if (ex != null)
			{
				succeeded = false;
				Debug.LogException(ex);
				callback(57005);
			}
		}));
		if (!succeeded)
		{
			yield break;
		}
		yield return Timing.WaitUntilDone(BeamTeam.RunCoroutine(Timing.Instance, AfterSave_Sequence(player, save, callback), (Exception ex) =>
		{
			if (ex != null)
			{
				succeeded = false;
				Debug.LogException(ex);
				callback(48879);
			}
		}));
	}

	private static IEnumerator<float> AfterSave_Sequence(IPlayer player, JObject save, SaveCallback callback)
	{
		try
		{
			BeforeWrite?.Invoke(save);
		}
		catch (Exception)
		{
			Debug.Log((object)"SaveManager::Save:: Errors were thrown in BeforeWrite event.");
			throw;
		}
		using (FileStream stream = new FileStream(GetSaveFilePath(Game.Mode), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			string value = save.ToString();
			streamWriter.Write(value);
		}
		Debug.LogError((object)"Yield 4");
		yield return 0f;
		int result = 0;
		callback(result);
		Debug.LogError((object)"Yield 5");
		yield return 0f;
		GC.Collect();
		Debug.LogError((object)"Garbage collect finished");
	}

	public static IEnumerator<float> GenerateSave_Sequence(JObject save, JObject persistent)
	{
		try
		{
			PreSave?.Invoke();
		}
		catch (Exception)
		{
			Debug.Log((object)"SaveManager::Save:: Errors were thrown in PreSave event.");
			throw;
		}
		PlayerRegistry.Save();
		try
		{
			PreSave?.Invoke();
		}
		catch (Exception ex2)
		{
			Debug.Log((object)("SaveManager::Save:: Errors were thrown in PreSave event.\n\n" + ex2));
		}
		JObject data = PlayerRegistry.Save();
		persistent.AddField("PlayerRegistry", data);
		(from behaviour in Object.FindObjectsOfType<MonoBehaviour>()
			where behaviour is IPersistentSaveable
			orderby !(behaviour is StrandedWorld)
			select behaviour).Cast<IPersistentSaveable>().ToList().ForEach((IPersistentSaveable saveable) =>
		{
			JObject jObject2 = SavingUtilities.Save(saveable);
			if (jObject2 != null)
			{
				persistent.AddField(saveable.GetType().Name, jObject2);
			}
			else
			{
				Debug.LogError((object)("SaveManager::Save:: Error saving a persistent object.\n\nName: " + saveable.GetType().Name));
			}
		});
		save.AddField("Version", ExtensionMethods.ToPaddedString(BuildInformation.BuildVersion));
		save.AddField("Persistent", persistent);
		LevelLoader.Instance?.PatchServerClientCopy(save);
		JObject jObject = SavingUtilities.Save(Options.CustomSettings);
		if (jObject != null)
		{
			save.AddField("CustomSettings", jObject);
		}
		yield return 0f;
	}

	public static bool SaveToFile(string path, JObject rootData)
	{
		if (string.IsNullOrEmpty(path))
		{
			Debug.LogError((object)"Path is null or empty!");
			return false;
		}
		if (rootData == null || rootData.IsNull())
		{
			Debug.LogError((object)"Root data is null or empty!");
			return false;
		}
		try
		{
			using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(rootData.ToString());
			return true;
		}
		catch (Exception ex)
		{
			Debug.Log((object)("SaveManager:: Error saving progress:\n\n" + ex));
		}
		return false;
	}

	public static void Save(GameMode mode, JObject save)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		SaveToFile(GetSaveFilePath(mode), save);
	}

	public static bool ChangeCurrentSlot(int slotID)
	{
		Debug.Log((object)("SaveManager:: Selecting slot " + slotID));
		int lastSaveSlotUsed = Options.GeneralSettings.LastSaveSlotUsed;
		Options.GeneralSettings.LastSaveSlotUsed = slotID;
		string slotFolderName = FilePath.GetSlotFolderName(slotID);
		FilePath.WORLD_FOLDER = PathTools.Combine(new string[3]
		{
			FilePath.SAVE_FOLDER,
			slotFolderName,
			"World"
		});
		FilePath.HEIGHTMAPS_FOLDER = PathTools.Combine(new string[3]
		{
			FilePath.SAVE_FOLDER,
			slotFolderName,
			"Heightmaps"
		});
		return lastSaveSlotUsed != slotID;
	}

	private static void ReplicateSaveErrorDialogue(IPlayer invokingPlayer, int resultCode)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (invokingPlayer.Id == PlayerRegistry.LocalPlayer.Id)
		{
			UpdateSavingSpinner(active: false);
			int pId = invokingPlayer.Id;
			int prevMap = RewiredUtilities.GetCurrentMapId(invokingPlayer.Input);
			RewiredUtilities.DisableMaps(RewiredUtilities.GetOtherPlayers(invokingPlayer.Input));
			RewiredUtilities.SetAndCacheMapsEnabled(invokingPlayer.Input, state: false, prevMap);
			RewiredUtilities.SetAndCacheMapsEnabled(invokingPlayer.Input, state: true, 16);
			RewiredUtilities.SetCursorEnabled(invokingPlayer.Input, enabled: true);
			string prettyResultMsg = ExtensionMethods.GetPrettyResultMsg(resultCode, "SAVE_DB_ERROR_SAVING_DESC");
			DialogueBox.Show(PlayerRegistry.LocalPlayer.Input, "SAVE_DB_ERROR_TITLE", prettyResultMsg, DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pId);
				RewiredUtilities.RestoreMapsEnabled(RewiredUtilities.GetOtherPlayers(playerFromId.Input));
				RewiredUtilities.SetAndCacheMapsEnabled(playerFromId.Input, state: false, 16);
				RewiredUtilities.SetAndCacheMapsEnabled(playerFromId.Input, state: true, prevMap);
				RewiredUtilities.SetCursorEnabled(playerFromId.Input, enabled: false);
			};
		}
		else if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateSaveErrorDialogueMsg replicateSaveErrorDialogueMsg = new ReplicateSaveErrorDialogueMsg();
			replicateSaveErrorDialogueMsg.InvokingPlayer = invokingPlayer;
			replicateSaveErrorDialogueMsg.ResultCode = resultCode;
			replicateSaveErrorDialogueMsg.Post();
		}
	}

	public static CoroutineHandle SaveInternal(IPlayer player, SaveCallback callback)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Save(player, callback);
	}
}
