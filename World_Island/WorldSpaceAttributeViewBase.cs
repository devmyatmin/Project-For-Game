using UnityEngine;

public abstract class WorldSpaceAttributeViewBase : MonoBehaviour
{
	public abstract void Show(IHaveAttribute attribute);

	public abstract void Hide();
}
