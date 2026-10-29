using Beam;
using UnityEngine;

public class DestroyPlayersOnDestroy : MonoBehaviour
{
	private void OnDestroy()
	{
		PlayerRegistry.DestroyLocalPlayers();
	}
}
