using Beam;
using Beam.Utilities;
using UnityEngine;

public class Ladders : MonoBehaviour
{
	public bool isTree;

	public Transform LandPos;

	[HideInInspector]
	public BoxCollider LadderCollider;

	private void Awake()
	{
		LadderCollider = ((Component)this).GetComponent<BoxCollider>();
	}

	private void OnTriggerEnter(Collider collided)
	{
		if (((Component)collided).CompareTag("Player"))
		{
			IPlayer ladder = BeamTeam.GetInterface<IPlayer>((Component)(object)collided);
			SetLadder(ladder);
		}
	}

	private void OnTriggerExit(Collider collided)
	{
		if (((Component)collided).CompareTag("Player"))
		{
			IPlayer player = BeamTeam.GetInterface<IPlayer>((Component)(object)collided);
			ResetLadder(player);
		}
	}

	private void SetLadder(IPlayer player)
	{
		if (!BeamTeam.IsNullOrDestroyed(player))
		{
			player.Movement.activeLadder = this;
		}
	}

	private void ResetLadder(IPlayer player)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(player) && (Object)(object)player.Movement.activeLadder == (Object)(object)this)
		{
			if (player.Movement.ClimbingLadder)
			{
				player.Movement.CharacterController.Move(-((Component)player.Movement).transform.forward * 1f + ((Component)player.Movement).transform.up * 0.2f * Time.deltaTime);
			}
			player.Movement.ExitLadder();
		}
	}
}
