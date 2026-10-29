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
		LeanTween.cancel(base.gameObject);
		LeanTween.value(base.gameObject, Fade_OnUpdate, _canvasGroup.Alpha, 1f, (1f - _canvasGroup.Alpha) * 0.2f).setDelay(0.7f).setIgnoreTimeScale(useUnScaledTime: true);
	}

	public override void Hide()
	{
		LeanTween.cancel(base.gameObject);
		LeanTween.value(base.gameObject, Fade_OnUpdate, _canvasGroup.Alpha, 0f, 0.2f).setOnComplete(FadeOut_OnComplete).setIgnoreTimeScale(useUnScaledTime: true);
	}

	public override void SetPosition(Vector2 screenPosition)
	{
		RectTransform obj = (RectTransform)base.transform.parent;
		float num = obj.sizeDelta.x / (float)Screen.width;
		float num2 = obj.sizeDelta.y / (float)Screen.height;
		Vector2 anchoredPosition = new Vector2(screenPosition.x * num, screenPosition.y * num2);
		((RectTransform)base.transform).anchoredPosition = anchoredPosition;
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
