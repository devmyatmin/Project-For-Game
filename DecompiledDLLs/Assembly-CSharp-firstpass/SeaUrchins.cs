using Beam;
using UnityEngine;

public class SeaUrchins : MonoBehaviour
{
	private bool _canAttack = true;

	private void Start()
	{
		if (PlayerUtilities.GetServerWildlifeMode() == EGameWildlifeMode.Passive || PlayerUtilities.GetServerWildlifeMode() == EGameWildlifeMode.Removed)
		{
			_canAttack = false;
		}
	}

	private void OnTriggerEnter(Collider collided)
	{
		if (_canAttack && ((Component)collided).gameObject.CompareTag("Player"))
		{
			((Component)collided).GetComponent<IPlayer>().Statistics.ApplyStatusEffect(StatusEffect.POISON);
		}
	}
}
