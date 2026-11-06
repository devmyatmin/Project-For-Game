using System;
using LE_LevelEditor.Core;
using LE_LevelEditor.UI;
using UnityEngine;

namespace LE_LevelEditor.Logic;

public class LE_LogicObjects : LE_LogicBase
{
	private LE_GUI3dObject m_GUI3dObject;

	public LE_LogicObjects(LE_GUI3dObject p_GUI3dObject, LE_ObjectMap p_objectMap)
	{
		m_GUI3dObject = p_GUI3dObject;
		LE_GUIInterface.EventHandlers events = LE_GUIInterface.Instance.events;
		events.OnObjectEditModeBtn = (EventHandler<LE_GUIInterface.EventHandlers.ObjectEditModeEventArgs>)Delegate.Combine(events.OnObjectEditModeBtn, new EventHandler<LE_GUIInterface.EventHandlers.ObjectEditModeEventArgs>(OnObjectEditModeBtn));
		LE_GUIInterface.EventHandlers events2 = LE_GUIInterface.Instance.events;
		events2.OnObjectSelectDraggable = (EventHandler<LE_GUIInterface.EventHandlers.ObjectSelectDraggableEventArgs>)Delegate.Combine(events2.OnObjectSelectDraggable, new EventHandler<LE_GUIInterface.EventHandlers.ObjectSelectDraggableEventArgs>(OnObjectSelectDraggable));
		LE_GUIInterface.EventHandlers events3 = LE_GUIInterface.Instance.events;
		events3.OnSelectedObjectFocusBtn = (EventHandler)Delegate.Combine(events3.OnSelectedObjectFocusBtn, new EventHandler(OnSelectedObjectFocusBtn));
		LE_GUIInterface.EventHandlers events4 = LE_GUIInterface.Instance.events;
		events4.OnSelectedObjectDuplicateBtn = (EventHandler)Delegate.Combine(events4.OnSelectedObjectDuplicateBtn, new EventHandler(OnSelectedObjectDuplicateBtn));
		LE_GUIInterface.EventHandlers events5 = LE_GUIInterface.Instance.events;
		events5.OnSelectedObjectDeleteBtn = (EventHandler)Delegate.Combine(events5.OnSelectedObjectDeleteBtn, new EventHandler(OnSelectedObjectDeleteBtn));
		if (p_objectMap != null)
		{
			if (p_objectMap.ObjectPrefabs.Count > 0)
			{
				if (LE_GUIInterface.Instance.delegates.SetObjects != null)
				{
					LE_GUIInterface.Instance.delegates.SetObjects(p_objectMap);
				}
				else
				{
					Debug.LogError((object)"LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetObjects, but the provided object map is not empty. You have to set this delegate to update your object UI.");
				}
			}
		}
		else
		{
			Debug.LogError((object)"LE_LogicObjects: ROOT_OBJECT_MAP was not initialized! You need to set it in the inspector of LE_LevelEditorMain.");
		}
		m_GUI3dObject.ObjectEditMode = LE_EObjectEditMode.MOVE;
		if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectFocusBtnInteractable == null)
		{
			Debug.LogWarning((object)"LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetIsSelectedObjectFocusBtnInteractable delegate! It might confuse players if you have a focus button, which is always clickable, but works only when an object is selected. Set this delegate to disable the focus button when no object is selected.");
		}
		if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDuplicateBtnInteractable == null)
		{
			Debug.LogWarning((object)"LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetIsSelectedObjectDuplicateBtnInteractable delegate! It might confuse players if you have a duplicate button, which is always clickable, but works only when an object can be duplicated. Set this delegate to disable the duplicate button when no object is selected or this object cannot be created (e.g. max. count reached in this level).");
		}
		if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDeleteBtnInteractable == null)
		{
			Debug.LogWarning((object)"LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetIsSelectedObjectDeleteBtnInteractable delegate! It might confuse players if you have a delete button, which is always clickable, but works only when an object is selected. Set this delegate to disable the delete button when no object is selected.");
		}
	}

	public override void Destroy()
	{
		if ((Object)(object)LE_GUIInterface.Instance != (Object)null)
		{
			LE_GUIInterface.EventHandlers events = LE_GUIInterface.Instance.events;
			events.OnObjectEditModeBtn = (EventHandler<LE_GUIInterface.EventHandlers.ObjectEditModeEventArgs>)Delegate.Remove(events.OnObjectEditModeBtn, new EventHandler<LE_GUIInterface.EventHandlers.ObjectEditModeEventArgs>(OnObjectEditModeBtn));
			LE_GUIInterface.EventHandlers events2 = LE_GUIInterface.Instance.events;
			events2.OnObjectSelectDraggable = (EventHandler<LE_GUIInterface.EventHandlers.ObjectSelectDraggableEventArgs>)Delegate.Remove(events2.OnObjectSelectDraggable, new EventHandler<LE_GUIInterface.EventHandlers.ObjectSelectDraggableEventArgs>(OnObjectSelectDraggable));
			LE_GUIInterface.EventHandlers events3 = LE_GUIInterface.Instance.events;
			events3.OnSelectedObjectFocusBtn = (EventHandler)Delegate.Remove(events3.OnSelectedObjectFocusBtn, new EventHandler(OnSelectedObjectFocusBtn));
			LE_GUIInterface.EventHandlers events4 = LE_GUIInterface.Instance.events;
			events4.OnSelectedObjectDuplicateBtn = (EventHandler)Delegate.Remove(events4.OnSelectedObjectDuplicateBtn, new EventHandler(OnSelectedObjectDuplicateBtn));
			LE_GUIInterface.EventHandlers events5 = LE_GUIInterface.Instance.events;
			events5.OnSelectedObjectDeleteBtn = (EventHandler)Delegate.Remove(events5.OnSelectedObjectDeleteBtn, new EventHandler(OnSelectedObjectDeleteBtn));
		}
	}

	public override void Update()
	{
		bool flag = (Object)(object)m_GUI3dObject.SelectedObject != (Object)null;
		bool obj = flag && m_GUI3dObject.IsObjectPlaceable(m_GUI3dObject.SelectedObject);
		if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectFocusBtnInteractable != null)
		{
			LE_GUIInterface.Instance.delegates.SetIsSelectedObjectFocusBtnInteractable(flag);
		}
		if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDuplicateBtnInteractable != null)
		{
			LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDuplicateBtnInteractable(obj);
		}
		if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDeleteBtnInteractable != null)
		{
			LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDeleteBtnInteractable(flag);
		}
	}

	private void OnObjectEditModeBtn(object p_obj, LE_GUIInterface.EventHandlers.ObjectEditModeEventArgs p_args)
	{
		m_GUI3dObject.ObjectEditMode = p_args.EditMode;
	}

	private void OnObjectSelectDraggable(object p_obj, LE_GUIInterface.EventHandlers.ObjectSelectDraggableEventArgs p_args)
	{
		m_GUI3dObject.SetDraggableObject(p_args.ObjPrefab);
	}

	private void OnSelectedObjectFocusBtn(object p_obj, EventArgs p_args)
	{
		if ((Object)(object)m_GUI3dObject.SelectedObject != (Object)null)
		{
			m_GUI3dObject.Focus();
		}
		else
		{
			Debug.LogError((object)"LE_LogicObject: OnSelectedObjectFocusBtn: you have triggered the focus button behaviour, but there is no object selected! You should listen to the LE_GUIInterface.delegates.SetIsSelectedObjectFocusBtnInteractable and change the button's state accordingly. This will prevent users from getting irritated by not working buttons.");
		}
	}

	private void OnSelectedObjectDuplicateBtn(object p_obj, EventArgs p_args)
	{
		if ((Object)(object)m_GUI3dObject.SelectedObject != (Object)null)
		{
			m_GUI3dObject.CloneObject();
		}
		else
		{
			Debug.LogError((object)"LE_LogicObject: OnSelectedObjectDuplicateBtn: you have triggered the duplicate button behaviour, but there is no object selected! You should listen to the LE_GUIInterface.delegates.SetIsSelectedObjectDuplicateBtnInteractable and change the button's state accordingly. This will prevent users from getting irritated by not working buttons.");
		}
	}

	private void OnSelectedObjectDeleteBtn(object p_obj, EventArgs p_args)
	{
		if ((Object)(object)m_GUI3dObject.SelectedObject != (Object)null)
		{
			if (LE_GUIInterface.Instance.delegates.ShowPopupConfirmDeleteObject != null)
			{
				ConfirmDelete();
			}
			else
			{
				Debug.LogError((object)"LE_LogicObject: OnSelectedObjectDeleteBtn: you have triggered the delete button behaviour, but the LE_GUIInterface.delegates.ShowPopupConfirmDeleteObject delegate is not set! Set this delegate to make the delete button work!");
			}
		}
		else
		{
			Debug.LogError((object)"LE_LogicObject: OnSelectedObjectDeleteBtn: you have triggered the delete button behaviour, but there is no object selected! You should listen to the LE_GUIInterface.delegates.SetIsSelectedObjectDeleteBtnInteractable and change the button's state accordingly. This will prevent users from getting irritated by not working buttons.");
		}
	}

	public void ConfirmDelete()
	{
		LE_EObjectEditMode objectEditMode = m_GUI3dObject.ObjectEditMode;
		m_GUI3dObject.ObjectEditMode = LE_EObjectEditMode.NO_EDIT;
		LE_GUIInterface.Instance.delegates.ShowPopupConfirmDeleteObject((bool p_isDeleteConfirmed) =>
		{
			if (p_isDeleteConfirmed)
			{
				m_GUI3dObject.Delete();
			}
		});
		m_GUI3dObject.ObjectEditMode = objectEditMode;
	}
}
