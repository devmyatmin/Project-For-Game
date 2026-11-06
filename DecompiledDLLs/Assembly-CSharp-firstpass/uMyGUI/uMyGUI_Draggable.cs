using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace uMyGUI;

public class uMyGUI_Draggable : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public class DragEvent : EventArgs
	{
		public readonly PointerEventData m_event;

		public DragEvent(PointerEventData p_event)
		{
			m_event = p_event;
		}
	}

	[SerializeField]
	private bool m_isResetRotationWhenDragged;

	[SerializeField]
	private bool m_isSnapBackOnEndDrag;

	[SerializeField]
	private bool m_isTopInHierarchyWhenDragged = true;

	[SerializeField]
	private CanvasGroup m_disableBlocksRaycastsOnDrag;

	private bool m_isDragged;

	public EventHandler<DragEvent> m_onBeginDrag;

	public EventHandler<DragEvent> m_onDrag;

	public EventHandler<DragEvent> m_onEndDrag;

	private RectTransform m_initialParentTransform;

	private RectTransform m_canvasTransform;

	private RectTransform m_transform;

	private int m_initialSiblingIndex;

	private Vector3 m_initialPosition;

	private Quaternion m_initialRotation;

	private Vector3 m_dragOffset = Vector3.zero;

	public bool IsResetRotationWhenDragged
	{
		get
		{
			return m_isResetRotationWhenDragged;
		}
		set
		{
			m_isResetRotationWhenDragged = value;
		}
	}

	public bool IsSnapBackOnEndDrag
	{
		get
		{
			return m_isSnapBackOnEndDrag;
		}
		set
		{
			m_isSnapBackOnEndDrag = value;
		}
	}

	public bool IsTopInHierarchyWhenDragged
	{
		get
		{
			return m_isTopInHierarchyWhenDragged;
		}
		set
		{
			m_isTopInHierarchyWhenDragged = value;
		}
	}

	public CanvasGroup DisableBlocksRaycastsOnDrag
	{
		get
		{
			return m_disableBlocksRaycastsOnDrag;
		}
		set
		{
			m_disableBlocksRaycastsOnDrag = value;
		}
	}

	public bool IsDragged => m_isDragged;

	public void OnBeginDrag(PointerEventData p_event)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if ((int)p_event.button == 0)
		{
			m_isDragged = true;
			Transform parent = ((Transform)m_transform).parent;
			m_initialParentTransform = (RectTransform)(object)((parent is RectTransform) ? parent : null);
			m_initialPosition = ((Transform)m_transform).position;
			m_initialRotation = ((Transform)m_transform).rotation;
			Vector3 val = default;
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_canvasTransform, p_event.position, p_event.pressEventCamera, ref val))
			{
				m_dragOffset = ((Transform)m_transform).position - val;
			}
			else
			{
				m_dragOffset = Vector3.zero;
			}
			if (m_isResetRotationWhenDragged)
			{
				((Transform)m_transform).rotation = Quaternion.identity;
			}
			if (m_isTopInHierarchyWhenDragged)
			{
				m_initialSiblingIndex = ((Transform)m_transform).GetSiblingIndex();
				((Transform)m_transform).SetParent((Transform)(object)m_canvasTransform, true);
				((Transform)m_transform).SetAsLastSibling();
			}
			if ((Object)(object)m_disableBlocksRaycastsOnDrag != (Object)null)
			{
				m_disableBlocksRaycastsOnDrag.blocksRaycasts = false;
			}
			if (m_onBeginDrag != null)
			{
				m_onBeginDrag(this, new DragEvent(p_event));
			}
		}
	}

	public void OnDrag(PointerEventData p_event)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (m_isDragged)
		{
			Vector3 val = default;
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_canvasTransform, p_event.position, p_event.pressEventCamera, ref val))
			{
				((Transform)m_transform).position = val + m_dragOffset;
			}
			if (m_onDrag != null)
			{
				m_onDrag(this, new DragEvent(p_event));
			}
		}
	}

	public void OnEndDrag(PointerEventData p_event)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (m_isDragged)
		{
			m_isDragged = false;
			if (m_isSnapBackOnEndDrag)
			{
				((Transform)m_transform).position = m_initialPosition;
				((Transform)m_transform).rotation = m_initialRotation;
			}
			if (m_isTopInHierarchyWhenDragged)
			{
				((Transform)m_transform).SetParent((Transform)(object)m_initialParentTransform, true);
				((Transform)m_transform).SetSiblingIndex(m_initialSiblingIndex);
			}
			if ((Object)(object)m_disableBlocksRaycastsOnDrag != (Object)null)
			{
				m_disableBlocksRaycastsOnDrag.blocksRaycasts = true;
			}
			if (m_onEndDrag != null)
			{
				m_onEndDrag(this, new DragEvent(p_event));
			}
		}
	}

	private void Start()
	{
		Canvas componentInParent = ((Component)this).GetComponentInParent<Canvas>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			m_canvasTransform = ((Component)componentInParent).GetComponent<RectTransform>();
		}
		else
		{
			Debug.LogError((object)"uMyGUI_Draggable: no Canvas component was found in parent!");
			((Behaviour)this).enabled = false;
		}
		m_transform = ((Component)this).GetComponent<RectTransform>();
	}

	public uMyGUI_Draggable()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
	}
}
