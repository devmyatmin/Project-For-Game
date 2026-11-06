using System;
using System.IO;
using Beam;
using Beam.Utilities;
using UnityEngine;

public static class FilePath
{
	public static readonly string LEGACY_USER_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

	public static readonly string LEGACY_SAVE_FOLDER = PathTools.Combine(LEGACY_USER_FOLDER, "Stranded Deep/Data/");

	public static readonly string USER_FOLDER = Application.persistentDataPath;

	public static readonly string SAVE_ROOT = "Data/";

	public static readonly string SAVE_FOLDER = PathTools.Combine(USER_FOLDER, SAVE_ROOT);

	public static readonly string ASSET_BUNDLES_FOLDER = PathTools.Combine(Application.dataPath, "Data/Bundles/");

	public const string SAVE_SLOT_FOLDER_PREFIX = "Slot";

	public const string SAVE_FILE_NAME = "data";

	public const string OPTIONS_FILE_NAME = "Options.json";

	public static readonly string OPTIONS_FILE = PathTools.Combine(SAVE_FOLDER, "Options.json");

	public const string PLAYER_PREFAB_PATH = "Prefabs/Player";

	public const string PREFAB_REFERENCE_FILE = "Prefabs/Prefabs";

	public const string PREFAB_LEGACY_MAPPING_FILE = "Prefabs/PrefabsLegacyMappping";

	public const string CRAFTING_COMBOS_FILE = "CraftingCombinations";

	public const string BUILDING_COMBOS_FILE = "BuildingCombinations";

	public const string BAD_WORDS_FILE = "bad-words";

	public const string SCREENSHOTS_EXTENSION = ".bytes";

	public const string DEFAULT_BACKGROUND_NAME = "default";

	public const string DYNAMIC_BACKGROUND_NAME = "dynamic";

	public const string DYNAMIC_BACKGROUND_NAME_COOP = "dynamic_COOP";

	public const string WORLD_FOLDER_NAME = "World";

	public const string HEIGHTMAPS_FOLDER_NAME = "Heightmaps";

	public const string LOCAL_MAPS_FOLDER_NAME = "Maps";

	public static string WORLD_FOLDER = PathTools.Combine(SAVE_FOLDER, GetSlotFolderName(0), "World");

	public static string HEIGHTMAPS_FOLDER = PathTools.Combine(SAVE_FOLDER, GetSlotFolderName(0), "Heightmaps");

	public static readonly string LOCAL_MAPS_FOLDER = PathTools.Combine(SAVE_FOLDER, "Maps");

	public static readonly string MODS_ROOT_FOLDER = PathTools.Combine(Application.dataPath, "Mods");

	public static readonly string MODS_MAPS_FOLDER = PathTools.Combine(MODS_ROOT_FOLDER, "Maps");

	public static readonly string MODS_LOCALIZATION_FOLDER = PathTools.Combine(MODS_ROOT_FOLDER, "Localization");

	public static readonly string DEFAULT_MAPS_FOLDER = PathTools.Combine(Application.dataPath, "Data/Default Maps/");

	public const string PREFS_FALLBACK_FOLDER_NAME = ".prefs/";

	public static readonly string PREFS_FALLBACK_FOLDER = PathTools.Combine(SAVE_FOLDER, ".prefs/");

	public const string PREFS_SESSION_KEY = "__aus_s";

	public const string PREFS_TOTAL_KEY = "__aus_t";

	private static readonly string[] _layout = new string[14]
	{
		SAVE_ROOT ?? "",
		SAVE_ROOT + "/Maps",
		SAVE_ROOT + "/Slot0",
		SAVE_ROOT + "/Slot0/World",
		SAVE_ROOT + "/Slot0/Heightmaps",
		SAVE_ROOT + "/Slot1",
		SAVE_ROOT + "/Slot1/World",
		SAVE_ROOT + "/Slot1/Heightmaps",
		SAVE_ROOT + "/Slot2",
		SAVE_ROOT + "/Slot2/World",
		SAVE_ROOT + "/Slot2/Heightmaps",
		SAVE_ROOT + "/Slot3",
		SAVE_ROOT + "/Slot3/World",
		SAVE_ROOT + "/Slot3/Heightmaps"
	};

	public static string GetSlotFolderName(int slotNumber)
	{
		return "Slot" + slotNumber;
	}

	public static string GetSaveFileSuffix(GameMode mode)
	{
		switch (mode)
		{
		case GameMode.Coop_2:
		case GameMode.Coop_3:
		case GameMode.Coop_4:
			return "_COOP";
		case GameMode.Coop_Server:
		case GameMode.Coop_Client:
			return "_MP_COOP";
		default:
			return string.Empty;
		}
	}

	public static string GetSaveFileExtension(GameMode mode)
	{
		return GetSaveFileSuffix(mode) + ".json";
	}

	public static string GetPrefsSessionKey(int slotNumber, GameMode mode)
	{
		return ((slotNumber > 0) ? string.Format("{0}_{1}", "__aus_s", slotNumber) : "__aus_s") + GetSaveFileSuffix(mode).ToLower();
	}

	public static string GetPrefsTotalKey(int slotNumber, GameMode mode)
	{
		return ((slotNumber > 0) ? string.Format("{0}_{1}", "__aus_t", slotNumber) : "__aus_t") + GetSaveFileSuffix(mode).ToLower();
	}

	public static bool CheckFilePermissions()
	{
		try
		{
			string text = Path.Combine(USER_FOLDER, "temp");
			if (Directory.Exists(text))
			{
				Directory.Delete(text);
			}
			Directory.CreateDirectory(text);
			FileInfo fileInfo = new FileInfo(text);
			fileInfo.Attributes |= FileAttributes.ReadOnly;
			fileInfo.Attributes &= ~FileAttributes.ReadOnly;
			Directory.Delete(text);
			string text2 = Path.Combine(USER_FOLDER, "temp.txt");
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			using (File.Create(text2))
			{
			}
			FileInfo fileInfo2 = new FileInfo(text2);
			fileInfo2.Attributes |= FileAttributes.ReadOnly;
			fileInfo2.Attributes &= ~FileAttributes.ReadOnly;
			File.Delete(text2);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			return false;
		}
	}

	public static bool CreateDirectories()
	{
		try
		{
			string[] layout = _layout;
			foreach (string text in layout)
			{
				string path = PathTools.Combine(USER_FOLDER, text);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			return false;
		}
	}
}
