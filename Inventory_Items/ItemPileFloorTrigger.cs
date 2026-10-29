using UnityEngine;

public class ItemPileFloorTrigger : MonoBehaviour
{
	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.TryGetComponent<Rigidbody>(out var component))
		{
			component.isKinematic = true;
		}
	}
}
