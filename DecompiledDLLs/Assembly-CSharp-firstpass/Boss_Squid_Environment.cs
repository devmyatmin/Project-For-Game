using Beam;
using UnityEngine;

public class Boss_Squid_Environment : MonoBehaviour
{
	[SerializeField]
	private Transform _buoy;

	[SerializeField]
	private Transform _buoyChain;

	[SerializeField]
	private Light _buoyLight;

	[SerializeField]
	private AudioSource _buoyBellAudio;

	private float _buoyBellTime;

	private float _buoyBellTimer;

	private void Start()
	{
		TriggerLight();
		if (Object.op_Implicit((Object)(object)Singleton<Atmosphere>.Instance))
		{
			Singleton<Atmosphere>.Instance.SunsetEvent += TriggerLight;
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)Singleton<Atmosphere>.Instance))
		{
			Singleton<Atmosphere>.Instance.SunsetEvent -= TriggerLight;
		}
	}

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		_buoyChain.position = new Vector3(_buoyChain.position.x, _buoy.position.y, _buoyChain.position.z);
		_buoyBellTime += Time.deltaTime;
		if (_buoyBellTime > _buoyBellTimer)
		{
			_buoyBellTime = 0f;
			_buoyBellTimer = Random.Range(5, 15);
			_buoyBellAudio.pitch = Random.Range(0.95f, 1.05f);
			_buoyBellAudio.Play();
			if (Random.value < 0.1f)
			{
				_buoyBellTimer = 1f;
			}
		}
	}

	private void TriggerLight()
	{
		((Behaviour)_buoyLight).enabled = !Singleton<Atmosphere>.Instance.DayTime;
	}
}
