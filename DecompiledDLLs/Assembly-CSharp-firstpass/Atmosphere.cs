using System;
using Beam;
using Beam.Utilities;
using SharpNeatLib.Maths;
using UnityEngine;
using UnityEngine.Rendering;

public class Atmosphere : Singleton<Atmosphere>
{
	[Serializable]
	public class Illuminary
	{
		[SerializeField]
		private GameObject _gameObject;

		private Transform _transform;

		public Light Light;

		public Renderer Renderer;

		[Range(0f, 8f)]
		[SerializeField]
		private float _intensity;

		private float _currentIntensity;

		[SerializeField]
		private Gradient _gradient;

		public GameObject GameObject => _gameObject;

		public Transform Transform
		{
			get
			{
				if (!Object.op_Implicit((Object)(object)_transform))
				{
					_transform = _gameObject.transform;
				}
				return _transform;
			}
		}

		public float Intensity => _intensity;

		public float CurrentIntensity
		{
			get
			{
				return _currentIntensity;
			}
			set
			{
				_currentIntensity = value;
			}
		}

		public Gradient Gradient => _gradient;

		public void Enable()
		{
			((Behaviour)Light).enabled = true;
			if ((Object)(object)Renderer != (Object)null)
			{
				Renderer.enabled = true;
			}
		}

		public void Disable()
		{
			((Behaviour)Light).enabled = false;
			if ((Object)(object)Renderer != (Object)null)
			{
				Renderer.enabled = false;
			}
		}
	}

	[SerializeField]
	private Illuminary _sunIlluminary;

	[SerializeField]
	private Illuminary _moonIlluminary;

	[SerializeField]
	private MeshRenderer _skyDome;

	private bool _dayTime;

	private float _latitude;

	private float _longitude;

	private float _date = 180f;

	private float _theta;

	private float _sunOrbitRadius;

	private float _altitude;

	[SerializeField]
	private Gradient _skyColorPolariser;

	[SerializeField]
	private Gradient _ambientSkyColorGradient;

	[SerializeField]
	private Gradient _ambientGroundColorGradient;

	[SerializeField]
	private Gradient _ambientUnderwaterGroundColorGradient;

	[SerializeField]
	[HideInInspector]
	private Texture2D _transmittance;

	[SerializeField]
	[HideInInspector]
	private Texture2D _inscatter;

	private Texture3D _inscatterEncoded;

	[SerializeField]
	[HideInInspector]
	private Texture2D _irradiance;

	[SerializeField]
	private RenderTexture _atmosphereLookupTexture;

	private Material _atmosphereLookupMaterial;

	private Color _ambientLightColor;

	private Color _ambientGroundColor;

	[Range(0f, 1f)]
	[SerializeField]
	private float _mieG = 0.85f;

	private float _lastUpdatedTime = -1f;

	[Range(0f, 0.1f)]
	[SerializeField]
	public float MilitaryTimeDiffToUpdate = 0.005f;

	public Illuminary SunIlluminary => _sunIlluminary;

	public Illuminary MoonIlluminary => _moonIlluminary;

	public Illuminary CurrentIlluminary
	{
		get
		{
			if (!_dayTime)
			{
				return _moonIlluminary;
			}
			return _sunIlluminary;
		}
	}

	public bool DayTime => _dayTime;

	public float Altitude => _altitude;

	public Gradient AmbientGroundColorGradient => _ambientGroundColorGradient;

	public Gradient AmbientUnderwaterGroundColorGradient => _ambientUnderwaterGroundColorGradient;

	public Texture2D Transmittance => _transmittance;

	public Texture2D Inscatter => _inscatter;

	public Texture2D Irradiance => _irradiance;

	public event EmptyEventHandler SunsetEvent;

	private bool CheckUpdate()
	{
		if (Options.GraphicsSettings.ShadowQuality > Quality_Shadows.MEDIUM)
		{
			return true;
		}
		float militaryTime = Singleton<GameTime>.Instance.MilitaryTime;
		if ((double)_lastUpdatedTime < 0.0)
		{
			_lastUpdatedTime = militaryTime;
			return true;
		}
		if (Mathf.PingPong(militaryTime - _lastUpdatedTime, 24f) >= MilitaryTimeDiffToUpdate)
		{
			_lastUpdatedTime = militaryTime;
			return true;
		}
		return false;
	}

	protected virtual void OnSunsetEvent()
	{
		SunsetEvent?.Invoke();
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_sunOrbitRadius = ((Component)_skyDome).transform.localScale.x * 0.99f;
		_inscatterEncoded = ConvertLookUpTable(_inscatter);
		_atmosphereLookupTexture = CreateBRDFTexture();
		_atmosphereLookupMaterial = CreateBRDFMaterial();
		SetStaticShaderUniforms();
		SetMaterialUniforms(((Renderer)_skyDome).sharedMaterial);
		RenderSettings.ambientMode = (AmbientMode)1;
		RenderSettings.ambientIntensity = 1f;
	}

	public RenderTexture CreateBRDFTexture()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_0019: Expected Obj, but got Unknown
		RenderTexture val = new RenderTexture(512, 512, 0, (RenderTextureFormat)2);
		RenderTexture val2 = val;
		_atmosphereLookupTexture = val;
		((Texture)val2).filterMode = (FilterMode)1;
		val2.useMipMap = true;
		val2.autoGenerateMips = true;
		return val2;
	}

	public Material CreateBRDFMaterial()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		Shader val = Shader.Find("Beam Team/Atmosphere/Atmosphere BRDF");
		if ((Object)(object)val != (Object)null)
		{
			Material val2 = new Material(val);
			SetMaterialUniforms(val2);
			return val2;
		}
		Debug.LogError((object)"Atmosphere::Start: Atmosphere BRDF Shader could not be found.");
		return null;
	}

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreCull = (CameraCallback)Delegate.Combine((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(Camera_OnPreCull));
	}

	protected override void OnDestroy()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		BeamTeam.Destroy<Material>(_atmosphereLookupMaterial);
		BeamTeam.Destroy<RenderTexture>(_atmosphereLookupTexture);
		BeamTeam.Destroy<Texture3D>(_inscatterEncoded);
		Camera.onPreCull = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(Camera_OnPreCull));
		base.OnDestroy();
	}

	private void Camera_OnPreCull(Camera camera)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)camera == (Object)null) && (((Component)camera).CompareTag("MainCamera") || ((Component)camera).CompareTag("PlayerCamera")))
		{
			ClampSkyPosition(((Component)camera).transform.position);
			CalculateIlluminaryColors(camera);
			CalculateAmbientColor(camera);
		}
	}

	public static Texture3D ConvertLookUpTable(Texture2D texture)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected Obj, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)texture))
		{
			if (((Texture)texture).height != Mathf.FloorToInt(Mathf.Sqrt((float)((Texture)texture).width)))
			{
				Debug.LogWarning((object)("Atmosphere::ConvertLookUpTable: The given 2D texture " + ((Object)texture).name + " cannot be used as a 3D LUT."));
				return null;
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
			Texture3D val = new Texture3D(num, height, num2, (TextureFormat)5, false)
			{
				wrapMode = (TextureWrapMode)1,
				filterMode = (FilterMode)1
			};
			val.SetPixels(array);
			val.Apply();
			return val;
		}
		Debug.LogError((object)"Atmosphere::ConvertLookUpTable(): Couldn't load 3D LUT texture. Texture3D will be disabled.");
		return null;
	}

	private void SetStaticShaderUniforms()
	{
		float num = (float)(new FastRandom(StrandedWorld.WORLD_SEED).NextDouble() * 1000.0);
		Shader.SetGlobalFloat("_RandomSeed", num);
		Shader.SetGlobalFloat("_SkyFade", 0f);
		Shader.SetGlobalFloat("_StormFade", 0f);
	}

	private void SetDynamicShaderUniforms()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		Color color = _sunIlluminary.Light.color;
		Shader.SetGlobalColor("_SunColor", color.linear);
		Shader.SetGlobalFloat("_SunIntensity", _sunIlluminary.Light.intensity);
		Shader.SetGlobalVector("_SunPosition", Vector4.op_Implicit(_sunIlluminary.Transform.position));
		Shader.SetGlobalVector("_SunDirection", Vector4.op_Implicit(_sunIlluminary.Transform.forward * -1f));
		Shader.SetGlobalVector("_MoonDirection", Vector4.op_Implicit(_moonIlluminary.Transform.forward * -1f));
	}

	public void SetMaterialUniforms(Material material)
	{
		if (!((Object)(object)material == (Object)null))
		{
			material.SetTexture("_Transmittance", (Texture)(object)_transmittance);
			material.SetTexture("_Inscatter", (Texture)(object)_inscatterEncoded);
			material.SetTexture("_Irradiance", (Texture)(object)_irradiance);
			material.SetFloat("_MieG", _mieG);
			material.SetFloat("Rg", 6360000f);
			material.SetFloat("Rt", 6420000f);
			material.SetFloat("Rl", 6421000f);
			material.SetFloat("RES_R", 16f);
			material.SetFloat("RES_MU", 16f);
			material.SetFloat("RES_MU_S", 16f);
			material.SetFloat("RES_NU", 1f);
		}
	}

	private void ClampSkyPosition(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position2 = position;
		position2.y = 0f;
		((Component)this).transform.position = position2;
	}

	private void Update()
	{
		if (CheckUpdate())
		{
			CalculateIlluminaryPositions();
			SetDynamicShaderUniforms();
			SetMaterialUniforms(((Renderer)_skyDome).sharedMaterial);
			SetMaterialUniforms(_atmosphereLookupMaterial);
			ComputeAtmosphereLookupTexture(_atmosphereLookupMaterial, Singleton<GameTime>.Instance.NormalizedTime);
		}
	}

	public void ComputeAtmosphereLookupTexture(Material lookupMaterial, float normalizedTime)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		lookupMaterial.SetColor("_PolarisingColor", _skyColorPolariser.Evaluate(normalizedTime));
		Graphics.Blit((Texture)null, _atmosphereLookupTexture, lookupMaterial);
		Shader.SetGlobalTexture("_SkyMap", (Texture)(object)_atmosphereLookupTexture);
	}

	private void CalculateIlluminaryPositions()
	{
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Math.PI / 180f * _latitude;
		float num2 = Mathf.Sin(num);
		float num3 = Mathf.Cos(num);
		float num4 = (float)Math.PI / 180f * _longitude;
		float num5 = 0.4093f * Mathf.Sin((float)Math.PI / 184f * (_date - 81f));
		float num6 = Mathf.Sin(num5);
		float num7 = Mathf.Cos(num5);
		float num8 = (int)(_longitude / 15f);
		float num9 = (float)Math.PI / 12f * num8;
		float num10 = Singleton<GameTime>.Instance.MilitaryTime + 0.17f * Mathf.Sin(0.03369f * (_date - 80f)) - 0.129f * Mathf.Sin((float)Math.PI * 2f / 355f * (_date - 8f)) + 12f / (float)Math.PI * (num9 - num4);
		float num11 = (float)Math.PI / 12f * num10;
		float num12 = Mathf.Sin(num11);
		float num13 = Mathf.Cos(num11);
		float num14 = Mathf.Asin(num2 * num6 - num3 * num7 * num13);
		float num15 = (0f - num7) * num12;
		float num16 = num3 * num6 - num2 * num7 * num13;
		float num17 = Mathf.Atan2(num15, num16);
		_theta = (float)Math.PI / 2f - num14;
		float phi = num17;
		_sunIlluminary.Transform.position = ((Component)this).transform.position + ((Component)this).transform.rotation * SphericalToCartesian(_sunOrbitRadius, _theta, phi);
		_sunIlluminary.Transform.LookAt(((Component)this).transform.position);
		_moonIlluminary.Transform.position = ((Component)this).transform.position + ((Component)this).transform.rotation * SphericalToCartesian(_sunOrbitRadius, _theta + (float)Math.PI, phi);
		_moonIlluminary.Transform.LookAt(((Component)this).transform.position);
		_altitude = (float)Math.PI / 2f - _theta;
		float num18 = Mathf.Abs(_altitude);
		float num19 = (float)Math.PI / 180f;
		float num20 = (float)Math.PI / 15f;
		if (_altitude > 0f)
		{
			_sunIlluminary.Enable();
			_moonIlluminary.Disable();
			float num21 = Mathf.Lerp(0f, _sunIlluminary.Intensity, num18 / num19);
			_sunIlluminary.CurrentIntensity = ((num18 < num19) ? num21 : _sunIlluminary.Intensity);
			_sunIlluminary.Light.intensity = _sunIlluminary.CurrentIntensity;
			if (!_dayTime)
			{
				_dayTime = true;
				OnSunsetEvent();
			}
		}
		else
		{
			_moonIlluminary.Enable();
			_sunIlluminary.Disable();
			float num22 = Mathf.Lerp(0f, _moonIlluminary.Intensity, num18 / num20);
			_moonIlluminary.CurrentIntensity = ((num18 < num20) ? num22 : _moonIlluminary.Intensity);
			_moonIlluminary.Light.intensity = _moonIlluminary.CurrentIntensity;
			if (_dayTime)
			{
				_dayTime = false;
				OnSunsetEvent();
			}
		}
	}

	private void CalculateIlluminaryColors(Camera camera)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Cameras.IsUnderwater(((Component)camera).transform);
		float normalizedTime = Singleton<GameTime>.Instance.NormalizedTime;
		Color val = _sunIlluminary.Gradient.Evaluate(normalizedTime);
		Color val2 = _moonIlluminary.Gradient.Evaluate(normalizedTime);
		WeatherEvent weatherEvent = Singleton<AtmosphereStorm>.Instance?.CurrentWeatherEvent;
		if ((Object)(object)weatherEvent != (Object)null && weatherEvent.HasStarted)
		{
			val = weatherEvent.GetSunColor(val);
			val2 = weatherEvent.GetMoonColor(val2);
		}
		_sunIlluminary.Light.color = val;
		_moonIlluminary.Light.color = val2;
		if (flag)
		{
			float num = Singleton<CetoSettings>.Instance.BelowVisibility * 1.5f;
			float num2 = Mathf.Clamp01((((Component)camera).transform.position.y + num) / num);
			CurrentIlluminary.Light.color = Color.Lerp(CurrentIlluminary.Light.color * new Color(0.5f, 0.9f, 1f), CurrentIlluminary.Light.color, num2);
			CurrentIlluminary.Light.intensity = CurrentIlluminary.CurrentIntensity * num2;
		}
	}

	private void CalculateAmbientColor(Camera camera)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Cameras.IsUnderwater(((Component)camera).transform);
		float normalizedTime = Singleton<GameTime>.Instance.NormalizedTime;
		_ambientLightColor = _ambientSkyColorGradient.Evaluate(normalizedTime) * _skyColorPolariser.Evaluate(normalizedTime);
		_ambientGroundColor = (flag ? _ambientUnderwaterGroundColorGradient.Evaluate(normalizedTime) : _ambientGroundColorGradient.Evaluate(normalizedTime));
		WeatherEvent weatherEvent = Singleton<AtmosphereStorm>.Instance?.CurrentWeatherEvent;
		if ((Object)(object)weatherEvent != (Object)null && weatherEvent.HasStarted)
		{
			_ambientLightColor = weatherEvent.GetAmbientSkyColor(_ambientLightColor);
			_ambientGroundColor = weatherEvent.GetAmbientGroundColor(_ambientGroundColor);
		}
		if (flag)
		{
			float num = Singleton<CetoSettings>.Instance.BelowVisibility * 1.5f;
			float num2 = Mathf.Clamp01((((Component)camera).transform.position.y + num) / num);
			_ambientLightColor *= num2;
			_ambientGroundColor *= num2;
		}
		RenderSettings.sun = CurrentIlluminary.Light;
		RenderSettings.ambientLight = _ambientLightColor;
		RenderSettings.ambientEquatorColor = Color.Lerp(_ambientGroundColor, _ambientLightColor, 0.6f);
		RenderSettings.ambientGroundColor = _ambientGroundColor;
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
