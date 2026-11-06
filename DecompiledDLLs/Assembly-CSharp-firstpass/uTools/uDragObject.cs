using UnityEngine;
using UnityEngine.EventSystems;

namespace uTools;

[AddComponentMenu("uTools/Internal/Drag Object(uTools)")]
public class uDragObject : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	public RectTransform target;

	private RectTransform cacheTarget
	{
		get
		{
			if ((Object)(object)target == (Object)null)
			{
				target = ((Component)this).GetComponent<RectTransform>();
			}
			return target;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Transform)cacheTarget).localPosition;
		Vector3 to = localPosition + new Vector3(eventData.delta.x, eventData.delta.y, 0f);
		uTweenPosition.Begin(((Component)this).gameObject, localPosition, to, 0.02f);
	}
}
