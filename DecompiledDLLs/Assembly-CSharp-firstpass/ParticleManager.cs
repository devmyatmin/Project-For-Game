using Beam;
using UnityEngine;

public class ParticleManager : Singleton<ParticleManager>
{
	public static Transform TRAIL_CONTAINER;

	[SerializeField]
	private Transform _trailContainer;

	public GameObject bubbleBurst;

	public GameObject PARTICLE_COOKING_SMOKE;

	public GameObject PARTICLE_FOOD_SPOIL;

	private void Awake()
	{
		TRAIL_CONTAINER = _trailContainer;
	}

	public static void CreateDamageParticle(IPlayer player)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit lastHitInfo = player.GameInput.Raycaster.LastHitInfo;
		Vector3 point = lastHitInfo.point;
		Quaternion rotation = Quaternion.LookRotation(player.transform.position - point);
		PhysicMaterial sharedMaterial = lastHitInfo.collider.sharedMaterial;
		GameObject particleFromMaterial = ParticleUtiliity.GetParticleFromMaterial((sharedMaterial != null) ? ((Object)sharedMaterial).name : null);
		if ((Object)(object)particleFromMaterial != (Object)null)
		{
			ParticleSystem component = ObjectPoolManager.CreatePooled(particleFromMaterial, point, rotation).GetComponent<ParticleSystem>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Component)component).GetComponent<Renderer>().enabled = true;
				component.time = 0f;
				component.Clear(true);
				component.Play(true);
			}
		}
	}

	public static GameObject CreateParticle(GameObject particle, Vector3 position, Vector3 rotation = default(Vector3))
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)particle))
		{
			return null;
		}
		GameObject val = ObjectPoolManager.CreatePooled(particle, position, Quaternion.Euler(rotation));
		ParticleSystem component = val.GetComponent<ParticleSystem>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((Component)component).GetComponent<Renderer>().enabled = true;
			component.time = 0f;
			component.Clear(true);
			component.Play(true);
		}
		return val;
	}

	public static GameObject CreateParticle(GameObject particle, Transform parent)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)particle))
		{
			return null;
		}
		GameObject val = ObjectPoolManager.CreatePooled(particle, parent.position, Quaternion.identity);
		val.transform.parent = parent;
		ParticleSystem component = val.GetComponent<ParticleSystem>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((Component)component).GetComponent<Renderer>().enabled = true;
			component.time = 0f;
			component.Clear(true);
			component.Play(true);
		}
		return val;
	}

	public static void DestroyParticle(GameObject particle)
	{
		if (Object.op_Implicit((Object)(object)particle))
		{
			ParticleSystem component = particle.GetComponent<ParticleSystem>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.Stop();
				component.time = 0f;
				component.Clear(true);
				((Component)component).GetComponent<Renderer>().enabled = false;
			}
			ObjectPoolManager.DestroyPooled(particle);
		}
	}
}
