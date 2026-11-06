using System;
using System.Collections.Generic;
using Beam;
using Funlabs;
using LE_LevelEditor.Core;
using LE_LevelEditor.Events;
using MyUtility;
using Rewired;
using S_SnapTools;
using UnityEngine;

namespace LE_LevelEditor.UI;

public class LE_GUI3dObject : LE_GUI3dBase
{
	private const float MIN_FOCUS_DISTANCE = 3f;

	private const float SELECTION_MAX_OVERSIZE = 10f;

	private LE_Object m_object;

	private bool m_isObjectPlaceable;

	private string m_dragMessage = "";

	private LE_Object m_previewInstance;

	private LE_Object m_selectedObject;

	private LE_Object m_cursorActionOnObject;

	private bool m_isSelectionPossible;

	private bool m_isCursorAction;

	private bool m_isCursorActionInThisFrame;

	private Vector3 m_lastCursorActionStartPos = -100f * Vector3.one;

	private bool m_isSnapToObjectActive = true;

	private Dictionary<string, S_SnapToObject> m_snapPointUIDsToSnapPoints = new Dictionary<string, S_SnapToObject>();

	private Dictionary<string, int> m_snapPointUIDsToObjUIDs = new Dictionary<string, int>();

	public int TERRAIN_LAYER = 14;

	public Action<Vector3, float> OnFocused;

	private LE_EObjectEditMode m_objectEditMode;

	private bool m_isKeyComboDuplicate = true;

	private bool m_isKeyComboFocus = true;

	public LE_Object SelectedObject => m_selectedObject;

	public Dictionary<string, int> SnapPointUIDsToObjUIDs => m_snapPointUIDsToObjUIDs;

	public override LE_EEditMode ActiveEditMode => LE_EEditMode.OBJECT;

	public LE_EObjectEditMode ObjectEditMode
	{
		get
		{
			return m_objectEditMode;
		}
		set
		{
			m_objectEditMode = value;
		}
	}

	public bool IsKeyComboDuplicate
	{
		get
		{
			return m_isKeyComboDuplicate;
		}
		set
		{
			m_isKeyComboDuplicate = value;
		}
	}

	public bool IsKeyComboFocus
	{
		get
		{
			return m_isKeyComboFocus;
		}
		set
		{
			m_isKeyComboFocus = value;
		}
	}

	public override void SetCursorPosition(Vector3 p_cursorScreenCoords)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_object != (Object)null && (m_object.SnapType == LE_Object.ESnapType.SNAP_TO_TERRAIN || m_object.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN))
		{
			m_cursorScreenCoords = p_cursorScreenCoords;
			m_cursorRay = Camera.main.ScreenPointToRay(p_cursorScreenCoords);
			SetIsCursorOverSomething(Physics.Raycast(m_cursorRay, ref m_cursorHitInfo, float.MaxValue, 1 << TERRAIN_LAYER, (QueryTriggerInteraction)1));
		}
		else
		{
			base.SetCursorPosition(p_cursorScreenCoords);
		}
	}

	public override void SetIsCursorAction(bool p_isCursorAction)
	{
		if (IsInteractable)
		{
			m_isCursorActionInThisFrame |= p_isCursorAction;
		}
	}

	public void SetDraggableObject(LE_Object p_object)
	{
		if ((Object)(object)m_object != (Object)(object)p_object)
		{
			m_dragMessage = "";
			SetDragMessageInUI();
			m_object = p_object;
			m_isObjectPlaceable = IsObjectPlaceable();
		}
	}

	public void Focus()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_selectedObject != (Object)null))
		{
			return;
		}
		Renderer[] componentsInChildren = ((Component)m_selectedObject).GetComponentsInChildren<Renderer>();
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		float num = 0f;
		Renderer[] array = componentsInChildren;
		foreach (Renderer val3 in array)
		{
			if ((Object)(object)((Component)val3).GetComponentInParent<LE_ObjectEditHandle>() == (Object)null)
			{
				Vector3 val4 = val;
				Bounds bounds = val3.bounds;
				val = val4 + bounds.center;
				Vector3 val5 = val2;
				bounds = val3.bounds;
				val2 = val5 + bounds.size;
				num++;
			}
		}
		if (num != 0f)
		{
			val *= 1f / num;
			val2 *= 1f / num;
		}
		else
		{
			val = ((Component)m_selectedObject).transform.position;
			val2 = Vector3.one * 3f;
		}
		Vector3 val6 = ((Component)Camera.main).transform.position - val;
		float num2 = Mathf.Max(new float[3] { val2.x, val2.y, val2.z }) * 3f;
		((Component)Camera.main).transform.position = val + val6.normalized * num2;
		if (((Component)Camera.main).transform.position.y < val.y)
		{
			Vector3 position = ((Component)Camera.main).transform.position;
			position.y = val.y;
			((Component)Camera.main).transform.position = position;
		}
		((Component)Camera.main).transform.LookAt(val, Vector3.up);
		if (OnFocused != null)
		{
			OnFocused(val, num2);
		}
	}

	public void Delete()
	{
		if ((Object)(object)m_selectedObject != (Object)null)
		{
			ReactivateSnapPoints(m_selectedObject.UID);
			Object.Destroy((Object)(object)((Component)m_selectedObject).gameObject);
			((Object)m_selectedObject).name = "deleted";
			m_isObjectPlaceable = IsObjectPlaceable();
			LE_EventInterface.LevelDataChanged(((Component)m_selectedObject).gameObject);
		}
	}

	public void RemoveSelection()
	{
		SelectObject(null);
	}

	public void CloneObject()
	{
		if ((Object)(object)m_selectedObject != (Object)null && IsObjectPlaceable(m_selectedObject))
		{
			m_selectedObject.EditMode = LE_EObjectEditMode.NO_EDIT;
			m_selectedObject.IsSelected = false;
			_ = ((Object)m_selectedObject).name;
			PlaceObject(m_selectedObject, ((Component)m_selectedObject).transform, p_isDestroyClonedScripts: true, clonedOffset: true);
		}
	}

	public bool IsObjectPlaceable(LE_Object p_object)
	{
		if (p_object.MaxInstancesInLevel != 0)
		{
			int num = 0;
			LE_Object[] array = Object.FindObjectsOfType<LE_Object>();
			for (int i = 0; i < array.Length; i++)
			{
				if ((!((Object)(object)m_previewInstance != (Object)null) || !((Object)(object)m_previewInstance == (Object)(object)array[i])) && array[i].Id == p_object.Id)
				{
					num++;
					if (p_object.MaxInstancesInLevel <= num)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public void AddSnappingScripts(LE_Object p_newObject)
	{
		if ((Object)(object)p_newObject != (Object)null)
		{
			if (p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_OBJECT)
			{
				AddObjectSnapping(p_newObject);
			}
			else if (p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_TERRAIN)
			{
				AddTerrainSnapping(p_newObject);
			}
			else if (p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_3D_GRID || p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
			{
				AddGridSnapping(p_newObject, p_isPreview: false);
			}
		}
		else
		{
			Debug.LogError((object)"LE_GUI3dObject: AddSnappingScripts: passed object is null!");
		}
	}

	public void SetSnapPointUIDsToObjUIDs(Dictionary<string, int> p_snapPointUIDsToObjUIDs)
	{
		foreach (KeyValuePair<string, int> p_snapPointUIDsToObjUID in p_snapPointUIDsToObjUIDs)
		{
			m_snapPointUIDsToObjUIDs.Add(p_snapPointUIDsToObjUID.Key, p_snapPointUIDsToObjUID.Value);
		}
	}

	public void ClearLevelData()
	{
		m_snapPointUIDsToSnapPoints.Clear();
		m_snapPointUIDsToObjUIDs.Clear();
		m_isSnapToObjectActive = true;
	}

	private void Start()
	{
		if (LE_GUIInterface.Instance.delegates.IsObjectDragged == null)
		{
			Debug.LogError((object)"LE_GUI3dObject: you must provide the LE_GUIInterface.delegates.IsObjectDragged to make the object drag&drop working!");
		}
		if (LE_GUIInterface.Instance.delegates.SetDraggableObjectMessage == null)
		{
			Debug.LogWarning((object)"LE_GUI3dObject: LE_GUIInterface.delegates.SetDraggableObjectMessage is not set. Set it if you want to visualize the 'max. # reached!' message or messages generated via LE_ObjectDragEvent.");
		}
		if (LE_GUIInterface.Instance.delegates.SetDraggableObjectState == null)
		{
			Debug.LogWarning((object)"LE_GUI3dObject: LE_GUIInterface.delegates.SetDraggableObjectState is not set. Set it if you want to hide your UI when a 3d preview is shown or show different UI if the object is not placeable.");
		}
		S_SnapToObject.OnNewObjectSnapped = (EventHandler<S_SnapToObjectEventArgs>)Delegate.Combine(S_SnapToObject.OnNewObjectSnapped, new EventHandler<S_SnapToObjectEventArgs>(OnNewObjectSnapped));
		S_SnapToObject.OnPreviewObjectInstantiated = (EventHandler<S_SnapToObjectEventArgs>)Delegate.Combine(S_SnapToObject.OnPreviewObjectInstantiated, new EventHandler<S_SnapToObjectEventArgs>(OnPreviewObjectInstantiated));
	}

	private void OnDestroy()
	{
		S_SnapToObject.OnNewObjectSnapped = (EventHandler<S_SnapToObjectEventArgs>)Delegate.Remove(S_SnapToObject.OnNewObjectSnapped, new EventHandler<S_SnapToObjectEventArgs>(OnNewObjectSnapped));
		S_SnapToObject.OnPreviewObjectInstantiated = (EventHandler<S_SnapToObjectEventArgs>)Delegate.Remove(S_SnapToObject.OnPreviewObjectInstantiated, new EventHandler<S_SnapToObjectEventArgs>(OnPreviewObjectInstantiated));
	}

	private void OnNewObjectPlaced(LE_Object p_newInstance)
	{
		AddSnappingScripts(p_newInstance);
		SelectObject(p_newInstance);
		m_isObjectPlaceable = IsObjectPlaceable();
		LE_EventInterface.LevelDataChanged(p_newInstance);
		if (LE_EventInterface.OnObjectPlaced != null)
		{
			LE_EventInterface.OnObjectPlaced(this, new LE_ObjectPlacedEvent(p_newInstance));
		}
	}

	private void OnPreviewObjectInstantiated(object p_sender, S_SnapToObjectEventArgs p_args)
	{
		LE_Object component = p_args.NewInstance.GetComponent<LE_Object>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
	}

	private void OnNewObjectSnapped(object p_sender, S_SnapToObjectEventArgs p_args)
	{
		LE_Object component = p_args.NewInstance.GetComponent<LE_Object>();
		if ((Object)(object)component != (Object)null)
		{
			LE_Object componentInParent = ((Component)p_args.Source).GetComponentInParent<LE_Object>();
			LE_Object component2 = p_args.NewInstance.GetComponent<LE_Object>();
			MarkSnapPointAsUsed(componentInParent, component2, p_args.Source);
			if (component2.RootSnapPointIndex != -1)
			{
				MarkSnapPointAsUsed(component2, componentInParent, component2.RootSnapPointIndex);
			}
			OnNewObjectPlaced(component);
		}
		else
		{
			Debug.LogError((object)"LE_GUI3dObject: OnNewObjectSnapped: LE_Object is not attached to the root of the new object! This object will not be saved if it has no LE_Object attached!");
		}
	}

	private void MarkSnapPointAsUsed(LE_Object p_sourceObj, LE_Object p_destinationObj, S_SnapToObject p_snapScript)
	{
		if ((Object)(object)p_sourceObj != (Object)null)
		{
			int snapPointIndex = -1;
			for (int i = 0; i < p_sourceObj.ObjectSnapPoints.Length; i++)
			{
				if ((Object)(object)p_sourceObj.ObjectSnapPoints[i].SnapSystemInstance == (Object)(object)p_snapScript)
				{
					snapPointIndex = i;
					break;
				}
			}
			MarkSnapPointAsUsed(p_sourceObj, p_destinationObj, snapPointIndex);
		}
		else
		{
			Debug.LogError((object)"LE_GUI3dObject: MarkSnapPointAsUsed: could not find LE_Object of snap source!");
		}
	}

	private void MarkSnapPointAsUsed(LE_Object p_sourceObj, LE_Object p_destinationObj, int snapPointIndex)
	{
		if ((Object)(object)p_sourceObj != (Object)null)
		{
			if (snapPointIndex != -1)
			{
				string text = p_sourceObj.UID + "_" + snapPointIndex;
				if (!m_snapPointUIDsToObjUIDs.ContainsKey(text))
				{
					m_snapPointUIDsToObjUIDs.Add(text, p_destinationObj.UID);
					return;
				}
				Debug.LogError((object)("LE_GUI3dObject: MarkSnapPointAsUsed: duplicate snapping on snapPointUID(" + text + ") new (ignored) objID(" + p_destinationObj.UID + ") and old objID(" + m_snapPointUIDsToObjUIDs[text] + ")"));
			}
			else
			{
				Debug.LogError((object)"LE_GUI3dObject: MarkSnapPointAsUsed: could not find LE_ObjectSnapPoint of snap source!");
			}
		}
		else
		{
			Debug.LogError((object)"LE_GUI3dObject: MarkSnapPointAsUsed: could not find LE_Object of snap source!");
		}
	}

	private bool IsObjectPlaceable()
	{
		if ((Object)(object)m_object != (Object)null)
		{
			if (!IsObjectPlaceable(m_object))
			{
				m_dragMessage = "MAP_EDITOR_MAX_NUM_OBJECTS";
				SetDragMessageInUI();
				return false;
			}
			m_dragMessage = "";
			SetDragMessageInUI();
			return true;
		}
		return false;
	}

	private void AddObjectSnapping(LE_Object p_newObject)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected Obj, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected Obj, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		for (int i = 0; i < p_newObject.ObjectSnapPoints.Length; i++)
		{
			if (p_newObject.ObjectSnapPoints[i] != null)
			{
				Material p_materialLine = null;
				Material p_materialFill = null;
				if (p_newObject.IsDrawSnapToObjectUI)
				{
					p_materialLine = (Material)Resources.Load("SnapToObjectUIMaterial_Line");
					p_materialFill = (Material)Resources.Load("SnapToObjectUIMaterial_Fill");
				}
				S_SnapToObject s_SnapToObject = p_newObject.ObjectSnapPoints[i].InstatiateSnapSystem((GameObject)Object.Instantiate(Resources.Load("ObjectSnapButtonVisuals")), p_newObject.IsDrawSnapToObjectUI, p_materialLine, p_materialFill);
				if ((Object)(object)s_SnapToObject != (Object)null)
				{
					string text = p_newObject.UID + "_" + i;
					if (!m_snapPointUIDsToSnapPoints.ContainsKey(text))
					{
						m_snapPointUIDsToSnapPoints.Add(text, s_SnapToObject);
					}
					else
					{
						Debug.LogError((object)("LE_GUI3dObject: AddObjectSnapping: a snap point instance of with the UID '" + text + "' already exists!"));
					}
					if (!m_isSnapToObjectActive)
					{
						((Component)s_SnapToObject).gameObject.SetActive(false);
					}
					if (m_snapPointUIDsToObjUIDs.ContainsKey(text))
					{
						s_SnapToObject.IncSnapCounter();
					}
				}
			}
			else
			{
				Debug.LogError((object)("LE_GUI3dObject: AddObjectSnapping: object '" + ((Object)p_newObject).name + "' has a nullpointer in the ObjectSnapPoints array at index '" + i + "'!"));
			}
		}
	}

	private void AddTerrainSnapping(LE_Object p_newObject)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		S_SnapToWorld worldSnap = ((Component)p_newObject).gameObject.AddComponent<S_SnapToWorld>();
		worldSnap.SnapToLayers = LayerMask.op_Implicit(1 << TERRAIN_LAYER);
		worldSnap.SnapFrameRate = -1;
		worldSnap.IsRotationSnap = p_newObject.IsPlacementRotationByNormal;
		worldSnap.DoSnap();
		EventHandler snapUpdateFunct = (object p_object, EventArgs p_args) =>
		{
			if (p_object is LE_TerrainManager || (p_object is LE_ObjectEditHandle && ((Component)(LE_ObjectEditHandle)p_object).transform.IsChildOf(((Component)p_newObject).transform)))
			{
				worldSnap.DoSnap();
			}
		};
		LE_EventInterface.OnChangeLevelData = (EventHandler)Delegate.Combine(LE_EventInterface.OnChangeLevelData, snapUpdateFunct);
		((Component)p_newObject).gameObject.AddComponent<UtilityOnDestroyHandler>().m_onDestroy = () =>
		{
			LE_EventInterface.OnChangeLevelData = (EventHandler)Delegate.Remove(LE_EventInterface.OnChangeLevelData, snapUpdateFunct);
		};
	}

	private void AddGridSnapping(LE_Object p_newObject, bool p_isPreview)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		S_SnapToGrid s_SnapToGrid = ((Component)p_newObject).gameObject.AddComponent<S_SnapToGrid>();
		s_SnapToGrid.GridOffset = p_newObject.SnapGridOffset;
		s_SnapToGrid.GridCellSize = p_newObject.SnapGridCellSize;
		s_SnapToGrid.SnapCondition = ((!p_isPreview) ? S_SnapToGrid.ESnapCondition.WHEN_STILL : S_SnapToGrid.ESnapCondition.ON_UPDATE);
		s_SnapToGrid.IsInstantSnap = p_isPreview;
		if (p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
		{
			s_SnapToGrid.IsSnapAxisY = false;
			if (!p_isPreview)
			{
				AddTerrainSnapping(p_newObject);
			}
		}
	}

	private void Update()
	{
		UpdateObjectSelection();
		UpdateNewObjectDragAndDrop();
		UpdateSnapToObjectInstances();
		Player player = ReInput.players.GetPlayer(0);
		if (m_isKeyComboFocus && player.GetButtonDown(53))
		{
			Focus();
		}
		m_isCursorActionInThisFrame = false;
	}

	private void UpdateObjectSelection()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		bool isCursorOverSomething = IsCursorOverSomething;
		if ((Object)(object)m_previewInstance == (Object)null && !IsObjectDraggedInUI())
		{
			if (!IsInteractable)
			{
				m_isCursorAction = isCursorOverSomething;
				m_isSelectionPossible = false;
			}
			else if (!m_isCursorAction && m_isCursorActionInThisFrame)
			{
				if (isCursorOverSomething)
				{
					m_cursorActionOnObject = ((Component)m_cursorHitInfo.collider).GetComponentInParent<LE_Object>();
				}
				else
				{
					m_cursorActionOnObject = null;
				}
				m_lastCursorActionStartPos = m_cursorScreenCoords;
				m_isSelectionPossible = true;
				m_isCursorAction = true;
			}
			else if (m_isCursorAction && m_isCursorActionInThisFrame)
			{
				if ((isCursorOverSomething || !((Object)(object)m_cursorActionOnObject != (Object)null)) && (!isCursorOverSomething || !((Object)(object)m_cursorActionOnObject != (Object)(object)((Component)m_cursorHitInfo.collider).GetComponentInParent<LE_Object>())))
				{
					Vector3 val = m_lastCursorActionStartPos - m_cursorScreenCoords;
					if (!((double)val.magnitude > (double)Screen.height * 0.025))
					{
						goto IL_016f;
					}
				}
				m_isSelectionPossible = false;
				m_cursorActionOnObject = null;
			}
			else if (m_isCursorAction && !m_isCursorActionInThisFrame)
			{
				if (m_isSelectionPossible)
				{
					if ((Object)(object)m_cursorActionOnObject == (Object)null)
					{
						m_cursorActionOnObject = TrySelectByOversizedBoundingVolume();
					}
					SelectObject(m_cursorActionOnObject);
				}
				m_isSelectionPossible = false;
				m_cursorActionOnObject = null;
				m_isCursorAction = false;
			}
			else
			{
				m_cursorActionOnObject = null;
				m_isCursorAction = false;
			}
		}
		goto IL_016f;
		IL_016f:
		if ((Object)(object)m_selectedObject != (Object)null)
		{
			m_selectedObject.EditMode = m_objectEditMode;
		}
	}

	private void UpdateNewObjectDragAndDrop()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_object != (Object)null))
		{
			return;
		}
		SetDragMessageInUI();
		if (!IsInteractable)
		{
			if ((Object)(object)m_previewInstance != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)m_previewInstance).gameObject);
			}
		}
		else if (IsCursorOverSomething && m_cursorHitInfo.point.x < 150f && m_cursorHitInfo.point.x > -150f && m_cursorHitInfo.point.z < 150f && m_cursorHitInfo.point.z > -150f)
		{
			if (IsObjectDraggedInUI())
			{
				if (OnObjectDrag() && ((m_object.SnapType != LE_Object.ESnapType.SNAP_TO_TERRAIN && m_object.SnapType != LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN) || ((Object)(object)m_cursorHitInfo.collider != (Object)null && ((Component)m_cursorHitInfo.collider).gameObject.layer == TERRAIN_LAYER)))
				{
					if ((Object)(object)m_previewInstance == (Object)null)
					{
						m_previewInstance = Object.Instantiate<LE_Object>(m_object);
						((Component)m_previewInstance).gameObject.GetComponent<Transform>().parent = Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer;
						MoveToLayer(((Component)m_previewInstance).transform, LayerMask.NameToLayer("Ignore Raycast"));
						Rigidbody[] componentsInChildren = ((Component)m_previewInstance).GetComponentsInChildren<Rigidbody>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							Object.Destroy((Object)(object)componentsInChildren[i]);
						}
						if (m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_3D_GRID || m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
						{
							AddGridSnapping(m_previewInstance, p_isPreview: true);
						}
					}
					((Component)m_previewInstance).transform.position = m_cursorHitInfo.point;
					if (m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_3D_GRID || m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
					{
						Transform transform = ((Component)m_previewInstance).transform;
						transform.position += m_cursorHitInfo.normal * 0.005f;
					}
					if (m_previewInstance.IsPlacementRotationByNormal)
					{
						((Component)m_previewInstance).transform.up = m_cursorHitInfo.normal;
					}
				}
			}
			else if (m_isObjectPlaceable)
			{
				PlaceObject();
			}
			else if ((Object)(object)m_previewInstance != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)m_previewInstance).gameObject);
			}
		}
		else if ((Object)(object)m_previewInstance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)m_previewInstance).gameObject);
		}
		if (LE_GUIInterface.Instance.delegates.SetDraggableObjectState != null)
		{
			if (!IsInteractable || !IsObjectDraggedInUI())
			{
				LE_GUIInterface.Instance.delegates.SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState.NONE);
			}
			else if ((Object)(object)m_previewInstance != (Object)null)
			{
				LE_GUIInterface.Instance.delegates.SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState.IN_3D_PREVIEW);
			}
			else
			{
				LE_GUIInterface.Instance.delegates.SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState.NOT_PLACEABLE);
			}
		}
	}

	private void UpdateSnapToObjectInstances()
	{
		bool flag = !m_isSnapToObjectActive && IsInteractable;
		if (!flag && (!m_isSnapToObjectActive || IsInteractable))
		{
			return;
		}
		m_isSnapToObjectActive = flag;
		List<string> list = new List<string>();
		foreach (string key in m_snapPointUIDsToSnapPoints.Keys)
		{
			if ((Object)(object)m_snapPointUIDsToSnapPoints[key] != (Object)null)
			{
				if (!flag || m_snapPointUIDsToSnapPoints[key].SnapCounter == 0)
				{
					((Component)m_snapPointUIDsToSnapPoints[key]).gameObject.SetActive(flag);
				}
			}
			else
			{
				list.Add(key);
			}
		}
		foreach (string item in list)
		{
			m_snapPointUIDsToSnapPoints.Remove(item);
		}
	}

	private void ReactivateSnapPoints(int p_deletedObjectUID)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, int> snapPointUIDsToObjUID in m_snapPointUIDsToObjUIDs)
		{
			if (snapPointUIDsToObjUID.Value == p_deletedObjectUID)
			{
				list.Add(snapPointUIDsToObjUID.Key);
				if (m_snapPointUIDsToSnapPoints.ContainsKey(snapPointUIDsToObjUID.Key))
				{
					m_snapPointUIDsToSnapPoints[snapPointUIDsToObjUID.Key].DecSnapCounter();
				}
			}
		}
		foreach (string item in list)
		{
			m_snapPointUIDsToObjUIDs.Remove(item);
		}
	}

	private LE_Object TrySelectByOversizedBoundingVolume()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		List<LE_Object> list = new List<LE_Object>();
		List<float> list2 = new List<float>();
		LE_Object[] array = Object.FindObjectsOfType<LE_Object>();
		Vector3 val;
		for (int i = 0; i < array.Length; i++)
		{
			if (GetMinMaxBounds(array[i], out var o_min, out var o_max))
			{
				float num = 1f;
				val = ((Component)array[i]).transform.position - ((Component)Camera.main).transform.position;
				float magnitude = val.magnitude;
				Vector3 val2 = new Vector3
				{
					x = Mathf.Abs(o_max.x - o_min.x),
					y = Mathf.Abs(o_max.y - o_min.y),
					z = Mathf.Abs(o_max.z - o_min.z)
				};
				float magnitude2 = val2.magnitude;
				Vector3 val3 = Vector3.one * 10f + val2;
				if (magnitude > 50f)
				{
					num = 2.25f;
					val2 = Vector3.Max(val2, Vector3.one * 1.5f);
				}
				else if (magnitude > 25f)
				{
					num = 1.85f;
					val2 = Vector3.Max(val2, Vector3.one);
				}
				else if (magnitude > 15f)
				{
					num = 1.5f;
					val2 = Vector3.Max(val2, Vector3.one * 0.5f);
				}
				val2 = Vector3.Min(val2 * num, val3);
				Bounds val4 = new Bounds((o_max + o_min) * 0.5f, val2);
				if (val4.IntersectRay(m_cursorRay))
				{
					list.Add(array[i]);
					list2.Add(magnitude2);
				}
			}
		}
		if (list.Count > 0)
		{
			LE_Object result = list[0];
			float num2 = list2[0];
			for (int j = 1; j < list.Count; j++)
			{
				if (list2[j] < num2)
				{
					result = list[j];
					num2 = list2[j];
				}
			}
			float num3 = 9999999f;
			for (int k = 0; k < list.Count; k++)
			{
				if (list2[k] <= num2 * 1.25f)
				{
					val = Vector3.Cross(m_cursorRay.direction, ((Component)list[k]).transform.position - m_cursorRay.origin);
					float magnitude3 = val.magnitude;
					if (num3 > magnitude3)
					{
						num3 = magnitude3;
						result = list[k];
					}
				}
			}
			return result;
		}
		return null;
	}

	private bool GetMinMaxBounds(LE_Object p_object, out Vector3 o_min, out Vector3 o_max)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		o_min = Vector3.one * 9999999f;
		o_max = -Vector3.one * 9999999f;
		Collider[] componentsInChildren = ((Component)p_object).GetComponentsInChildren<Collider>();
		if (componentsInChildren.Length != 0)
		{
			bool result = false;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if ((Object)(object)((Component)componentsInChildren[i]).GetComponent<LE_ObjectEditHandleCollider>() == (Object)null)
				{
					result = true;
					Bounds bounds = componentsInChildren[i].bounds;
					o_min = Vector3.Min(o_min, bounds.min);
					o_max = Vector3.Max(o_max, bounds.max);
				}
			}
			return result;
		}
		return false;
	}

	private void PlaceObject()
	{
		if ((Object)(object)m_previewInstance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)m_previewInstance).gameObject);
			PlaceObject(m_object, ((Component)m_previewInstance).transform, p_isDestroyClonedScripts: false);
		}
	}

	private LE_Object PlaceObject(LE_Object p_prefab, Transform p_copyTransform, bool p_isDestroyClonedScripts, bool clonedOffset = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = p_copyTransform.position;
		position.x++;
		if (MPExtensions.XZSqr(position) > 15625f)
		{
			return null;
		}
		LE_Object lE_Object = Object.Instantiate<LE_Object>(p_prefab);
		((Object)((Component)lE_Object).gameObject).name = ((Object)p_prefab).name;
		((Component)lE_Object).gameObject.GetComponent<Transform>().parent = Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer;
		if (clonedOffset)
		{
			((Component)lE_Object).transform.position = p_copyTransform.position + new Vector3(1f, 0f, 0f);
		}
		else
		{
			((Component)lE_Object).transform.position = p_copyTransform.position;
		}
		((Component)lE_Object).transform.rotation = p_copyTransform.rotation;
		((Component)lE_Object).transform.localScale = p_copyTransform.localScale;
		if (p_isDestroyClonedScripts)
		{
			LE_ObjectEditHandle componentInChildren = ((Component)lE_Object).GetComponentInChildren<LE_ObjectEditHandle>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)componentInChildren).gameObject);
			}
			S_SnapToWorld component = ((Component)lE_Object).GetComponent<S_SnapToWorld>();
			if ((Object)(object)component != (Object)null)
			{
				Object.Destroy((Object)(object)component);
			}
			S_SnapToGrid component2 = ((Component)lE_Object).GetComponent<S_SnapToGrid>();
			if ((Object)(object)component2 != (Object)null)
			{
				Object.Destroy((Object)(object)component2);
			}
			S_SnapToObject[] componentsInChildren = ((Component)lE_Object).GetComponentsInChildren<S_SnapToObject>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				LE_ObjectSnapPoint.DestroySnapSystem(componentsInChildren[i]);
			}
			UtilityOnDestroyHandler component3 = ((Component)lE_Object).GetComponent<UtilityOnDestroyHandler>();
			if ((Object)(object)component3 != (Object)null)
			{
				component3.DestroyWithoutHandling();
			}
		}
		OnNewObjectPlaced(lE_Object);
		return lE_Object;
	}

	private void MoveToLayer(Transform root, int layer)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		((Component)root).gameObject.layer = layer;
		foreach (Transform item in root)
		{
			Transform root2 = item;
			MoveToLayer(root2, layer);
		}
	}

	private void SelectObject(LE_Object p_object)
	{
		if ((Object)(object)m_selectedObject != (Object)null && (Object)(object)m_selectedObject != (Object)(object)p_object)
		{
			m_selectedObject.EditMode = LE_EObjectEditMode.NO_EDIT;
			m_selectedObject.IsSelected = false;
		}
		m_selectedObject = p_object;
		if ((Object)(object)m_selectedObject != (Object)null)
		{
			m_selectedObject.IsSelected = true;
		}
	}

	private bool OnObjectDrag()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		bool isObjectPlaceable = m_isObjectPlaceable;
		if (LE_EventInterface.OnObjectDragged != null)
		{
			LE_ObjectDragEvent lE_ObjectDragEvent = new LE_ObjectDragEvent(m_object, m_previewInstance, m_isObjectPlaceable, m_dragMessage, m_cursorHitInfo);
			LE_EventInterface.OnObjectDragged(this, lE_ObjectDragEvent);
			isObjectPlaceable = lE_ObjectDragEvent.IsObjectPlaceable;
			SetDragMessageInUI(lE_ObjectDragEvent.Message);
		}
		if (!isObjectPlaceable && (Object)(object)m_previewInstance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)m_previewInstance).gameObject);
		}
		return isObjectPlaceable;
	}

	private void SetDragMessageInUI()
	{
		SetDragMessageInUI(m_dragMessage);
	}

	private void SetDragMessageInUI(string p_message)
	{
		if (LE_GUIInterface.Instance.delegates.SetDraggableObjectMessage != null)
		{
			LE_GUIInterface.Instance.delegates.SetDraggableObjectMessage(m_dragMessage);
		}
	}

	private bool IsObjectDraggedInUI()
	{
		if (LE_GUIInterface.Instance.delegates.IsObjectDragged != null)
		{
			return LE_GUIInterface.Instance.delegates.IsObjectDragged();
		}
		return false;
	}

	public LE_GUI3dObject()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
	}
}
