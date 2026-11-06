using UnityEngine;

namespace RootMotion.Demos;

public class ApplicationQuit : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)113) || Input.GetKeyDown((KeyCode)27))
		{
			Application.Quit();
		}
	}
}
