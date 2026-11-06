using UnityEngine;

public class DemoGUI : MonoBehaviour
{
	public GameObject[] Prefabs;

	private int currentNomber;

	private GameObject currentInstance;

	private GUIStyle guiStyleHeader = new GUIStyle();

	private void Start()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		guiStyleHeader.fontSize = 15;
		guiStyleHeader.normal.textColor = new Color(0.15f, 0.15f, 0.15f);
		currentInstance = Object.Instantiate<GameObject>(Prefabs[currentNomber], ((Component)this).transform.position, default(Quaternion));
		currentInstance.AddComponent<DemoReactivator>().TimeDelayToReactivate = 1.5f;
	}

	private void OnGUI()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (GUI.Button(new Rect(10f, 15f, 105f, 30f), "Previous Effect"))
		{
			ChangeCurrent(-1);
		}
		if (GUI.Button(new Rect(130f, 15f, 105f, 30f), "Next Effect"))
		{
			ChangeCurrent(1);
		}
		GUI.Label(new Rect(300f, 15f, 100f, 20f), "Prefab name is \"" + ((Object)Prefabs[currentNomber]).name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
	}

	private void ChangeCurrent(int delta)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		currentNomber += delta;
		if (currentNomber > Prefabs.Length - 1)
		{
			currentNomber = 0;
		}
		else if (currentNomber < 0)
		{
			currentNomber = Prefabs.Length - 1;
		}
		if ((Object)(object)currentInstance != (Object)null)
		{
			Object.Destroy((Object)(object)currentInstance);
		}
		currentInstance = Object.Instantiate<GameObject>(Prefabs[currentNomber], ((Component)this).transform.position, default(Quaternion));
		currentInstance.AddComponent<DemoReactivator>().TimeDelayToReactivate = 1.5f;
	}

	public DemoGUI()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
	}
}
