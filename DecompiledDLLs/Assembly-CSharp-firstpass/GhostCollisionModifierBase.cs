using UnityEngine;

public abstract class GhostCollisionModifierBase : MonoBehaviour
{
	public abstract bool ValidateCollision(Collider other);
}
