using UnityEngine;
using UnityEngine.EventSystems;

namespace uTools;

[AddComponentMenu("uTools/Tween/Button Scale(uTools)")]
public class uButtonScale : MonoBehaviour, uIPointHandler, IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler, IPointerExitHandler
{
	public RectTransform tweenTarget;

	public Vector3 enter = new Vector3(1.1f, 1.1f, 1.1f);

	public Vector3 down = new Vector3(1.05f, 1.05f, 1.05f);

	public float duration = 0.2f;

	private Vector3 mScale;

	private void Start()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tweenTarget == (Object)null)
		{
			tweenTarget = ((Component)this).GetComponent<RectTransform>();
		}
		mScale = ((Transform)tweenTarget).localScale;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Scale(enter);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Scale(mScale);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Scale(down);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Scale(mScale);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
	}

	private void Scale(Vector3 to)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		uTweenScale.Begin(((Component)tweenTarget).gameObject, ((Transform)tweenTarget).localScale, to, duration);
	}

	public uButtonScale()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
	}
}
