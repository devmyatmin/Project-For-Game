using System.Collections.Generic;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Serialization;

public static class Prefabs
{
	private static bool _initialized;

	private static List<PrefabReference> _prefabReferences;

	private static HashSet<string> _prefabPaths;

	private static Dictionary<uint, string> _prefabsMultiplayerNames;

	private static Dictionary<string, uint> _stringToUIntIdMap;

	private static Dictionary<uint, string> _uIntToStringIdMap;

	public static IList<PrefabReference> PrefabReferences => _prefabReferences;

	public static IEnumerable<string> PrefabPaths => _prefabPaths;

	public static bool TryGetMultiplayerPrefabName(uint prefabId, out string multiplayerPrefabName)
	{
		multiplayerPrefabName = null;
		if (prefabId == 0)
		{
			return false;
		}
		return _prefabsMultiplayerNames.TryGetValue(prefabId, out multiplayerPrefabName);
	}

	static Prefabs()
	{
		_prefabReferences = new List<PrefabReference>();
		_prefabPaths = new HashSet<string>();
		_prefabsMultiplayerNames = new Dictionary<uint, string>();
		_stringToUIntIdMap = new Dictionary<string, uint>();
		_uIntToStringIdMap = new Dictionary<uint, string>();
		Initialize();
	}

	public static void Initialize()
	{
		if (!_initialized)
		{
			InitializePrefabPaths();
			LoadPrefabs();
			_initialized = true;
		}
	}

	private static void InitializePrefabPaths()
	{
		_prefabPaths.Add("Prefabs/StrandedObjects/Animals/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Building/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Containers/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Crafting/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Easter Eggs/");
		_prefabPaths.Add("Prefabs/StrandedObjects/End Game/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Farming/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Food/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Furniture/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Medical/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Missions/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Other/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Props/");
		_prefabPaths.Add("Prefabs/StrandedObjects/SeaForts/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Shipwrecks/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Tools/");
		_prefabPaths.Add("Prefabs/StrandedObjects/Trees/");
	}

	public static SaveablePrefab CreatePrefabFromId(uint id, Transform parent = null)
	{
		SaveablePrefab prefab = GetPrefab(id);
		if ((Object)(object)prefab != (Object)null)
		{
			return Object.Instantiate<SaveablePrefab>(prefab, parent);
		}
		return null;
	}

	public static SaveablePrefab CreatePrefabFromId(JObject prefabIdData, Transform parent = null)
	{
		return CreatePrefabFromId(ConvertLegacyPrefabId(prefabIdData));
	}

	public static SaveablePrefab GetPrefab(uint id)
	{
		if (id == 0)
		{
			Debug.LogError((object)"Prefabs::GetPrefab:: Prefab id is not valid (0)");
			return null;
		}
		foreach (PrefabReference prefabReference in _prefabReferences)
		{
			if (prefabReference.Id == id)
			{
				if ((Object)(object)prefabReference.Prefab == (Object)null)
				{
					prefabReference.Prefab = Resources.Load<SaveablePrefab>(prefabReference.Path);
				}
				return prefabReference.Prefab;
			}
		}
		Debug.LogError((object)("Prefabs::GetPrefab:: Prefab id of [" + id + "] not found."));
		return null;
	}

	public static SaveablePrefab GetPrefab(JObject prefabIdData)
	{
		return GetPrefab(ConvertLegacyPrefabId(prefabIdData));
	}

	public static uint ConvertLegacyPrefabId(string prefabId)
	{
		if (string.IsNullOrEmpty(prefabId))
		{
			return 0u;
		}
		uint value = 0u;
		_stringToUIntIdMap.TryGetValue(prefabId, out value);
		return value;
	}

	public static uint ConvertLegacyPrefabId(JObject prefabIdData)
	{
		if (prefabIdData == null || prefabIdData.Type == JObject.EToken.NULL)
		{
			Debug.LogError((object)"Prefabs::ConvertLegacyPrefabId:: Prefab id was null or empty.");
			return 0u;
		}
		if (prefabIdData.Type == JObject.EToken.STRING)
		{
			return ConvertLegacyPrefabId(prefabIdData.GetValue<string>());
		}
		return prefabIdData.GetValue<uint>();
	}

	public static string ConvertPrefabIdToLegacy(uint prefabId)
	{
		_uIntToStringIdMap.TryGetValue(prefabId, out var value);
		return value;
	}

	public static uint GetPrefabId(JObject data)
	{
		JObject field = data.GetField("prefab");
		if ((bool)field)
		{
			return ConvertLegacyPrefabId(field);
		}
		return 0u;
	}

	public static bool IsMultiplayerEntity(uint prefabId)
	{
		string multiplayerPrefabName;
		return TryGetMultiplayerPrefabName(prefabId, out multiplayerPrefabName);
	}

	public static Vector3 GetPosition(JObject data)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return JSerializer.Deserialize<Vector3>(data.GetField("Transform").GetField("localPosition"));
	}

	public static Vector3 GetPositionFromTransformData(JObject transformData)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return JSerializer.Deserialize<Vector3>(transformData.GetField("localPosition"));
	}

	public static MiniGuid GetReferenceId(JObject data, bool safe = true)
	{
		MiniGuid result = default;
		JObject field = data.GetField("reference");
		if (field != null)
		{
			result = Beam.Utilities.ExtensionMethods.ToMiniGuid(field.GetValue<string>());
		}
		if (safe && result.IsDefault())
		{
			result = MiniGuid.New();
		}
		return result;
	}

	public static void SetReferenceId(JObject data, MiniGuid referenceId)
	{
		JObject field = data.GetField("reference");
		if (field == null)
		{
			data.AddField("reference", referenceId.ToString());
		}
		else
		{
			field.Data = referenceId.ToString();
		}
	}

	public static void LoadPrefabMap()
	{
		if (_stringToUIntIdMap.Count != 0)
		{
			return;
		}
		TextAsset val = Resources.Load<TextAsset>("Prefabs/PrefabsLegacyMappping");
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError((object)"Prefabs:: Error loading prefab map file.");
			return;
		}
		string[] array = val.text.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (!string.IsNullOrEmpty(text))
			{
				string[] array2 = text.Split(',');
				string s = array2[0];
				string text2 = array2[1];
				uint num = uint.Parse(s);
				_stringToUIntIdMap[text2] = num;
				_uIntToStringIdMap[num] = text2;
			}
		}
	}

	public static void LoadPrefabs()
	{
		LoadPrefabMap();
		_prefabReferences.Clear();
		TextAsset val = Resources.Load<TextAsset>("Prefabs/Prefabs");
		if (Object.op_Implicit((Object)(object)val))
		{
			JObject jObject = new JObject(((object)val).ToString());
			if (!jObject.IsNull())
			{
				foreach (JObject child in jObject.Children)
				{
					try
					{
						string value = child.GetField("Path").GetValue<string>();
						uint num = ConvertLegacyPrefabId(child.GetField("Id"));
						if (num == 0)
						{
							Debug.LogError((object)("Prefabs:: Error loading prefab id for path [" + value + "]"));
						}
						else
						{
							SaveablePrefab saveablePrefab = Resources.Load<SaveablePrefab>(value);
							if ((Object)(object)saveablePrefab != (Object)null)
							{
								PrefabReference prefabReference = new PrefabReference();
								prefabReference.Prefab = saveablePrefab;
								prefabReference.Path = value;
								prefabReference.Id = num;
								_prefabReferences.Add(prefabReference);
							}
							else
							{
								Debug.LogError((object)("Prefabs:: Error loading prefab from path [" + value + "]"));
							}
						}
					}
					catch (JSerializerException ex)
					{
						Debug.LogError((object)("Prefabs:: Error restoring prefab!\n" + ex));
					}
				}
				return;
			}
			Debug.LogError((object)"Prefabs:: Saved prefab data was empty!");
		}
		else
		{
			Debug.LogError((object)"Prefabs:: Error finding prefab file.");
		}
	}

	public static void LinkMPPrefabs()
	{
		foreach (PrefabReference prefabReference in _prefabReferences)
		{
			string path = prefabReference.Path;
			uint id = prefabReference.Id;
			if (MultiplayerMng.TryGetPrefabName(path, out var multiplayerPrefabName))
			{
				_prefabsMultiplayerNames[id] = multiplayerPrefabName;
			}
		}
	}
}
