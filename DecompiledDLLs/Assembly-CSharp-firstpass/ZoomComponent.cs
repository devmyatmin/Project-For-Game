using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomComponent : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	[SerializeField]
	private RectTransform _target;

	[SerializeField]
	private RectTransform _viewport;

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private Vector2 _border;

	[SerializeField]
	private float _minScale = 0.3f;

	[SerializeField]
	private float _maxScale = 1f;

	[SerializeField]
	private float _step = 0.25f;

	private float _currentScale = 1f;

	private int _childCount;

	public void SetScale(float scale)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_currentScale = Mathf.Clamp(scale, _minScale, _maxScale);
		((Transform)_target).localScale = new Vector3(_currentScale, _currentScale, 1f);
	}

	public void SetMinScale(float minScale)
	{
		_minScale = minScale;
		if (_currentScale < _minScale)
		{
			SetScale(_minScale);
		}
	}

	public void SetMaxScale(float maxScale)
	{
		_maxScale = maxScale;
		if (_currentScale > _maxScale)
		{
			SetScale(_maxScale);
		}
	}

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (_target.anchorMin != new Vector2(0.5f, 0.5f) || _target.anchorMax != new Vector2(0.5f, 0.5f))
		{
			Debug.LogWarning((object)"Anchors are not set to Middle(center)");
		}
		_camera = ((Component)((Component)this).transform).GetComponentInParent<Canvas>().worldCamera;
	}

	public void Reset()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		_target.pivot = new Vector2(0.5f, 0.5f);
		((Transform)_target).localPosition = Vector3.zero;
	}

	public void EncapsulateContent()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		int childCount = ((Transform)_target).childCount;
		if (_childCount == childCount)
		{
			return;
		}
		_childCount = childCount;
		Bounds val = default;
		for (int i = 0; i < ((Transform)_target).childCount; i++)
		{
			Transform child = ((Transform)_target).GetChild(i);
			RectTransform val2 = (RectTransform)(object)((child is RectTransform) ? child : null);
			if (!((Object)(object)val2 == (Object)null))
			{
				Vector3 localPosition = ((Transform)val2).localPosition;
				Vector3 one = Vector3.one;
				Rect rect = val2.rect;
				Bounds val3 = new Bounds(localPosition, one * rect.width);
				val.Encapsulate(val3);
			}
		}
		val.extents += new Vector3(_border.x, _border.y, 0f);
		_target.sizeDelta = new Vector2(val.size.x, val.size.y);
	}

	public void ScaleToFitContent()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		EncapsulateContent();
		float num = Mathf.Max(_target.sizeDelta.x, _target.sizeDelta.y);
		Rect rect = _viewport.rect;
		float width = rect.width;
		rect = _viewport.rect;
		float num2 = Mathf.Min(width, rect.height) / num;
		num2 *= 0.999f;
		SetMinScale(num2);
		SetScale(num2);
	}

	void IScrollHandler.OnScroll(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float num = eventData.scrollDelta.y * _step;
		float newScaleValue = _currentScale + num;
		ApplyScale(newScaleValue, eventData.position);
	}

	private void ApplyScale(float newScaleValue, Vector2 position)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		float scale = Mathf.Clamp(newScaleValue, _minScale, _maxScale);
		if (!scale.Equals(_currentScale))
		{
			Vector2 localPoint = default;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_target, position, _camera, ref localPoint);
			SetPivot(_target, localPoint);
			SetScale(scale);
		}
	}

	private void SetPivot(RectTransform rectTransform, Vector2 localPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = rectTransform.rect;
		float num = (float)(((double)localPoint.x - (double)rect.x) / ((double)rect.xMax - (double)rect.x));
		float num2 = (float)(((double)localPoint.y - (double)rect.y) / ((double)rect.yMax - (double)rect.y));
		Vector2 val = new Vector2(num, num2);
		Vector2 val2 = (rectTransform.pivot - val) * _currentScale;
		Vector2 size = rect.size;
		Vector3 val3 = new Vector3(val2.x * size.x, val2.y * size.y) * -1f;
		rectTransform.pivot = val;
		((Transform)rectTransform).localPosition = ((Transform)rectTransform).localPosition + val3;
	}
}
