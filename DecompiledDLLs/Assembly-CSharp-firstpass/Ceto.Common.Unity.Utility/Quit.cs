using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public class Quit : MonoBehaviour
{
	public KeyCode quitKey = (KeyCode)27;

	private void OnGUI()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown(quitKey))
		{
			Application.Quit();
		}
	}

	public Quit()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
	}
}
