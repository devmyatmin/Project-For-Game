using System;
using Beam;
using Beam.Events;
using Funlabs;
using UnityEngine;
using kode80.Clouds;

public class AtmosphereClouds : MonoBehaviour
{
	[SerializeField]
	private Shader _standard;

	[SerializeField]
	private Shader _volumetric;

	[SerializeField]
	private Renderer _cloudsRenderer;

	[SerializeField]
	private Gradient _cloudColor;

	[SerializeField]
	public Color _cloudBaseColorMultiplierBlendColor;

	[SerializeField]
	private float _cloudBaseColorMultiplier = 1f;

	public TimeOfDayKeyFrame[] keyFrames;

	[SerializeField]
	private Texture2D _overcastTexture;

	[SerializeField]
	private float _overcastTextureScale = 1f;

	[SerializeField]
	private float _overcastTextureSpeed = 0.2f;

	private Vector2 _overcastTextureDirection;

	private kode80Clouds _volumetricClouds;

	private bool _volumetricCloudsSupported;

	private void Start()
	{
		_volumetricClouds = Object.FindObjectOfType<kode80Clouds>();
		_volumetricCloudsSupported = SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 11");
		if (_volumetricCloudsSupported)
		{
			Singleton<Atmosphere>.Instance.SunsetEvent += Atmosphere_SunsetEvent;
		}
		else
		{
			((Component)_volumetricClouds).gameObject.SetActive(false);
		}
		LoadOptions();
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		SortKeyFrames();
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		if (_volumetricCloudsSupported && Object.op_Implicit((Object)(object)Singleton<Atmosphere>.Instance))
		{
			Singleton<Atmosphere>.Instance.SunsetEvent -= Atmosphere_SunsetEvent;
		}
	}

	private void EventManager_OptionsApplied(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	public void LoadOptions()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		switch ((!MPExtensions.IsLocalCoop(Game.Mode) && _volumetricCloudsSupported) ? Options.GraphicsSettings.Clouds : Quality_VolumetricClouds.OFF)
		{
		case Quality_VolumetricClouds.OFF:
			((Behaviour)_volumetricClouds).enabled = false;
			_cloudsRenderer.sharedMaterial.shader = _standard;
			break;
		case Quality_VolumetricClouds.LOW:
			((Behaviour)_volumetricClouds).enabled = true;
			_volumetricClouds.subPixelSize = kode80Clouds.SubPixelSize.Sub4x4;
			_volumetricClouds.maxIterations = 48;
			_cloudsRenderer.sharedMaterial.shader = _volumetric;
			break;
		case Quality_VolumetricClouds.MEDIUM:
			((Behaviour)_volumetricClouds).enabled = true;
			_volumetricClouds.subPixelSize = kode80Clouds.SubPixelSize.Sub4x4;
			_volumetricClouds.maxIterations = 64;
			_cloudsRenderer.sharedMaterial.shader = _volumetric;
			break;
		case Quality_VolumetricClouds.HIGH:
			((Behaviour)_volumetricClouds).enabled = true;
			_volumetricClouds.subPixelSize = kode80Clouds.SubPixelSize.Sub2x2;
			_volumetricClouds.maxIterations = 64;
			_cloudsRenderer.sharedMaterial.shader = _volumetric;
			break;
		}
	}

	private void Atmosphere_SunsetEvent()
	{
		_volumetricClouds.sunLight = (Singleton<Atmosphere>.Instance.DayTime ? Singleton<Atmosphere>.Instance.SunIlluminary.Light : Singleton<Atmosphere>.Instance.MoonIlluminary.Light);
	}

	private void Update()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Color val = _cloudColor.Evaluate(Singleton<GameTime>.Instance.NormalizedTime);
		_cloudsRenderer.sharedMaterial.SetColor("_CloudColor", val);
		UpdateVolumetricCloudsColors();
		UpdateVolumetricCloudsLighting();
		Vector2 val2 = Vector2.op_Implicit(Wind.Direction);
		val2 *= Time.timeSinceLevelLoad * _overcastTextureSpeed;
		Shader.SetGlobalVector("_OvercastTex_Coords", new Vector4(1f, 1f, val2.x, val2.y) * (1f / _overcastTextureScale));
		Shader.SetGlobalFloat("_Overcast", Mathf.Max(new float[1] { Mathf.Pow(Singleton<Atmosphere>.Instance.Altitude, 0.3f) }));
		Shader.SetGlobalTexture("_OvercastTex", (Texture)(object)(_overcastTexture ?? Texture2D.whiteTexture));
	}

	private void UpdateVolumetricCloudsColors()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (_volumetricCloudsSupported && ((Behaviour)_volumetricClouds).enabled)
		{
			_volumetricClouds.cloudTopColor = _cloudColor.Evaluate(Singleton<GameTime>.Instance.NormalizedTime);
			_volumetricClouds.cloudBaseColor = RenderSettings.ambientSkyColor * (_cloudBaseColorMultiplierBlendColor * _cloudBaseColorMultiplier);
		}
	}

	private void UpdateVolumetricCloudsLighting()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (_volumetricCloudsSupported && ((Behaviour)_volumetricClouds).enabled && keyFrames.Length >= 1)
		{
			float x = Singleton<Atmosphere>.Instance.SunIlluminary.Transform.localEulerAngles.x;
			TimeOfDayKeyFrame timeOfDayKeyFrame = KeyFrameBelowAngle(x);
			TimeOfDayKeyFrame timeOfDayKeyFrame2 = KeyFrameAboveAngle(x);
			float num = timeOfDayKeyFrame2.angle - timeOfDayKeyFrame.angle;
			float num2 = ((num == 0f) ? 0.5f : ((x - timeOfDayKeyFrame.angle) / num));
			_volumetricClouds.sunScalar = Mathf.Lerp(timeOfDayKeyFrame.SunScalar, timeOfDayKeyFrame2.SunScalar, num2);
			_volumetricClouds.ambientScalar = Mathf.Lerp(timeOfDayKeyFrame.AmbientScalar, timeOfDayKeyFrame2.AmbientScalar, num2);
		}
	}

	public void SortKeyFrames()
	{
		Array.Sort(keyFrames, (TimeOfDayKeyFrame a, TimeOfDayKeyFrame b) => a.angle.CompareTo(b.angle));
	}

	private TimeOfDayKeyFrame KeyFrameBelowAngle(float angle)
	{
		return KeyFrameForAngle(angle, findGreaterThan: false);
	}

	private TimeOfDayKeyFrame KeyFrameAboveAngle(float angle)
	{
		return KeyFrameForAngle(angle, findGreaterThan: true);
	}

	private TimeOfDayKeyFrame KeyFrameForAngle(float angle, bool findGreaterThan)
	{
		if (keyFrames.Length != 0)
		{
			TimeOfDayKeyFrame timeOfDayKeyFrame = keyFrames[0];
			TimeOfDayKeyFrame timeOfDayKeyFrame2 = keyFrames[keyFrames.Length - 1];
			if (angle <= timeOfDayKeyFrame.angle)
			{
				return timeOfDayKeyFrame;
			}
			if (angle >= timeOfDayKeyFrame2.angle)
			{
				return timeOfDayKeyFrame2;
			}
			for (int i = 0; i < keyFrames.Length; i++)
			{
				timeOfDayKeyFrame = keyFrames[i];
				timeOfDayKeyFrame2 = ((i < keyFrames.Length - 1) ? keyFrames[i + 1] : timeOfDayKeyFrame);
				if (angle >= timeOfDayKeyFrame.angle && angle <= timeOfDayKeyFrame2.angle)
				{
					if (!findGreaterThan)
					{
						return timeOfDayKeyFrame;
					}
					return timeOfDayKeyFrame2;
				}
			}
		}
		return null;
	}
}
