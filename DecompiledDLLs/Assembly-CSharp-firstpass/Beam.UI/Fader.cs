using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class Fader : Singleton<Fader>
{
	[SerializeField]
	private FaderView _view;

	private bool _isFading;

	private CoroutineHandle _fadeInSequence;

	private CoroutineHandle _fadeOutSequence;

	private EasingType _ease = EasingType.Cubic;

	public bool IsFading => _isFading;

	public float CurrentValue => _view.CanvasGroup.Alpha;

	private void Awake()
	{
		ResetColor();
	}

	public void SetCustomTexture(Texture2D texture)
	{
		_view.CustomTexture.Texture = (Texture)(object)texture;
	}

	public void SetFadeColor(Color color)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_view.Image.Color = color;
	}

	public void SetFadeEasing(EasingType ease)
	{
		_ease = ease;
	}

	public void ResetColor()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		SetFadeColor(new Color(0.11f, 0.13f, 0.15f));
	}

	public void ResetEase()
	{
		_ease = EasingType.Cubic;
	}

	public void ResetFade(FadeMode mode, FadeDirection fade)
	{
		ResetFade(mode, (fade == FadeDirection.In) ? 1f : 0f);
	}

	public void ResetFade(FadeMode mode, float value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_fadeInSequence);
		Timing.KillCoroutines(_fadeOutSequence);
		bool flag = mode == FadeMode.Custom;
		_view.CustomTexture.Enabled = flag;
		((Component)_view.CustomTexture.transform).gameObject.SetActive(flag);
		bool flag2 = mode == FadeMode.Solid;
		_view.Image.Enabled = flag2;
		((Component)_view.Image.transform).gameObject.SetActive(flag2);
		_view.CanvasGroup.Alpha = value;
		_isFading = false;
	}

	public float FadeIn_Async(float time, FadeMode mode = FadeMode.Solid, Action callback = null)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(FadeIn(time, mode, callback));
	}

	public CoroutineHandle FadeIn(float time, FadeMode mode = FadeMode.Solid, Action callback = null)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		ResetFade(mode, 1f);
		_fadeInSequence = Timing.RunCoroutine(FadeIn_Sequence(time, callback));
		return _fadeInSequence;
	}

	public CoroutineHandle FadeIn(float time, float from, FadeMode mode = FadeMode.Solid, Action callback = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		ResetFade(mode, from);
		_fadeInSequence = Timing.RunCoroutine(FadeIn_Sequence(time, callback));
		return _fadeInSequence;
	}

	private IEnumerator<float> FadeIn_Sequence(float time, Action callback = null)
	{
		_isFading = true;
		_view.Show();
		if (time == 0f)
		{
			_view.CanvasGroup.Alpha = 0f;
			callback?.Invoke();
			yield break;
		}
		float from = _view.CanvasGroup.Alpha;
		float rate = from / time;
		float timer = 0f;
		while (timer < time)
		{
			timer += Time.unscaledDeltaTime;
			float alpha = Mathf.Max(0f, from - Easing.EaseIn(timer * rate, _ease));
			_view.CanvasGroup.Alpha = alpha;
			yield return 0f;
		}
		callback?.Invoke();
		_view.Hide();
		_isFading = false;
	}

	public float FadeOut_Async(float time, FadeMode mode = FadeMode.Solid, Action callback = null)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(FadeOut(time, mode, callback));
	}

	public float FadeOut_Async(float time, float from, FadeMode mode = FadeMode.Solid, Action callback = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(FadeOut(time, from, mode, callback));
	}

	public CoroutineHandle FadeOut(float time, FadeMode mode = FadeMode.Solid, Action callback = null)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		ResetFade(mode, 0f);
		_fadeOutSequence = Timing.RunCoroutine(FadeOut_Sequence(time, callback));
		return _fadeOutSequence;
	}

	public CoroutineHandle FadeOut(float time, float from, FadeMode mode = FadeMode.Solid, Action callback = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		ResetFade(mode, from);
		_fadeOutSequence = Timing.RunCoroutine(FadeOut_Sequence(time, callback));
		return _fadeOutSequence;
	}

	private IEnumerator<float> FadeOut_Sequence(float time, Action callback = null)
	{
		_isFading = true;
		_view.Show();
		if (time == 0f)
		{
			_view.CanvasGroup.Alpha = 1f;
			callback?.Invoke();
			yield break;
		}
		float from = _view.CanvasGroup.Alpha;
		float rate = Mathf.Clamp01(1f - from) / time;
		float timer = 0f;
		while (timer < time)
		{
			timer += Time.unscaledDeltaTime;
			float alpha = Mathf.Min(1f, from + Easing.EaseOut(timer * rate, _ease));
			_view.CanvasGroup.Alpha = alpha;
			yield return 0f;
		}
		callback?.Invoke();
		_isFading = false;
	}
}
