using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public abstract class UserInterfaceProviderBase : MonoBehaviour
{
	public abstract UserInterface GetUserInterface();
}
