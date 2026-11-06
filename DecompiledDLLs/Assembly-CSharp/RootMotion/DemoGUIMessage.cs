using System;
using UnityEngine;

namespace RootMotion;

public class DemoGUIMessage : MonoBehaviour
{
	public string text;

	public Color color = Color.white;

	private void OnGUI()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		GUI.color = color;
		GUILayout.Label(text, Array.Empty<GUILayoutOption>());
		GUI.color = Color.white;
	}

	public DemoGUIMessage()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
