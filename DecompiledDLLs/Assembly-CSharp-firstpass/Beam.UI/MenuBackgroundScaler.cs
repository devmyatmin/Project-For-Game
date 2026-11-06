using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RawImage))]
public class MenuBackgroundScaler : MonoBehaviour
{
	[SerializeField]
	private float _width;

	[SerializeField]
	private float _height;

	[SerializeField]
	private RectTransform _fillRect;

	[SerializeField]
	private RectTransform _fadeRect;

	[SerializeField]
	private float _maxFadeWidth;

	private Canvas _canvas;

	private RawImage _image;

	private void OnEnable()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_image == (Object)null)
		{
			_image = ((Component)this).GetComponent<RawImage>();
		}
		((Graphic)_image).rectTransform.anchorMin = new Vector2(0f, 0.5f);
		((Graphic)_image).rectTransform.anchorMax = new Vector2(0f, 0.5f);
		((Graphic)_image).rectTransform.pivot = new Vector2(0f, 0.5f);
	}

	private void Update()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_canvas == (Object)null)
		{
			_canvas = ((Component)this).GetComponentInParent<Canvas>();
		}
		Vector2 sizeDelta = ((RectTransform)((Component)_canvas).transform).sizeDelta;
		if (sizeDelta.x / sizeDelta.y < 1.7777778f)
		{
			((Graphic)_image).rectTransform.sizeDelta = Vector2.op_Implicit(new Vector3(sizeDelta.y * 1.7777778f, sizeDelta.y, 1f));
		}
		else
		{
			((Graphic)_image).rectTransform.sizeDelta = Vector2.op_Implicit(new Vector3(1920f, sizeDelta.y, 1f));
			_fillRect.sizeDelta = new Vector2(sizeDelta.x - ((Graphic)_image).rectTransform.sizeDelta.x, _fillRect.sizeDelta.y);
			RectTransform fadeRect = _fadeRect;
			Rect rect = _fillRect.rect;
			fadeRect.sizeDelta = new Vector2(Mathf.Clamp(rect.width * 4f, 0f, _maxFadeWidth), _fadeRect.sizeDelta.y);
		}
		((Graphic)_image).rectTransform.anchoredPosition = Vector2.zero;
	}
}
