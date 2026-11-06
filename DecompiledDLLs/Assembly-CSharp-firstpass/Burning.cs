using System.Collections;
using Beam;
using Beam.Serialization.Json;
using UnityEngine;

public class Burning : MonoBehaviour
{
	[SerializeField]
	private float _fuelHours;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private InteractiveObject _interactiveObject;

	private bool _burning;

	public float FuelHours => _fuelHours;

	public InteractiveObject InteractiveObject => _interactiveObject;

	public virtual void Awake()
	{
		_interactiveObject.Burning = this;
	}

	public void StartBurning(float heat)
	{
		((MonoBehaviour)this).StartCoroutine(Burning_Sequence(heat));
	}

	public void StopBurning()
	{
		_burning = false;
	}

	protected virtual IEnumerator Burning_Sequence(float heat)
	{
		_burning = true;
		while (_burning)
		{
			Burn(heat);
			yield return null;
		}
	}

	public virtual void Burn(float heat)
	{
		if (_fuelHours > 0f)
		{
			_fuelHours -= heat * GameTime.DeltaTimeHour;
		}
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("fuelPoints", JSerializer.Serialize((object)_fuelHours));
		return jObject;
	}

	public void Load(JObject data)
	{
		float value = data.GetField("fuelPoints").GetValue<float>();
		_fuelHours = value;
	}
}
