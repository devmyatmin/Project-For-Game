using UnityEngine;
using UnityEngine.UI;

namespace LE_LevelEditor.UI;

public class LE_HelpSelector : MonoBehaviour
{
	[SerializeField]
	private Toggle MOUSE_AND_KEYBOARD_TOGGLE;

	private void Start()
	{
		if ((Object)(object)MOUSE_AND_KEYBOARD_TOGGLE != (Object)null)
		{
			MOUSE_AND_KEYBOARD_TOGGLE.isOn = true;
		}
		else
		{
			Debug.LogError((object)"LE_HelpSelector: MOUSE_AND_KEYBOARD_TOGGLE were not set in inspector!");
		}
	}
}
