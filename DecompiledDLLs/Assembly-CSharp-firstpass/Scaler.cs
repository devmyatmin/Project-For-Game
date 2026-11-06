using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class Scaler : MonoBehaviour
{
	[SerializeField]
	private float _borderPx;

	private void Update()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)((Component)this).transform.parent;
		Rect rect = val.rect;
		float width = rect.width;
		rect = val.rect;
		float num;
		if ((num = rect.height) > width)
		{
			num = width;
		}
		num -= _borderPx * 2f;
		((RectTransform)((Component)this).transform).sizeDelta = new Vector2(num, num);
	}
}
