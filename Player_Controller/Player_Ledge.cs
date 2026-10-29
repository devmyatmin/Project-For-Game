using Beam;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Player_Ledge : MonoBehaviour
{
	public Transform LandPos;

	public bool IsRaft;

	private void OnTriggerEnter(Collider collided)
	{
		if (collided.CompareTag("Player"))
		{
			Player component = collided.GetComponent<Player>();
			if (component != null)
			{
				component.Movement.activeLedge = this;
			}
		}
	}

	private void OnTriggerExit(Collider collided)
	{
		if (collided.CompareTag("Player"))
		{
			Player component = collided.GetComponent<Player>();
			if (component != null && component.Movement.activeLedge == this)
			{
				component.Movement.activeLedge = null;
			}
		}
	}
}
