using Beam;
using Beam.AccountServices;
using Beam.Events;
using Funlabs;
using UnityEngine;

public class AchievementAwarder : MonoBehaviour
{
	[SerializeField]
	private EAchievement _achievement;

	private bool _awarded;

	public void AwardTrophy(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((!MPExtensions.IsMultiplayer(Game.Mode) || (player != null && player.IsOwner)) && !_awarded)
		{
			EventManager.RaiseEvent(new AchievementUnlockEvent(_achievement));
			_awarded = true;
		}
	}
}
