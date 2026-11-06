using System.Collections;
using UnityEngine;

public class FlareGunProjectile : MonoBehaviour
{
	[SerializeField]
	private GameObject _explosion;

	[SerializeField]
	private GameObject _smokeTrail;

	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private AudioClip _explosionClip;

	private bool _destroying;

	private float _lifeTime;

	private bool _detonated;

	private bool _hovering;

	private void Start()
	{
		if ((Object)(object)_rigidbody == (Object)null)
		{
			_rigidbody = ((Component)this).GetComponent<Rigidbody>();
		}
	}

	private void Update()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (!_detonated && _lifeTime > 0.5f)
		{
			_detonated = true;
			Explode();
		}
		_lifeTime += Time.deltaTime;
		if (!_detonated)
		{
			return;
		}
		if (!_hovering)
		{
			RaycastHit val = default;
			if (Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val, 25f, (1 << Layers.TERRAIN) | (1 << Layers.TERRAIN_OBJECTS) | (1 << Layers.CONSTRUCTIONS)))
			{
				_hovering = true;
				Hover();
			}
		}
		else
		{
			Transform transform = ((Component)this).transform;
			transform.position += Vector3.down * 0.02f * Time.deltaTime;
		}
		int num = 4;
		if ((_lifeTime >= (float)(60 * num) || ((Component)this).transform.position.y < 0f) && !_destroying)
		{
			_destroying = true;
			((MonoBehaviour)this).StartCoroutine("DestroyOverTime");
		}
	}

	private void Hover()
	{
		_rigidbody.isKinematic = true;
	}

	private void Explode()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).transform.position.y > 0f)
		{
			_explosion.SetActive(true);
		}
		AudioManager.GetAudioPlayer().Play3D(_explosionClip, ((Component)this).transform.position, AudioMixerChannels.FX);
		EmissionModule emission = _smokeTrail.GetComponent<ParticleSystem>().emission;
		emission.enabled = false;
		_rigidbody.drag = 4f;
	}

	private IEnumerator DestroyOverTime()
	{
		_explosion.GetComponent<ParticleSystem>().Stop(true);
		yield return (object)new WaitForSeconds(5f);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
