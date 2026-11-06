using UnityEngine;

namespace Beam.UI;

public abstract class ControllerBase : MonoBehaviour
{
	public abstract void SetView(ViewAdapterBase view);
}
