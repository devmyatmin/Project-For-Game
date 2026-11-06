using UnityEngine;

public class ParticlePoolDestroy : MonoBehaviour
{
	private ParticleSystem _ps;

	[SerializeField]
	private bool _pool;

	private void Awake()
	{
		_ps = ((Component)this).GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)_ps) && _pool && !_ps.IsAlive(true))
		{
			if (_pool)
			{
				ParticleManager.DestroyParticle(((Component)this).gameObject);
			}
			else
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}
}
