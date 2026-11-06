using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LE_LevelEditor.UI;

public class LE_GUIInterface_uGUIimplBase : MonoBehaviour
{
	protected RectTransform m_transform;

	protected virtual void Start()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected Obj, but got Unknown
		if ((Object)(object)LE_GUIInterface.Instance != (Object)null)
		{
			LE_GUIInterface.Delegates delegates = LE_GUIInterface.Instance.delegates;
			delegates.IsCursorOverUI = (LE_GUIInterface.Delegates.GetBoolFunction)Delegate.Combine(delegates.IsCursorOverUI, new LE_GUIInterface.Delegates.GetBoolFunction(IsCursorOverUI));
		}
		else
		{
			Debug.LogError((object)"LE_GUIInterface_uGUIimplBase: Start: could not find LE_GUIInterface!");
		}
		m_transform = (RectTransform)((Component)this).transform;
		if ((Object)(object)m_transform == (Object)null)
		{
			Debug.LogError((object)"LE_GUIInterface_uGUIimplBase: this script must be attached to a RectTransform!");
		}
	}

	protected virtual bool IsCursorOverUI()
	{
		return EventSystem.main.IsPointerOverGameObject();
	}
}
