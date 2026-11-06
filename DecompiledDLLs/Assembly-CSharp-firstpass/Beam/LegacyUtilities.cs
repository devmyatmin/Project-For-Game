using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Beam.AccountServices;
using Beam.Crafting;
using Beam.Serialization.Json;
using Beam.Terrain;
using Beam.Utilities;
using Newtonsoft.Json.Linq;
using Rewired;
using UnityEngine;

namespace Beam;

public static class LegacyUtilities
{
	private const string LEGACY_SUFFIX = "_LEGACY";

	public static void LoadPlayer(JObject persistentData)
	{
		if (persistentData.GetField("CraftingCombinations") != null)
		{
			LoadPre_64_04_Player(persistentData);
		}
		else
		{
			PlayerRegistry.Load(persistentData.GetField("PlayerRegistry"));
		}
	}

	private static void LoadPre_64_04_Player(JObject persistentData)
	{
		JObject data = persistentData["CraftingCombinations"]["Combinations"];
		JObject jObject = new JObject();
		jObject.AddField("Combinations", data);
		JObject field = persistentData.GetField("PlayerRegistry");
		foreach (JObject child in field.Children)
		{
			child.AddField("Crafter", jObject);
		}
		Debug.Log((object)"LegacyUtilities:: Loading pre-0.64.04 player data...");
		PlayerRegistry.Load(field);
	}

	public static bool LoadStrandedWorldPlayerPosition_PreCoop(JObject data, out Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		position = Vector3.zero;
		JObject field = data.GetField("PlayerPosition");
		if (field != null)
		{
			position = JSerializer.Deserialize<Vector3>(field);
			return true;
		}
		return false;
	}

	public static bool LoadStrandedWorldPlayerZoneId_PreCoop(JToken data, out string zoneId)
	{
		zoneId = string.Empty;
		JToken val = data[(object)"PlayerZoneId"];
		if (val != null)
		{
			zoneId = val.ToObject<string>();
			return true;
		}
		return false;
	}

	public static void LoadCompletedMissionGyroPieces(CraftingCombinations craftingCombinations)
	{
		CraftingCombination craftingCombination = craftingCombinations.Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == "EEL TROPHY");
		if (craftingCombination != null && craftingCombination.Unlocked)
		{
			CraftingCombination craftingCombination2 = craftingCombinations.Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == "GYROCOPTER FRAME");
			if (craftingCombination2 != null && !craftingCombination2.Unlocked)
			{
				craftingCombination2.Unlocked = true;
			}
			CraftingCombination craftingCombination3 = craftingCombinations.Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == "AIRCRAFT ENGINE PART");
			if (craftingCombination3 != null && !craftingCombination3.Unlocked)
			{
				craftingCombination3.Unlocked = true;
			}
		}
		CraftingCombination craftingCombination4 = craftingCombinations.Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == "SHARK TROPHY");
		if (craftingCombination4 != null && craftingCombination4.Unlocked)
		{
			CraftingCombination craftingCombination5 = craftingCombinations.Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == "GYROCOPTER MOTOR");
			if (craftingCombination5 != null && !craftingCombination5.Unlocked)
			{
				craftingCombination5.Unlocked = true;
			}
			CraftingCombination craftingCombination6 = craftingCombinations.Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == "AIRCRAFT RUDDER PART");
			if (craftingCombination6 != null && !craftingCombination6.Unlocked)
			{
				craftingCombination6.Unlocked = true;
			}
		}
		CraftingCombination craftingCombination7 = craftingCombinations.Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == "SQUID TROPHY");
		if (craftingCombination7 != null && craftingCombination7.Unlocked)
		{
			CraftingCombination craftingCombination8 = craftingCombinations.Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == "GYROCOPTER ROTORS");
			if (craftingCombination8 != null && !craftingCombination8.Unlocked)
			{
				craftingCombination8.Unlocked = true;
			}
			CraftingCombination craftingCombination9 = craftingCombinations.Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == "AIRCRAFT PROPELLER PART");
			if (craftingCombination9 != null && !craftingCombination9.Unlocked)
			{
				craftingCombination9.Unlocked = true;
			}
		}
	}

	private static bool IsLegacySaveFile()
	{
		if (SaveManager.CompareSaveFileVersion((GameMode)0, new Version(0, 71, 0)) == VersionStatus.Newer || SaveManager.CompareSaveFileVersion((GameMode)1, new Version(0, 71, 0)) == VersionStatus.Newer || SaveManager.CompareSaveFileVersion((GameMode)4, new Version(0, 75, 0)) == VersionStatus.Newer)
		{
			return false;
		}
		return true;
	}

	private static bool IsLegacyWorld()
	{
		if (!Directory.Exists(FilePath.WORLD_FOLDER))
		{
			return false;
		}
		IEnumerable<string> enumerable = from x in Directory.GetDirectories(FilePath.WORLD_FOLDER)
			select Path.GetFileName(x) into x
			where !string.IsNullOrEmpty(x)
			select x;
		if (enumerable == null || enumerable.Count() == 0)
		{
			return false;
		}
		return enumerable.Contains("0_0");
	}

	private static void UseLegacyBackups()
	{
		string saveFilePath = SaveManager.GetSaveFilePath((GameMode)0);
		if (File.Exists(saveFilePath + "_LEGACY"))
		{
			File.Move(saveFilePath, saveFilePath + "_LEGACY2");
			File.Move(saveFilePath + "_LEGACY", saveFilePath);
			Debug.Log((object)"Using legacy save backup for 1P upgrade...");
		}
		saveFilePath = SaveManager.GetSaveFilePath((GameMode)1);
		if (File.Exists(saveFilePath + "_LEGACY"))
		{
			File.Move(saveFilePath, saveFilePath + "_LEGACY2");
			File.Move(saveFilePath + "_LEGACY", saveFilePath);
			Debug.Log((object)"Using legacy save backup for 2P upgrade...");
		}
		saveFilePath = SaveManager.GetSaveFilePath((GameMode)4);
		if (File.Exists(saveFilePath + "_LEGACY"))
		{
			File.Move(saveFilePath, saveFilePath + "_LEGACY2");
			File.Move(saveFilePath + "_LEGACY", saveFilePath);
			Debug.Log((object)"Using legacy save backup for MP upgrade...");
		}
	}

	public static void UpgradeLegacyGame()
	{
		if (IsLegacySaveFile() && IsLegacyWorld())
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			UpgradeLegacyWorld();
			UpgradeLegacySaveFiles();
			GC.Collect();
			Debug.Log((object)$"Upgraded world in: {stopwatch.ElapsedMilliseconds}ms");
		}
	}

	private static void UpgradeLegacyWorld()
	{
		Debug.Log((object)"LegacyUtilities::UpgradeLegacyWorld()");
		World.UpgradeLegacyWorld(out var _);
	}

	private static void UpgradeLegacySaveFiles()
	{
		UseLegacyBackups();
		Debug.Log((object)"LegacyUtilities::UpgradeLegacySaveFiles()");
		if (SaveManager.CheckSaveFileVersion((GameMode)0) || SaveManager.CheckSaveFileVersion((GameMode)1) || SaveManager.CheckSaveFileVersion((GameMode)4))
		{
			SaveManager.BackupSaveFiles("_LEGACY");
			UpgradeSaveFile((GameMode)0);
			UpgradeSaveFile((GameMode)1);
			UpgradeSaveFile((GameMode)4);
		}
	}

	private static void UpgradeSaveFile(GameMode mode)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"LegacyUtilities::UpgradeSaveFile:: Upgrading {mode} save file...");
		if (!SaveManager.CheckSaveFileVersion(mode))
		{
			return;
		}
		JObject jObject = SaveManager.LoadSaveFile(mode);
		if (jObject == null || jObject.IsNull())
		{
			Debug.LogError((object)"Root data null. Could not upgrade..");
			return;
		}
		JObject persistentData = SaveManager.GetPersistentData(jObject);
		if (persistentData == null || persistentData.IsNull())
		{
			Debug.LogError((object)"Persistent data null. Could not upgrade..");
			return;
		}
		JObject field = persistentData.GetField("StrandedWorld");
		if (field == null || field.IsNull())
		{
			Debug.LogError((object)"StrandedWorld data null. Could not upgrade..");
			return;
		}
		JObject field2 = field.GetField("Zones");
		if (field2 == null || field2.IsNull())
		{
			Debug.LogError((object)"Zone data null. Could not upgrade..");
			return;
		}
		JObject jObject2 = new JObject();
		int i = 0;
		int num = 0;
		for (; i < 7; i++)
		{
			int num2 = 0;
			while (num2 < 7)
			{
				Map map = World.MapList[num];
				JObject jObject3 = new JObject();
				jObject3.AddField("Name", map.EditorData.Name);
				jObject3.AddField("Id", map.EditorData.Id);
				jObject3.AddField("Version", map.EditorData.VersionNumber);
				jObject2.Add(jObject3);
				num2++;
				num++;
			}
		}
		int num3 = 1;
		int num4 = 1;
		int j = 0;
		int num5 = 0;
		for (; j < 5; j++)
		{
			int num6 = 0;
			while (num6 < 5)
			{
				JObject value = field2.Children[num5];
				int num7 = j + num3;
				int num8 = num6 + num4;
				jObject2.Children[num8 * 7 + num7] = value;
				num6++;
				num5++;
			}
		}
		FixRigidbodyWorldPositions_Pre_72_00(jObject2);
		field["Zones"] = jObject2;
		JObject field3 = field.GetField("PlayersData");
		Vector2[] array = new Vector2[2];
		Vector3[] array2 = new Vector3[field3.Children.Count];
		for (int k = 0; k < field3.Children.Count; k++)
		{
			JObject jObject4 = field3.Children[k];
			string zoneId = jObject4.GetField("ZoneId").GetValue<string>();
			int num9 = Beam.Utilities.ExtensionMethods.FindIndex(World.MapList, (Map map2) => map2.EditorData.Id == zoneId);
			if (num9 != -1)
			{
				array[k] = World.GenerationZonePositons[num9];
				Vector3 val = ConvertLegacyWorldPositionToLocal(JSerializer.Deserialize<Vector3>(jObject4.GetField("Position")), World.GenerationZonePositons[num9]);
				array2[k] = val;
				JObject value2 = JSerializer.Serialize((object)array2[k]);
				jObject4["Position"] = value2;
			}
		}
		JObject field4 = persistentData.GetField("PlayerRegistry");
		for (int num10 = 0; num10 < field4.Children.Count; num10++)
		{
			JObject jObject5 = field4.Children[num10];
			JObject value3 = JSerializer.Serialize((object)array2[num10]);
			jObject5["Movement"]["Transform"]["localPosition"] = value3;
		}
		FixPiscusFollowerWorldPositions(persistentData, array);
		JObject jObject6 = new JObject();
		jObject6.AddField("Version", JSerializer.Serialize((object)global::ExtensionMethods.ToPaddedString(BuildInformation.BuildVersion)));
		jObject6.AddField("Persistent", persistentData);
		JObject field5 = jObject.GetField("CustomSettings");
		if (field5 != null)
		{
			jObject6.AddField("CustomSettings", field5);
		}
		JObject field6 = jObject.GetField("ScreenCapture");
		if (field6 != null)
		{
			jObject6.AddField("ScreenCapture", field6);
		}
		SaveManager.SaveToFile(SaveManager.GetSaveFilePath(mode), jObject6);
	}

	private static void FixPiscusFollowerWorldPositions(JObject persistentData, Vector2[] newPositions)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		JObject field = persistentData.GetField("PiscusFollower");
		if (field != null)
		{
			JObject jObject = new JObject();
			for (int i = 0; i < field.Children.Count; i++)
			{
				_ = field.Children[i];
				Vector3 zero = Vector3.zero;
				zero.x = newPositions[i].x;
				zero.z = newPositions[i].y;
				jObject.AddField("CenterGridPos_" + i, JSerializer.Serialize((object)zero));
			}
			persistentData["PiscusFollower"] = jObject;
		}
	}

	private static void FixRigidbodyWorldPositions_Pre_72_00(JObject zonesData)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		foreach (JObject child in zonesData.Children)
		{
			string id = child.GetField("Id").GetValue<string>();
			int num = Beam.Utilities.ExtensionMethods.FindIndex(World.MapList, (Map map) => map.EditorData.Id == id);
			if (num != -1)
			{
				JObject field = child.GetField("Objects");
				if (field != null && !field.IsNull())
				{
					RecursiveFixRigidbodyWorldPositions(field, Vector2.op_Implicit(World.GenerationZonePositons[num]));
				}
			}
		}
	}

	private static void RecursiveFixRigidbodyWorldPositions(JObject rootData, Vector3 newOrigin)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		foreach (JObject child in rootData.Children)
		{
			if (child.IsNull())
			{
				continue;
			}
			JObject field = child.GetField("Rigidbody");
			if (field != null)
			{
				FixRigidbodyWorldPosition(field, newOrigin);
			}
			JObject field2 = child.GetField("Ragdolls");
			if (field2 != null && !field2.IsNull())
			{
				foreach (JObject child2 in field2.Children)
				{
					FixRigidbodyWorldPosition(child2, newOrigin);
				}
			}
			RecursiveFixRigidbodyWorldPositions(child, newOrigin);
		}
	}

	private static void FixRigidbodyWorldPosition(JObject rigidbodyData, Vector3 newOrigin)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (rigidbodyData == null || rigidbodyData.IsNull())
		{
			Debug.LogError((object)"FixRigidbodyWorldPosition:: Rigidbody data is null or empty.");
			return;
		}
		Vector3 val = ConvertLegacyWorldPositionToLocal(rigidbodyData.GetField("position"), Vector2.op_Implicit(newOrigin));
		rigidbodyData["position"] = JSerializer.Serialize((object)val);
	}

	public static Vector3 ConvertLegacyWorldPositionToLocal(JObject worldPositionData, Vector2 localOrigin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ConvertLegacyWorldPositionToLocal(JSerializer.Deserialize<Vector3>(worldPositionData), localOrigin);
	}

	public static Vector3 ConvertLegacyWorldPositionToLocal(Vector3 worldPosition, Vector2 localOrigin)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = worldPosition;
		val.x -= 315f;
		val.z -= 315f;
		float num = BeamMath.Frac(val.x / 630f);
		float num2 = BeamMath.Frac(val.z / 630f);
		float num3 = num * 630f;
		float num4 = num2 * 630f;
		float num5 = (float)Math.Sign(val.x) * 315f;
		float num6 = (float)Math.Sign(val.z) * 315f;
		num3 -= num5;
		num4 -= num6;
		Vector3 zero = Vector3.zero;
		zero.x = localOrigin.x + num3;
		zero.z = localOrigin.y + num4;
		zero.y = val.y;
		return zero;
	}

	public static bool ResetInput_Pre_65_05()
	{
		Version fileVersion = Options.GetFileVersion();
		if (fileVersion == null)
		{
			return false;
		}
		VersionStatus versionStatus = (VersionStatus)fileVersion.CompareTo(new Version(0, 65, 5));
		if (versionStatus == VersionStatus.Equal || versionStatus == VersionStatus.Newer)
		{
			return false;
		}
		Options.Load();
		Player player = ReInput.players.GetPlayer(0);
		player.controllers.maps.LoadDefaultMaps((ControllerType)1);
		player.controllers.maps.LoadDefaultMaps((ControllerType)0);
		player.controllers.maps.LoadDefaultMaps((ControllerType)2);
		Options.InputSettings = new InputSettings();
		Options.Save();
		Debug.Log((object)"LegacyUtilities:: Reset input for pre-0.65.05 settings.");
		return true;
	}

	public static bool HasSaveFilePre_74_00(GameMode mode, out string filePath)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		filePath = string.Empty;
		string userId = GAccountService.Get().LocalUser.UserInfo.UserId;
		if (string.IsNullOrWhiteSpace(userId))
		{
			return false;
		}
		string text = PathTools.Combine(new string[2]
		{
			FilePath.LEGACY_SAVE_FOLDER,
			userId
		});
		if (!Directory.Exists(text))
		{
			return false;
		}
		string text2 = PathTools.Combine(new string[2]
		{
			text,
			userId + FilePath.GetSaveFileExtension(mode)
		});
		if (!File.Exists(text2))
		{
			return false;
		}
		filePath = text2;
		return true;
	}

	public static void MigrateSaveDataPre_74_00()
	{
		string filePath = string.Empty;
		string filePath2 = string.Empty;
		if ((0u | (HasSaveFilePre_74_00((GameMode)0, out filePath) ? 1u : 0u) | (HasSaveFilePre_74_00((GameMode)1, out filePath2) ? 1u : 0u)) != 0)
		{
			string text = PathTools.Combine(new string[2]
			{
				FilePath.SAVE_FOLDER,
				FilePath.GetSlotFolderName(0)
			});
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string destFileName = PathTools.Combine(new string[2]
			{
				text,
				"data" + FilePath.GetSaveFileExtension((GameMode)0)
			});
			string destFileName2 = PathTools.Combine(new string[2]
			{
				text,
				"data" + FilePath.GetSaveFileExtension((GameMode)1)
			});
			if (File.Exists(filePath))
			{
				File.Copy(filePath, destFileName, overwrite: true);
			}
			if (File.Exists(filePath2))
			{
				File.Copy(filePath2, destFileName2, overwrite: true);
			}
			BeamTeam.CopyDirectory(PathTools.Combine(new string[2]
			{
				FilePath.LEGACY_SAVE_FOLDER,
				"Editor/World"
			}), PathTools.Combine(new string[2] { text, "World" }));
			BeamTeam.CopyDirectory(PathTools.Combine(new string[2]
			{
				FilePath.LEGACY_SAVE_FOLDER,
				"Editor/Heightmaps"
			}), PathTools.Combine(new string[2] { text, "Heightmaps" }));
			BeamTeam.CopyDirectory(PathTools.Combine(new string[2]
			{
				FilePath.LEGACY_SAVE_FOLDER,
				"Editor/Maps"
			}), FilePath.LOCAL_MAPS_FOLDER);
			string text2 = PathTools.Combine(new string[2]
			{
				FilePath.LEGACY_SAVE_FOLDER,
				"Options.json"
			});
			if (File.Exists(text2))
			{
				File.Copy(text2, FilePath.OPTIONS_FILE, overwrite: true);
			}
			string text3 = PathTools.Combine(new string[2]
			{
				FilePath.LEGACY_SAVE_FOLDER,
				"Editor/Screenshots/dynamic.bytes"
			});
			if (File.Exists(text3))
			{
				File.Copy(text3, PathTools.Combine(new string[2] { text, "dynamic.bytes" }), overwrite: true);
			}
			string text4 = PathTools.Combine(new string[2]
			{
				FilePath.LEGACY_SAVE_FOLDER,
				"Editor/Screenshots/dynamic_COOP.bytes"
			});
			if (File.Exists(text4))
			{
				File.Copy(text4, PathTools.Combine(new string[2] { text, "dynamic_COOP.bytes" }), overwrite: true);
			}
		}
	}
}
