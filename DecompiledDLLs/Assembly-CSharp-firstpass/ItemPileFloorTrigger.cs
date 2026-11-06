using UnityEngine;

public class ItemPileFloorTrigger : MonoBehaviour
{
	private void OnCollisionEnter(Collision other)
	{
		Rigidbody val = default;
		if (other.gameObject.TryGetComponent<Rigidbody>(ref val))
		{
			val.isKinematic = true;
		}
	}
}
