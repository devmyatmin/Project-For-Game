using Rewired;
using UnityEngine;

public class DebugInputMaps : MonoBehaviour
{
	public bool ShowGUI;

	private void OnGUI()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		if (!ShowGUI)
		{
			return;
		}
		GUI.Label(new Rect(10f, 120f, 500f, 20f), "Enabled Input Maps:");
		Player player = ReInput.players.GetPlayer(0);
		if (player == null)
		{
			return;
		}
		Controller lastActiveController = player.controllers.GetLastActiveController();
		if (lastActiveController == null)
		{
			GUI.Label(new Rect(10f, 140f, 500f, 20f), "Null");
			return;
		}
		GUI.Label(new Rect(10f, 140f, 500f, 20f), $"Controller info: {lastActiveController.name}, Type: {lastActiveController.type}");
		int num = 0;
		foreach (ControllerMap map in player.controllers.maps.GetMaps(lastActiveController.type, lastActiveController.id))
		{
			if (map.enabled)
			{
				string name = ((InputCategory)ReInput.mapping.GetMapCategory(map.categoryId)).name;
				GUI.Label(new Rect(10f, (float)(160 + num++ * 20), 500f, 20f), "Map: " + name);
			}
		}
	}
}
