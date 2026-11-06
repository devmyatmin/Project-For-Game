using UnityEngine;

namespace Beam;

public class AnimationEventPlayer : MonoBehaviour
{
	private Footstep _footstep;

	public void SetPlayer(IPlayer player)
	{
		_footstep = player.Audio.Footstep;
	}

	public void PlayFootstepEvent(float intensity = 1f)
	{
		_footstep.PlayFootstep(intensity);
	}
}
