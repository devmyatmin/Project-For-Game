using System;
using UnityEngine;

namespace Beam.Testing;

public class Lighting : MonoBehaviour
{
	[SerializeField]
	private Light _sunIlluminary;

	[SerializeField]
	private Light _moonIlluminary;

	[SerializeField]
	private float _sunLightIntensity;

	[SerializeField]
	private float _moonLightIntensity;

	[SerializeField]
	private Gradient _sunLightColorGradient;

	[SerializeField]
	private Gradient _moonLightColorGradient;

	[SerializeField]
	[HideInInspector]
	private float _timeOfDay = 12f;

	private float _latitude;

	private float _longitude;

	private float _date = 180f;

	private float _sunOrbitRadius;

	private float _altitude;

	[SerializeField]
	private Gradient _ambientGroundColorGradient;

	[SerializeField]
	private ReflectionProbe _environmentReflectionProbe;

	private Material _skydomeMaterial;

	[SerializeField]
	private Texture2D _transmittance;

	[SerializeField]
	private Texture2D _inscatter;

	private Texture3D _inscatterEncoded;

	[SerializeField]
	private Texture2D _irradiance;

	[SerializeField]
	private Texture2D _ambientLUT;

	private Color _ambientLightColor;

	private Transform _transform;

	private Transform _mainCameraTransform;

	public Light SunIlluminary => _sunIlluminary;

	public Light MoonIlluminary => _moonIlluminary;

	public float TimeOfDay
	{
		get
		{
			return _timeOfDay;
		}
		set
		{
			_timeOfDay = value;
		}
	}

	public Gradient AmbientGroundColorGradient
	{
		get
		{
			return _ambientGroundColorGradient;
		}
		set
		{
			_ambientGroundColorGradient = value;
		}
	}

	public Texture2D Transmittance
	{
		get
		{
			return _transmittance;
		}
		set
		{
			_transmittance = value;
		}
	}

	public Texture2D Inscatter
	{
		get
		{
			return _inscatter;
		}
		set
		{
			_inscatter = value;
		}
	}

	public Texture2D Irradiance
	{
		get
		{
			return _irradiance;
		}
		set
		{
			_irradiance = value;
		}
	}

	private void Awake()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		_transform = ((Component)this).GetComponent<Transform>();
		_mainCameraTransform = ((Component)Camera.main).GetComponent<Transform>();
		_sunOrbitRadius = _transform.localScale.x * 0.999f;
		ConvertLookUpTable(_inscatter);
		InitializeShaders();
		_sunIlluminary.shadowStrength = 0.98f;
		_moonIlluminary.shadowStrength = 0.98f;
		RenderSettings.ambientIntensity = 1f;
		if (SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 11"))
		{
			((Component)_environmentReflectionProbe).gameObject.SetActive(true);
		}
		_skydomeMaterial = ((Component)this).GetComponent<Renderer>().sharedMaterial;
		InitializeMaterial(_skydomeMaterial);
	}

	private void ConvertLookUpTable(Texture2D texture)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected Obj, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)texture))
		{
			if (((Texture)texture).height != Mathf.FloorToInt(Mathf.Sqrt((float)((Texture)texture).width)))
			{
				Debug.LogWarning((object)("Atmosphere::ConvertLookUpTable(): The given 2D texture " + ((Object)texture).name + " cannot be used as a 3D LUT."));
				return;
			}
			int num = 16;
			int height = ((Texture)texture).height;
			int num2 = 16;
			Color[] pixels = texture.GetPixels();
			Color[] array = new Color[pixels.Length];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < height; j++)
				{
					for (int k = 0; k < num2; k++)
					{
						array[i + j * num + k * num * height] = pixels[i + j * num * num2 + k * num];
					}
				}
			}
			if (Object.op_Implicit((Object)(object)_inscatterEncoded))
			{
				Object.DestroyImmediate((Object)(object)_inscatterEncoded);
			}
			_inscatterEncoded = new Texture3D(num, height, num2, (TextureFormat)5, false);
			((Texture)_inscatterEncoded).wrapMode = (TextureWrapMode)1;
			((Texture)_inscatterEncoded).filterMode = (FilterMode)1;
			_inscatterEncoded.SetPixels(array);
			_inscatterEncoded.Apply();
		}
		else
		{
			Debug.LogError((object)"Atmosphere::ConvertLookUpTable(): Couldn't load 3D LUT texture. Texture3D will be disabled.");
		}
	}

	private void InitializeShaders()
	{
		Shader.SetGlobalFloat("_SkyFade", 0f);
		Shader.SetGlobalFloat("_StormFade", 0f);
	}

	public void InitializeMaterial(Material material)
	{
		if (Object.op_Implicit((Object)(object)material))
		{
			material.SetTexture("_Transmittance", (Texture)(object)_transmittance);
			material.SetTexture("_Inscatter", (Texture)(object)_inscatterEncoded);
			material.SetTexture("_Irradiance", (Texture)(object)_irradiance);
			material.SetFloat("_MieG", 0.8f);
			material.SetFloat("Rg", 6360000f);
			material.SetFloat("Rt", 6420000f);
			material.SetFloat("Rl", 6421000f);
			material.SetFloat("RES_R", 16f);
			material.SetFloat("RES_MU", 16f);
			material.SetFloat("RES_MU_S", 16f);
			material.SetFloat("RES_NU", 1f);
		}
	}

	private void Update()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		ClampSkyPosition();
		AdvanceTimeOfDay();
		CalculateAmbientColor();
		CalculateSunAndMoonPosition();
		InitializeMaterial(_skydomeMaterial);
		Color color = _sunIlluminary.color;
		Shader.SetGlobalColor("_SunColor", color.linear);
		Shader.SetGlobalFloat("_SunIntensity", _sunIlluminary.intensity);
		Shader.SetGlobalVector("_SunPosition", Vector4.op_Implicit(((Component)_sunIlluminary).transform.position));
		Shader.SetGlobalVector("_SunDirection", Vector4.op_Implicit(((Component)_sunIlluminary).transform.forward * -1f));
		Shader.SetGlobalVector("_MoonDirection", Vector4.op_Implicit(((Component)_moonIlluminary).transform.forward * -1f));
	}

	private void AdvanceTimeOfDay()
	{
		if (Input.GetKey((KeyCode)270))
		{
			_timeOfDay += Time.deltaTime;
			if (_timeOfDay > 24f)
			{
				_timeOfDay = 0f;
			}
		}
		else if (Input.GetKey((KeyCode)269))
		{
			_timeOfDay -= Time.deltaTime;
			if (_timeOfDay < 0f)
			{
				_timeOfDay = 24f;
			}
		}
	}

	private void ClampSkyPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = _mainCameraTransform.position;
		position.y = 0f;
		_transform.position = position;
	}

	private void CalculateSunAndMoonPosition()
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Math.PI / 180f * _latitude;
		float num2 = Mathf.Sin(num);
		float num3 = Mathf.Cos(num);
		float num4 = (float)Math.PI / 180f * _longitude;
		float num5 = 0.4093f * Mathf.Sin((float)Math.PI / 184f * (_date - 81f));
		float num6 = Mathf.Sin(num5);
		float num7 = Mathf.Cos(num5);
		float num8 = (int)(_longitude / 15f);
		float num9 = (float)Math.PI / 12f * num8;
		float num10 = _timeOfDay + 0.17f * Mathf.Sin(0.03369f * (_date - 80f)) - 0.129f * Mathf.Sin((float)Math.PI * 2f / 355f * (_date - 8f)) + 12f / (float)Math.PI * (num9 - num4);
		float num11 = (float)Math.PI / 12f * num10;
		float num12 = Mathf.Sin(num11);
		float num13 = Mathf.Cos(num11);
		float num14 = Mathf.Asin(num2 * num6 - num3 * num7 * num13);
		float num15 = (0f - num7) * num12;
		float num16 = num3 * num6 - num2 * num7 * num13;
		float num17 = Mathf.Atan2(num15, num16);
		float num18 = (float)Math.PI / 2f - num14;
		float phi = num17;
		((Component)_sunIlluminary).transform.position = _transform.position + _transform.rotation * SphericalToCartesian(_sunOrbitRadius, num18, phi);
		((Component)_sunIlluminary).transform.LookAt(_transform.position);
		((Component)_moonIlluminary).transform.position = _transform.position + _transform.rotation * SphericalToCartesian(_sunOrbitRadius, num18 + (float)Math.PI, phi);
		((Component)_moonIlluminary).transform.LookAt(_transform.position);
		CalculateSunAndMoonColor(num18);
	}

	private void CalculateSunAndMoonColor(float theta)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		_altitude = (float)Math.PI / 2f - theta;
		float num = Mathf.Abs(_altitude);
		float num2 = (float)Math.PI / 180f;
		float num3 = (float)Math.PI / 15f;
		if (_altitude > 0f)
		{
			((Behaviour)_moonIlluminary).enabled = false;
			((Behaviour)_sunIlluminary).enabled = true;
			float num4 = Mathf.Lerp(0f, _sunLightIntensity, num / num2);
			_sunIlluminary.intensity = ((num < num2) ? num4 : _sunLightIntensity);
		}
		else
		{
			((Behaviour)_moonIlluminary).enabled = true;
			((Behaviour)_sunIlluminary).enabled = false;
			float num5 = Mathf.Lerp(0f, _moonLightIntensity, num / num3);
			_moonIlluminary.intensity = ((num < num3) ? num5 : _moonLightIntensity);
		}
		float num6 = 1f / 24f * _timeOfDay;
		Color color = _sunLightColorGradient.Evaluate(num6);
		Color color2 = _moonLightColorGradient.Evaluate(num6);
		_sunIlluminary.color = color;
		_moonIlluminary.color = color2;
		Color val = _ambientGroundColorGradient.Evaluate(num6);
		RenderSettings.ambientLight = _ambientLightColor;
		RenderSettings.ambientEquatorColor = Color.Lerp(val, _ambientLightColor, 0.6f);
		RenderSettings.ambientGroundColor = val;
		if (((Component)_environmentReflectionProbe).gameObject.activeSelf)
		{
			_environmentReflectionProbe.backgroundColor = val;
		}
	}

	private void CalculateAmbientColor()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)_timeOfDay;
		int num2 = num + 1;
		float num3 = _timeOfDay - (float)(int)_timeOfDay;
		Color pixel = _ambientLUT.GetPixel(num, 0);
		Color pixel2 = _ambientLUT.GetPixel(num2, 0);
		_ambientLightColor = Color.Lerp(pixel, pixel2, num3);
	}

	private Vector3 SphericalToCartesian(float radius, float theta, float phi)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Sin(theta);
		float num2 = Mathf.Cos(theta);
		float num3 = Mathf.Sin(phi);
		float num4 = Mathf.Cos(phi);
		Vector3 zero = Vector3.zero;
		zero.x = radius * num * num4;
		zero.y = radius * num2;
		zero.z = radius * num * num3;
		return zero;
	}
}
