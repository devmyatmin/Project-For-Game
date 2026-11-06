using System;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

namespace Beam.Serialization;

public static class SavingUtilities
{
	public static JObject Save(ISaveable instance)
	{
		try
		{
			if (instance == null)
			{
				throw new JSerializerException("Instance was null");
			}
			JObject jObject = instance.Save();
			if (jObject != null)
			{
				return jObject;
			}
			Debug.LogError((object)$"SavingUtilities::Save:: Serialized object data was null for '{instance}'.");
			return null;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)($"SavingUtilities::Save:: Error saving object '{instance}'\n\n" + ex));
			return null;
		}
	}

	public static JObject SavePrefabToJObject(ISaveablePrefab instance)
	{
		if (instance.PrefabId == 0)
		{
			Debug.LogError((object)("SavingUtilities::SavePrefabToJObject:: Prefab Id was invalid (0) [" + instance?.ToString() + "]"));
			return null;
		}
		return Save(instance);
	}

	public static bool Load(ISaveable instance, JObject data)
	{
		if (data == null || data.IsNull())
		{
			Debug.LogError((object)("SavingUtilities::Load:: Data was null or empty [" + instance?.GetType().Name + "]."));
			return false;
		}
		try
		{
			instance.Load(data);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("SavingUtilities::Load:: Error loading object [" + instance?.GetType().Name + "]\n" + ex));
			return false;
		}
	}

	public static void LoadPrefabFromJData(MultiplayerZoneSync.Usage usage, JObject prefabData, Action<SaveablePrefab> preload = null, Action<SaveablePrefab> postLoad = null)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		uint prefabId = Prefabs.GetPrefabId(prefabData);
		if (prefabId == 0)
		{
			Debug.LogError((object)"SavingUtilities::LoadPrefab:: Error loading prefab. Prefab id was null or empty.");
			return;
		}
		MiniGuid referenceId = Prefabs.GetReferenceId(prefabData);
		if (MPExtensions.IsMaster(Game.Mode) || !Prefabs.IsMultiplayerEntity(Prefabs.GetPrefabId(prefabData)))
		{
			Load(MultiplayerMng.Instantiate<SaveablePrefab>(prefabId, referenceId), prefabData, preload, postLoad);
			return;
		}
		MultiplayerZoneSync.Instance.LinkData(usage, referenceId, (MultiplayerZoneSync.Command command) =>
		{
			Load(command.Instance, prefabData, preload, postLoad);
		});
	}

	private static void Load(SaveablePrefab saveablePrefab, JObject data, Action<SaveablePrefab> preload, Action<SaveablePrefab> postLoad)
	{
		if (Object.op_Implicit((Object)(object)saveablePrefab))
		{
			preload?.Invoke(saveablePrefab);
			if (Load(saveablePrefab, data))
			{
				postLoad?.Invoke(saveablePrefab);
			}
			else
			{
				Debug.LogError((object)("SavingUtilities::LoadPrefab:: Error loading. \n\nName: " + ((Object)saveablePrefab).name + "\nReference Id: " + saveablePrefab.ReferenceId.ToString()));
			}
		}
	}
}
