using System;
using Beam.Events;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

public class MainMenuEffectsController : MonoBehaviour, IMenuEffectsController
{
	[SerializeField]
	private Camera _camera;

	private BlurSettings _settings;

	private void OnEnable()
	{
		BeamPfxProfile profile = ((Component)_camera).GetComponent<BeamPfxLayer>().Profile;
		if ((Object)(object)profile == (Object)null || !profile.TryGetSettings<BlurSettings>(ref _settings))
		{
			((Behaviour)this).enabled = false;
		}
		else
		{
			EventManager.AddListener<MainMenuOpenedEvent>(EventManager_MainMenuOpened);
		}
	}

	private void OnDisable()
	{
		EventManager.RemoveListener<MainMenuOpenedEvent>(EventManager_MainMenuOpened);
	}

	public void SetMaskTexture(Texture2D texture)
	{
		((ParameterOverride<Texture>)(object)_settings.BlurMask).Override((Texture)(object)texture);
	}

	public void Show()
	{
		((ParameterOverride<bool>)(object)((PostProcessEffectSettings)_settings).enabled).Override(true);
		Fade_Callback(0f);
		LeanTween.cancel(((Component)this).gameObject);
		LeanTween.value(((Component)this).gameObject, 0f, 1f, 0.15f).setOnUpdate((Action<float>)Fade_Callback).setIgnoreTimeScale(true);
	}

	public void Hide()
	{
		LeanTween.cancel(((Component)this).gameObject);
		LeanTween.value(((Component)this).gameObject, 1f, 0f, 0.15f).setOnUpdate((Action<float>)Fade_Callback).setOnComplete((Action)(() =>
		{
			((ParameterOverride<bool>)(object)((PostProcessEffectSettings)_settings).enabled).Override(false);
		}))
			.setIgnoreTimeScale(true);
	}

	private void EventManager_MainMenuOpened(MainMenuOpenedEvent e)
	{
		if (e.Opened)
		{
			SetMaskTexture(Texture2D.whiteTexture);
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void Fade_Callback(float value)
	{
		((ParameterOverride<float>)(object)_settings.Alpha).Override(value);
	}
}
