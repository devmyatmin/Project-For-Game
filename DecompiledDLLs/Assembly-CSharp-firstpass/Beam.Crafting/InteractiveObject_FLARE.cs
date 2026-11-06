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
		_flareLid.SetActive(false);
		_particles.SetActive(true);
		((MonoBehaviour)this).StartCoroutine("FlareTimer");
	}

	private IEnumerator FlareTimer()
	{
		while (_flareTimer < 60f)
		{
			_flareTimer++;
			yield return (object)new WaitForSeconds(1f);
		}
		if (_flareTimer > 60f)
		{
			_particles.GetComponent<ParticleSystem>().Stop(true);
			yield return (object)new WaitForSeconds(5f);
			_particles.SetActive(false);
		}
		yield return null;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("flareTimer", JSerializer.Serialize((object)_flareTimer));
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		float value = data.GetField("flareTimer").GetValue<float>();
		_flareTimer = value;
	}
}
