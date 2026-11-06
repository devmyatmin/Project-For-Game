using Beam;
using Beam.Events;
using Beam.PostEffects;
using Funlabs;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostEffectsManager : MonoBehaviour, IIdleUntilInitialized
{
	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private BeamPfxLayer _pfxLayer;

	private SSAOPro _ambientOcclusionEffect;

	private bool _initialized;

	public bool IsInitialized => _initialized;

	public void Initialize()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			_initialized = true;
			_camera.depthTextureMode = (DepthTextureMode)1;
			Camera camera = _camera;
			camera.depthTextureMode = (DepthTextureMode)(camera.depthTextureMode | 2);
			GetPostEffects();
			SetDefaultQualitySettings();
			LoadOptions();
			EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		}
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
	}

	private void EventManager_OptionsApplied(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void GetPostEffects()
	{
		_pfxLayer = ((Component)_camera).GetComponent<BeamPfxLayer>();
		if (!Object.op_Implicit((Object)(object)_pfxLayer))
		{
			Debug.LogError((object)"PostEffectsManager::GetPostEffects:: No PostProcessLayer component found.", (Object)(object)this);
		}
		_ambientOcclusionEffect = ((Component)_camera).GetComponent<SSAOPro>();
		if (!Object.op_Implicit((Object)(object)_ambientOcclusionEffect))
		{
			Debug.LogError((object)"PostEffectsManager::GetPostEffects:: ", (Object)(object)this);
		}
	}

	private void SetDefaultQualitySettings()
	{
		QualitySettings.anisotropicFiltering = (AnisotropicFiltering)1;
		QualitySettings.antiAliasing = 0;
		QualitySettings.realtimeReflectionProbes = true;
		QualitySettings.billboardsFaceCameraPosition = true;
		QualitySettings.skinWeights = (SkinWeights)2;
		QualitySettings.lodBias = 2f;
		QualitySettings.maximumLODLevel = 0;
		QualitySettings.particleRaycastBudget = 1024;
	}

	public void LoadOptions()
	{
		if (!LevelLoader.IsLoading())
		{
			GraphicsUtilities.SetFixedFrameRate(Options.GraphicsSettings.FixedFrameRate);
			GraphicsUtilities.SetVSync(Options.GraphicsSettings.VerticalSync);
		}
		SetHighlighting(Options.GeneralSettings.ShowHighlighting);
		SetColorProfile(Options.GraphicsSettings.ColorMode);
		SetAmbientOcclusion(Options.GraphicsSettings.AmbientOcclusion);
		SetDrawDistance(Options.GraphicsSettings.DrawDistance);
		SetTextureQuality(Options.GraphicsSettings.TextureQuality);
		SetShadowQuality(Options.GraphicsSettings.ShadowQuality);
		SetShaderQuality(Options.GraphicsSettings.ShaderQuality);
		SetAntialiasing(Options.GraphicsSettings.Antialiasing);
		SetMotionBlur(Options.GraphicsSettings.MotionBlur);
		SetBloom(Options.GraphicsSettings.Bloom);
		SetLensEffects(Options.GraphicsSettings.LensEffects);
		SetCaustics(Options.GraphicsSettings.Caustics);
	}

	private void SetHighlighting(Quality_OFF_ON enabled)
	{
		HighlighterSettings highlighterSettings = default;
		if (!_pfxLayer.Profile.TryGetSettings<HighlighterSettings>(ref highlighterSettings))
		{
			Debug.LogError((object)"PostEffectsManager::SetHighlighting:: No Highlighting effect found!");
		}
		else
		{
			((ParameterOverride<bool>)(object)((PostProcessEffectSettings)highlighterSettings).enabled).Override(enabled == Quality_OFF_ON.ON);
		}
	}

	private void SetColorProfile(Quality_ColorMode colorMode)
	{
	}

	private void SetDrawDistance(Quality_DrawDistance drawDistance)
	{
	}

	private void SetTextureQuality(Quality_Textures textureQuality)
	{
		if (textureQuality == Quality_Textures.LOW)
		{
			QualitySettings.masterTextureLimit = 1;
		}
		else
		{
			QualitySettings.masterTextureLimit = 0;
		}
	}

	private void SetShadowQuality(Quality_Shadows shadowQuality)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		switch (shadowQuality)
		{
		case Quality_Shadows.OFF:
			Shader.EnableKeyword("BEAM_TRANSLUCENCY_OFF");
			Shader.DisableKeyword("BEAM_TRANSLUCENCY_ON");
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowDistance = 0f;
			QualitySettings.shadowProjection = (ShadowProjection)0;
			QualitySettings.shadowResolution = (ShadowResolution)0;
			break;
		case Quality_Shadows.LOW:
			Shader.EnableKeyword("BEAM_TRANSLUCENCY_ON");
			Shader.DisableKeyword("BEAM_TRANSLUCENCY_OFF");
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 300f;
			QualitySettings.shadowProjection = (ShadowProjection)0;
			QualitySettings.shadowResolution = (ShadowResolution)0;
			break;
		case Quality_Shadows.MEDIUM:
			Shader.EnableKeyword("BEAM_TRANSLUCENCY_ON");
			Shader.DisableKeyword("BEAM_TRANSLUCENCY_OFF");
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 300f;
			QualitySettings.shadowCascade2Split = 0.2f;
			QualitySettings.shadowProjection = (ShadowProjection)0;
			QualitySettings.shadowResolution = (ShadowResolution)1;
			break;
		case Quality_Shadows.HIGH:
			Shader.EnableKeyword("BEAM_TRANSLUCENCY_ON");
			Shader.DisableKeyword("BEAM_TRANSLUCENCY_OFF");
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 300f;
			QualitySettings.shadowCascade4Split = new Vector3(0.05f, 0.12f, 0.37f);
			QualitySettings.shadowProjection = (ShadowProjection)1;
			QualitySettings.shadowResolution = (ShadowResolution)2;
			break;
		case Quality_Shadows.ULTRA:
			Shader.EnableKeyword("BEAM_TRANSLUCENCY_ON");
			Shader.DisableKeyword("BEAM_TRANSLUCENCY_OFF");
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 300f;
			QualitySettings.shadowCascade4Split = new Vector3(0.035f, 0.07f, 0.17f);
			QualitySettings.shadowProjection = (ShadowProjection)1;
			QualitySettings.shadowResolution = (ShadowResolution)3;
			break;
		}
	}

	private void SetShaderQuality(Quality_LOW_ULTRA shaders)
	{
		if (Options.GraphicsSettings.Preset == Quality_Preset.LAPTOP)
		{
			Shader.EnableKeyword("SHADER_QUALITY_LAPTOP");
			Shader.DisableKeyword("SHADER_QUALITY_LOW");
			Shader.DisableKeyword("SHADER_QUALITY_MEDIUM");
			Shader.DisableKeyword("SHADER_QUALITY_HIGH");
			Shader.DisableKeyword("SHADER_QUALITY_ULTRA");
		}
		else
		{
			Shader.EnableKeyword("SHADER_QUALITY_HIGH");
			Shader.DisableKeyword("SHADER_QUALITY_LAPTOP");
			Shader.DisableKeyword("SHADER_QUALITY_LOW");
			Shader.DisableKeyword("SHADER_QUALITY_MEDIUM");
			Shader.DisableKeyword("SHADER_QUALITY_ULTRA");
		}
	}

	private void SetAntialiasing(Quality_Antialiasing antialiasing)
	{
		if (!((Object)(object)_pfxLayer == (Object)null))
		{
			BeamPfxLayer component = ((Component)Cameras.GetPostCamera()).GetComponent<BeamPfxLayer>();
			switch (antialiasing)
			{
			case Quality_Antialiasing.FXAA:
				component.AntialiasingMode = (Antialiasing)1;
				_pfxLayer.AntialiasingMode = (Antialiasing)0;
				break;
			case Quality_Antialiasing.SMAA:
				component.AntialiasingMode = (Antialiasing)2;
				_pfxLayer.AntialiasingMode = (Antialiasing)0;
				break;
			case Quality_Antialiasing.TXAA:
				component.AntialiasingMode = (Antialiasing)0;
				_pfxLayer.AntialiasingMode = (Antialiasing)3;
				break;
			case Quality_Antialiasing.OFF:
				component.AntialiasingMode = (Antialiasing)0;
				_pfxLayer.AntialiasingMode = (Antialiasing)0;
				break;
			}
		}
	}

	private void SetAmbientOcclusion(Quality_AmbientOcclusion ambientOcclusion)
	{
		if (Object.op_Implicit((Object)(object)_ambientOcclusionEffect))
		{
			switch (ambientOcclusion)
			{
			case Quality_AmbientOcclusion.LOW:
				((Behaviour)_ambientOcclusionEffect).enabled = true;
				_ambientOcclusionEffect.Downsampling = 2;
				_ambientOcclusionEffect.Samples = SSAOPro.SampleCount.VeryLow;
				_ambientOcclusionEffect.Blur = SSAOPro.BlurMode.Gaussian;
				break;
			case Quality_AmbientOcclusion.MEDIUM:
				((Behaviour)_ambientOcclusionEffect).enabled = true;
				_ambientOcclusionEffect.Downsampling = 1;
				_ambientOcclusionEffect.Samples = SSAOPro.SampleCount.Low;
				_ambientOcclusionEffect.Blur = SSAOPro.BlurMode.Gaussian;
				break;
			case Quality_AmbientOcclusion.HIGH:
				((Behaviour)_ambientOcclusionEffect).enabled = true;
				_ambientOcclusionEffect.Downsampling = 1;
				_ambientOcclusionEffect.Samples = SSAOPro.SampleCount.Medium;
				_ambientOcclusionEffect.Blur = SSAOPro.BlurMode.HighQualityBilateral;
				break;
			case Quality_AmbientOcclusion.OFF:
				((Behaviour)_ambientOcclusionEffect).enabled = false;
				break;
			}
		}
	}

	private void SetMotionBlur(Quality_MotionBlur motionBlur)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_pfxLayer == (Object)null)
		{
			return;
		}
		MotionBlur val = default;
		if (!_pfxLayer.Profile.TryGetSettings<MotionBlur>(ref val))
		{
			Debug.LogError((object)"PostEffectsManager::SetMotionBlur:: No Motion Blur effect found!");
			return;
		}
		if (MPExtensions.IsLocalCoop(Game.Mode))
		{
			((ParameterOverride<bool>)(object)((PostProcessEffectSettings)val).enabled).Override(false);
			return;
		}
		switch (motionBlur)
		{
		case Quality_MotionBlur.ON:
			((ParameterOverride<bool>)(object)((PostProcessEffectSettings)val).enabled).Override(true);
			break;
		case Quality_MotionBlur.OFF:
			((ParameterOverride<bool>)(object)((PostProcessEffectSettings)val).enabled).Override(false);
			break;
		}
	}

	private void SetBloom(Quality_Bloom bloom)
	{
		SENaturalBloomAndDirtyLens component = ((Component)Cameras.GetPostCamera()).GetComponent<SENaturalBloomAndDirtyLens>();
		if ((Object)(object)component != (Object)null)
		{
			((Behaviour)component).enabled = bloom == Quality_Bloom.ON;
		}
	}

	private void SetLensEffects(Quality_OFF_ON lensEffects)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)Game.State == 1)
		{
			lensEffects = Quality_OFF_ON.OFF;
		}
	}

	private void SetCaustics(Quality_Caustics caustics)
	{
		Singleton<CetoSettings>.Instance.SetCaustics(caustics == Quality_Caustics.ON);
	}
}
