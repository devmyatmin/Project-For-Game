using System;
using LE_LevelEditor.Core;
using UnityEngine;

namespace LE_LevelEditor.Events;

public class LE_ObjectDragEvent : EventArgs
{
	private readonly LE_Object m_objectPrefab;

	private readonly LE_Object m_objectPreview;

	private bool m_isObjectPlaceable;

	private string m_message;

	private readonly RaycastHit m_cursorHitInfo;

	public LE_Object ObjectPrefab => m_objectPrefab;

	public LE_Object ObjectPreview => m_objectPreview;

	public bool IsObjectPlaceable
	{
		get
		{
			return m_isObjectPlaceable;
		}
		set
		{
			if (!m_isObjectPlaceable & value)
			{
				Debug.LogError((object)"LE_ObjectDragEvent: IsObjectPlaceable was 'false' you have tried to set it 'true'!");
			}
			else
			{
				m_isObjectPlaceable = value;
			}
		}
	}

	public string Message
	{
		get
		{
			return m_message;
		}
		set
		{
			m_message = value;
		}
	}

	public RaycastHit CursorHitInfo
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_cursorHitInfo;
		}
	}

	public LE_ObjectDragEvent(LE_Object p_objectPrefab, LE_Object p_objectPreview, bool p_isObjectPlaceable, string p_message, RaycastHit p_cursorHitInfo)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		m_objectPrefab = p_objectPrefab;
		m_objectPreview = p_objectPreview;
		m_isObjectPlaceable = p_isObjectPlaceable;
		m_message = p_message;
		m_cursorHitInfo = p_cursorHitInfo;
	}
}
