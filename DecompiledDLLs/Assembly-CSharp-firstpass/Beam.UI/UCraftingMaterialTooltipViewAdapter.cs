using System;
using UnityEngine;

namespace Beam.UI;

public class UCraftingMaterialTooltipViewAdapter : CraftingMaterialTooltipViewAdapterBase
{
	[SerializeField]
	private GameObjectViewAdapter _tooltipGameObject;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private TMPLabelViewAdapter _descriptionLabel;

	public override void Show()
	{
		ResetFade();
		_tooltipGameObject.Active = true;
		base.Show();
		LeanTween.cancel(((Component)this).gameObject);
		LeanTween.value(((Component)this).gameObject, (Action<float>)Fade_OnUpdate, _canvasGroup.Alpha, 1f, (1f - _canvasGroup.Alpha) * 0.2f).setDelay(0.7f).setIgnoreTimeScale(true);
	}

	public override void Hide()
	{
		LeanTween.cancel(((Component)this).gameObject);
		LeanTween.value(((Component)this).gameObject, (Action<float>)Fade_OnUpdate, _canvasGroup.Alpha, 0f, 0.2f).setOnComplete((Action)FadeOut_OnComplete).setIgnoreTimeScale(true);
	}

	public override void SetPosition(Vector2 screenPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)((Component)this).transform.parent;
		float num = val.sizeDelta.x / (float)Screen.width;
		float num2 = val.sizeDelta.y / (float)Screen.height;
		Vector2 anchoredPosition = new Vector2(screenPosition.x * num, screenPosition.y * num2);
		((RectTransform)((Component)this).transform).anchoredPosition = anchoredPosition;
	}

	public override void SetDescription(string text)
	{
		_descriptionLabel.Text = text;
	}

	private void Fade_OnUpdate(float value)
	{
		_canvasGroup.Alpha = value;
	}

	private void FadeOut_OnComplete()
	{
		ResetFade();
		_tooltipGameObject.Active = false;
		base.Hide();
	}

	private void ResetFade()
	{
		_canvasGroup.Alpha = 0f;
	}
}
