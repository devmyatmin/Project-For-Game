using System.Collections.Generic;
using System.Linq;
using Beam.Serialization.Json;
using Rewired;
using UnityEngine;

namespace Beam;

public class RewiredOptions : MonoBehaviour
{
	private void Awake()
	{
		Options.PreSave += Options_PreSave;
		LegacyUtilities.ResetInput_Pre_65_05();
		Options.Loaded += Options_Loaded;
	}

	private void OnDestroy()
	{
		Options.PreSave -= Options_PreSave;
		Options.Loaded -= Options_Loaded;
	}

	private void Options_PreSave()
	{
		SaveRewiredSettings();
	}

	private void Options_Loaded(JObject data)
	{
		LoadRewiredSettings(data);
	}

	private void SaveRewiredSettings()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		PlayerSaveData saveData = ReInput.players.GetPlayer(0).GetSaveData(false);
		JObject jObject = new JObject();
		JObject jObject2 = new JObject();
		MouseMapSaveData[] mouseMapSaveData = saveData.mouseMapSaveData;
		for (int i = 0; i < mouseMapSaveData.Length; i++)
		{
			JObject data = new JObject(((ControllerMapSaveData)mouseMapSaveData[i]).map.ToJsonString());
			jObject2.Add(data);
		}
		jObject.AddField("MouseMaps", jObject2);
		JObject jObject3 = new JObject();
		KeyboardMapSaveData[] keyboardMapSaveData = saveData.keyboardMapSaveData;
		for (int i = 0; i < keyboardMapSaveData.Length; i++)
		{
			JObject data2 = new JObject(((ControllerMapSaveData)keyboardMapSaveData[i]).map.ToJsonString());
			jObject3.Add(data2);
		}
		jObject.AddField("KeyboardMaps", jObject3);
		Options.InputSettings.RewiredJsonData = jObject.ToString();
	}

	public void LoadRewiredSettings(JObject data)
	{
		JObject field = data.GetField("Input");
		if (field == null || field.IsNull())
		{
			Debug.LogError((object)"LoadRewiredSettings:: Null or empty.");
			return;
		}
		JObject field2 = field.GetField("RewiredData");
		if (field2 == null || field2.IsNull())
		{
			Debug.LogError((object)"LoadRewiredSettings:: Null or empty.");
			return;
		}
		JObject controllerMapsData = field2.GetField("ControllerMaps") ?? field2;
		LoadControllerMaps(controllerMapsData);
	}

	private void LoadControllerMaps(JObject controllerMapsData)
	{
		if (controllerMapsData == null || controllerMapsData.IsNull())
		{
			Debug.LogWarning((object)"LoadControllerMaps:: Null or empty.");
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		JObject field = controllerMapsData.GetField("MouseMaps");
		if (field != null && !field.IsNull())
		{
			Debug.Log((object)"LoadControllerMaps:: Loading saved mouse maps..");
			List<string> list = field.Children.Select((JObject data) => data.ToString()).ToList();
			player.controllers.maps.ClearMaps((ControllerType)1, false);
			player.controllers.maps.AddMapsFromJson((ControllerType)1, 0, list);
			RewiredUtilities.RefreshMapsEnabled(player, (ControllerType)1);
		}
		JObject field2 = controllerMapsData.GetField("KeyboardMaps");
		if (field2 != null && !field2.IsNull())
		{
			Debug.Log((object)"LoadControllerMaps:: Loading saved keyboard maps..");
			List<string> list2 = field2.Children.Select((JObject data) => data.ToString()).ToList();
			player.controllers.maps.ClearMaps((ControllerType)0, false);
			player.controllers.maps.AddMapsFromJson((ControllerType)0, 0, list2);
			RewiredUtilities.RefreshMapsEnabled(player, (ControllerType)0);
		}
		FixMissingInputAssignments();
	}

	private static void FixMissingInputAssignments()
	{
		foreach (Player player in RewiredUtilities.Players)
		{
			if (!RewiredUtilities.MapCategoryHasAction(player, (ControllerType)1, 0, 81, 3, 15))
			{
				RewiredUtilities.CreateActionElementMap(player, (ControllerType)1, 0, 81, 3, 15, (KeyCode)0);
			}
			if (!RewiredUtilities.MapCategoryHasAction(player, (ControllerType)1, 0, 82, 4, 15))
			{
				RewiredUtilities.CreateActionElementMap(player, (ControllerType)1, 0, 82, 4, 15, (KeyCode)0);
			}
			if (!RewiredUtilities.MapCategoryHasAction(player, (ControllerType)0, 0, 39, 60, 15))
			{
				RewiredUtilities.CreateActionElementMap(player, (ControllerType)0, 0, 39, 60, 15, (KeyCode)27);
			}
		}
	}
}
