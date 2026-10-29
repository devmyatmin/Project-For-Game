using System.Collections;
using Beam.Serialization.Json;
using UnityEngine;

namespace Beam.Crafting;

public class InteractiveObject_FLARE : InteractiveObject
{
	[SerializeField]
	private GameObject _flareLid;

	[SerializeField]
	private GameObject _particles;

	private bool used;

	private float _flareTimer;

	public override void Use()
	{
		if (!used)
		{
			used = true;
			StartFlare();
			base.Use();
		}
	}

	private void StartFlare()
	{
		_flareLid.SetActive(value: false);
		_particles.SetActive(value: true);
		StartCoroutine("FlareTimer");
	}

	private IEnumerator FlareTimer()
	{
		while (_flareTimer < 60f)
		{
			_flareTimer += 1f;
			yield return new WaitForSeconds(1f);
		}
		if (_flareTimer > 60f)
		{
			_particles.GetComponent<ParticleSystem>().Stop(withChildren: true);
			yield return new WaitForSeconds(5f);
			_particles.SetActive(value: false);
		}
		yield return null;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("flareTimer", JSerializer.Serialize(_flareTimer));
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		float value = data.GetField("flareTimer").GetValue<float>();
		_flareTimer = value;
	}
}
