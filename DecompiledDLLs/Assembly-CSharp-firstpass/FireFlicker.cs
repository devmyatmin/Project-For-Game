using UnityEngine;

public class FireFlicker : MonoBehaviour
{
	[SerializeField]
	private float _minIntensity;

	[SerializeField]
	private float _maxIntensity;

	private float _random;

	private Light _light;

	private void Start()
	{
		_light = ((Component)this).GetComponent<Light>();
		_random = Random.Range(0f, 2.1474836E+09f);
	}

	private void Update()
	{
		float num = Mathf.PerlinNoise(_random, Time.time * 5f);
		_light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, num);
	}
}
