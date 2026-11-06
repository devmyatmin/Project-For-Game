using Beam;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Player_Ledge : MonoBehaviour
{
	public Transform LandPos;

	public bool IsRaft;

	private void OnTriggerEnter(Collider collided)
	{
		if (((Component)collided).CompareTag("Player"))
		{
			Player component = ((Component)collided).GetComponent<Player>();
			if ((Object)(object)component != (Object)null)
			{
				component.Movement.activeLedge = this;
			}
		}
	}

	private void OnTriggerExit(Collider collided)
	{
		if (((Component)collided).CompareTag("Player"))
		{
			Player component = ((Component)collided).GetComponent<Player>();
			if ((Object)(object)component != (Object)null && (Object)(object)component.Movement.activeLedge == (Object)(object)this)
			{
				component.Movement.activeLedge = null;
			}
		}
	}
}
