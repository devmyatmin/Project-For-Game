using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class UserInterfaceProvider : UserInterfaceProviderBase
{
	[SerializeField]
	private UserInterface m_userInterface;

	public override UserInterface GetUserInterface()
	{
		return m_userInterface;
	}
}
