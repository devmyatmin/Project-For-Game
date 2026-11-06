using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class UserInterfaceAssigner : MonoBehaviour
{
	[SerializeField]
	private UserInterfaceProviderBase m_userInterfaceProvider;

	private void Awake()
	{
		UserInterface userInterface = m_userInterfaceProvider.GetUserInterface();
		if (userInterface != null)
		{
			Selectable[] componentsInChildren = ((Component)((Component)this).transform).GetComponentsInChildren<Selectable>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetUserInterface(userInterface);
			}
		}
	}
}
