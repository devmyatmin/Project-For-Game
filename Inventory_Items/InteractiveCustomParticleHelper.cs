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
		if (_followTransform)
		{
			ParticleManager.CreateParticle(_particle, base.transform);
		}
		else
		{
			ParticleManager.CreateParticle(_particle, base.transform.position);
		}
	}
}
