using UnityEngine;

namespace Beam;

public class InteractiveCustomParticleHelper : MonoBehaviour
{
	[SerializeField]
	private GameObject _particle;

	[SerializeField]
	private bool _followTransform;

	public virtual void PlayCustomParticle()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (_followTransform)
		{
			ParticleManager.CreateParticle(_particle, ((Component)this).transform);
		}
		else
		{
			ParticleManager.CreateParticle(_particle, ((Component)this).transform.position);
		}
	}
}
