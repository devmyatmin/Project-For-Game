using System.Collections.Generic;
using Beam;
using Beam.Serialization.Json;
using UnityEngine;

public class InputSettings : ISaveable
{
	public Quality_OFF_ON ToggleRadialMenus = Quality_OFF_ON.OFF;

	public Quality_OFF_ON CrouchToggle;

	public Quality_OFF_ON SprintToggle;

	public Quality_OFF_ON AutoWalk;

	public Quality_OFF_ON InvertY = Quality_OFF_ON.OFF;

	public float MouseSensitivity = 0.5f;

	public float MouseSmoothness = 0.2f;

	public Dictionary<string, ControllerAssignment> ControllerAssignments = new Dictionary<string, ControllerAssignment>();

	public string RewiredJsonData;

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("ToggleRadialMenus", JSerializer.Serialize((object)ToggleRadialMenus));
		jObject.AddField("CrouchToggle", JSerializer.Serialize((object)CrouchToggle));
		jObject.AddField("SprintToggle", JSerializer.Serialize((object)SprintToggle));
		jObject.AddField("AutoWalk", JSerializer.Serialize((object)AutoWalk));
		jObject.AddField("InvertY", JSerializer.Serialize((object)InvertY));
		jObject.AddField("MouseSmoothness", JSerializer.Serialize((object)MouseSmoothness));
		jObject.AddField("MouseSensitivity", JSerializer.Serialize((object)MouseSensitivity));
		JObject jObject2 = new JObject();
		foreach (ControllerAssignment value in ControllerAssignments.Values)
		{
			JObject jObject3 = new JObject();
			jObject3.AddField("pId", value.PlayerId);
			jObject3.AddField("name", value.ControllerName);
			jObject3.AddField("id", value.ControllerGuid);
			jObject2.Add(jObject3);
		}
		jObject.AddField("ControllerAssignments", jObject2);
		if (!string.IsNullOrEmpty(RewiredJsonData))
		{
			JObject data = new JObject(RewiredJsonData);
			jObject.AddField("RewiredData", data);
		}
		return jObject;
	}

	public void Load(JObject data)
	{
		ToggleRadialMenus = JSerializer.Deserialize<Quality_OFF_ON>(data.GetField("ToggleRadialMenus"));
		CrouchToggle = JSerializer.Deserialize<Quality_OFF_ON>(data.GetField("CrouchToggle"));
		JObject field = data.GetField("SprintToggle");
		if (field != null)
		{
			SprintToggle = JSerializer.Deserialize<Quality_OFF_ON>(field);
		}
		JObject field2 = data.GetField("AutoWalk");
		if (field2 != null)
		{
			AutoWalk = JSerializer.Deserialize<Quality_OFF_ON>(field2);
		}
		InvertY = JSerializer.Deserialize<Quality_OFF_ON>(data.GetField("InvertY"));
		MouseSmoothness = data.GetField("MouseSmoothness").GetValue<float>();
		MouseSensitivity = data.GetField("MouseSensitivity").GetValue<float>();
		JObject field3 = data.GetField("ControllerAssignments");
		LoadControllerAssignments(field3);
	}

	private void LoadControllerAssignments(JObject controllerAssignmentsData)
	{
		if (controllerAssignmentsData == null || controllerAssignmentsData.IsNull())
		{
			Debug.LogWarning((object)"LoadControllerAssignments:: Null or empty.");
			return;
		}
		ControllerAssignments.Clear();
		for (int i = 0; i < controllerAssignmentsData.Children.Count; i++)
		{
			JObject jObject = controllerAssignmentsData.Children[i];
			int value = jObject.GetField("pId").GetValue<int>();
			string value2 = jObject.GetField("id").GetValue<string>();
			string text = value2;
			JObject field = jObject.GetField("name");
			if (field != null)
			{
				text = field.GetValue<string>();
			}
			ControllerAssignment controllerAssignment = new ControllerAssignment();
			controllerAssignment.ControllerName = text;
			controllerAssignment.ControllerGuid = value2;
			controllerAssignment.PlayerId = value;
			ControllerAssignments.Add(value2, controllerAssignment);
			Debug.Log((object)$"LoadControllerAssignments:: [{value}/{text}] assignment loaded.");
		}
	}
}
