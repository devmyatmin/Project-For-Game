using UnityEngine;

namespace Beam.UI;

public class UnityInputAdapter : IInputProvider
{
	public bool AnyKey => Input.anyKey;
}
