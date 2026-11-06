using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class InteractiveParticleHelper : MonoBehaviour
{
	public virtual void PlayDamageParticle(IBase sender, IBaseActionEventData data)
	{
		if (data.OtherOwnerID != -1)
		{
			IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(data.OtherOwnerID);
			if (!BeamTeam.IsNullOrDestroyed(playerFromId))
			{
				ParticleManager.CreateDamageParticle(playerFromId);
			}
		}
	}
}
