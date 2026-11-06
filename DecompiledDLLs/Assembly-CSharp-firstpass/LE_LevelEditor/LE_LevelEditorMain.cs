using System;
using System.Collections.Generic;
using System.Linq;
using Beam;
using Beam.Utilities.Reflection;
using CPG_CameraPerspective;
using LE_LevelEditor.Core;
using LE_LevelEditor.Events;
using LE_LevelEditor.LEInput;
using LE_LevelEditor.Logic;
using LE_LevelEditor.UI;
using UnityEngine;

namespace LE_LevelEditor;

public class LE_LevelEditorMain : Singleton<LE_LevelEditorMain>, LE_IInputHandler
{
	private enum EEstimateDistanceMode
	{
		AVERAGE,
		NEAREST
	}

	public static int MAP_EDITOR_SEED;

	[SerializeField]
	private GameObject _screenShotCamera;

	public List<Texture2D> MapScreenshots = new List<Texture2D>();

	public List<LE_Object> MapObjectReferences = new List<LE_Object>();

	public GameObject Boundary;

	public Transform ObjectDumpContainer;

	public Transform ObjectInstanceContainer;

	[SerializeField]
	[Tooltip("Show/hide camera perspective gizmo. Works like the built-in Unity Editor scene view handle: click on the axes to change camera's position and direction; click in the middle to toggle between perspective and orthographic camera view.")]
	private bool IS_WITH_CAMERA_PERSPECTIVE_GIZMO = true;

	[SerializeField]
	[Tooltip("Camera perspective gizmo's render layer index. This layer should be excluded from the main camera.")]
	private int CAMERA_PERSPECTIVE_GIZMO_LAYER = 14;

	[SerializeField]
	[Tooltip("Root object map shown in the object editor's tree browser.")]
	private LE_ObjectMap ROOT_OBJECT_MAP;

	[SerializeField]
	[Tooltip("Enable to bring focused objects to the center of the visible screen (not hidden by right menu). On low resolution devices the right menu could use half of the screen. Without the oblique projection only the left half of a big object would be visible, because the right half would be behind the menu.")]
	private bool IS_OBLIQUE_FOCUS_CENTERING = true;

	[SerializeField]
	[Tooltip("If enabled, then the camera will be controlled by the level editor as described in the help popup.")]
	private bool IS_CAMERA_MOVEMENT = true;

	[SerializeField]
	[Tooltip("If enabled, then the terrain editor logic will be initialized and terrain related events handled.")]
	private bool IS_TERRAIN_EDITOR = true;

	[SerializeField]
	[Tooltip("If enabled, then the object editor logic will be initialized and object related events handled.")]
	private bool IS_OBJECT_EDITOR = true;

	[SerializeField]
	[LE_EnumFlags("Flag enum containing activated key combinations. For example if DUPLICATE is picked then objects will be cloned when Ctrl+D is typed.")]
	private LE_EKeyCombo ACTIVE_KEY_COMBOS = LE_EKeyCombo.FOCUS | LE_EKeyCombo.DUPLICATE;

	private LE_EEditMode m_editMode;

	private List<LE_GUI3dBase> m_GUI3d = new List<LE_GUI3dBase>();

	private LE_GUI3dTerrain m_GUI3dTerrain;

	private LE_GUI3dObject m_GUI3dObject;

	private List<LE_LogicBase> m_logic = new List<LE_LogicBase>();

	private LE_LogicTerrain m_logicTerrain;

	private LE_LogicLevel m_logicLevel;

	private LE_LogicObjects m_LogicObjects;

	private LE_Input m_input;

	private CPG_CameraPerspectiveGizmo m_cameraPerspectiveGizmo;

	private Vector3 m_camPivot = Vector3.zero;

	private Camera m_cam;

	private Rect m_perspectiveGizmoRect;

	private bool m_isFirstUpdateInitialized;

	private bool m_isSecondUpdateInitialized;

	private Action m_executeWhenReady;

	private int m_lastChangeFrame = -1;

	public LE_EEditMode EditMode => m_editMode;

	public LE_GUI3dTerrain GUI3dTerrain => m_GUI3dTerrain;

	public Vector3 CamPivot
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_camPivot;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_camPivot = value;
		}
	}

	private Camera Cam
	{
		get
		{
			if ((Object)(object)m_cam == (Object)null)
			{
				m_cam = Camera.main;
			}
			return m_cam;
		}
	}

	public bool IsReady => m_isFirstUpdateInitialized;

	public int LastChangeFrame => m_lastChangeFrame;

	public void ExecuteWhenReady(Action p_action)
	{
		m_executeWhenReady = (Action)Delegate.Combine(m_executeWhenReady, p_action);
	}

	public static bool SetObliqueFocusProjectionMatrix(bool p_isObliqueProjectionEnabled)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (LE_GUIInterface.Instance.delegates.GetObliqueCameraPerspectiveRightPixelOffset != null)
		{
			if (p_isObliqueProjectionEnabled)
			{
				Camera.main.ResetProjectionMatrix();
				Matrix4x4 projectionMatrix = Camera.main.projectionMatrix;
				projectionMatrix[0, 2] = LE_GUIInterface.Instance.delegates.GetObliqueCameraPerspectiveRightPixelOffset() / (float)Screen.width;
				Camera.main.projectionMatrix = projectionMatrix;
			}
			else
			{
				Camera.main.ResetProjectionMatrix();
			}
			return true;
		}
		return false;
	}

	void LE_IInputHandler.SetCursorPosition(Vector3 p_cursorScreenCoords)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_GUI3d.Count; i++)
		{
			if (m_GUI3d[i].ActiveEditMode == m_editMode && m_GUI3d[i].IsInteractable)
			{
				m_GUI3d[i].SetCursorPosition(p_cursorScreenCoords);
			}
		}
	}

	void LE_IInputHandler.SetIsCursorAction(bool p_isCursorAction)
	{
		for (int i = 0; i < m_GUI3d.Count; i++)
		{
			if (m_GUI3d[i].ActiveEditMode == m_editMode)
			{
				m_GUI3d[i].SetIsCursorAction(p_isCursorAction);
			}
		}
	}

	void LE_IInputHandler.MoveCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords, bool scrolling = false)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		if (!IS_CAMERA_MOVEMENT)
		{
			return;
		}
		Camera cam = Cam;
		if ((!scrolling || !IsMouseOverGUI()) && (Object)(object)cam != (Object)null)
		{
			float num = EstimateDistanceToLevel(EEstimateDistanceMode.AVERAGE);
			Vector3 val = p_toScreenCoords - p_fromScreenCoords;
			val = ((Component)cam).transform.TransformDirection(val);
			Vector3 val2;
			if (cam.orthographic)
			{
				float num2 = Vector3.Dot(val, ((Component)cam).transform.forward);
				val -= ((Component)cam).transform.forward * num2;
				cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - num2 * (cam.orthographicSize / (float)Screen.width), 5f, 1000f);
				val2 = val * (cam.orthographicSize / (float)Screen.width);
			}
			else
			{
				val2 = val * (num / (float)Screen.width);
			}
			Transform transform = ((Component)cam).transform;
			transform.position += val2;
			((Component)cam).transform.position = new Vector3(Mathf.Clamp(((Component)cam).transform.position.x, -300f, 300f), Mathf.Clamp(((Component)cam).transform.position.y, -100f, 150f), Mathf.Clamp(((Component)cam).transform.position.z, -300f, 300f));
		}
	}

	void LE_IInputHandler.RotateCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		if (!IS_CAMERA_MOVEMENT)
		{
			return;
		}
		Camera cam = Cam;
		if ((Object)(object)cam != (Object)null)
		{
			Ray val = cam.ScreenPointToRay(p_fromScreenCoords);
			Ray val2 = cam.ScreenPointToRay(p_toScreenCoords);
			Vector3 val3;
			if (cam.orthographic)
			{
				val3 = val.origin + val.direction * 100f - ((Component)cam).transform.position;
				val.direction = val3.normalized;
				val3 = val2.origin + val2.direction * 100f - ((Component)cam).transform.position;
				val2.direction = val3.normalized;
			}
			float num = Vector3.Angle(val.direction, val2.direction);
			val3 = Vector3.Cross(val.direction, val2.direction);
			Vector3 val4 = val3.normalized;
			float num2 = Mathf.Abs(((Component)cam).transform.forward.y);
			if (num2 > 0.9f)
			{
				num2 = (1f - num2) / 0.100000024f;
				num2 *= num2;
				float num3 = Vector3.Dot(val4, ((Component)cam).transform.right);
				val4 -= (1f - num2) * (val4 - num3 * ((Component)cam).transform.right);
			}
			((Component)cam).transform.Rotate(val4, 0f - num, (Space)0);
			if (((Component)cam).transform.up.y > 0f)
			{
				((Component)cam).transform.LookAt(((Component)cam).transform.position + ((Component)cam).transform.forward, Vector3.up);
			}
			else
			{
				((Component)cam).transform.LookAt(((Component)cam).transform.position + ((Component)cam).transform.forward, Vector3.down);
			}
		}
	}

	void LE_IInputHandler.RotateCameraAroundPivot(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (!IS_CAMERA_MOVEMENT)
		{
			return;
		}
		Camera cam = Cam;
		if ((Object)(object)cam != (Object)null)
		{
			Vector2 val = new Vector2(p_toScreenCoords.x, p_toScreenCoords.y) - new Vector2(p_fromScreenCoords.x, p_fromScreenCoords.y);
			val.x /= Screen.width;
			val.y /= Screen.height;
			((Component)cam).transform.RotateAround(m_camPivot, ((Component)cam).transform.right, val.y * 300f);
			((Component)cam).transform.RotateAround(m_camPivot, ((Component)cam).transform.up, (0f - val.x) * 300f);
			if (((Component)cam).transform.up.y > 0f)
			{
				((Component)cam).transform.LookAt(m_camPivot, Vector3.up);
			}
			else
			{
				((Component)cam).transform.LookAt(m_camPivot, Vector3.down);
			}
		}
	}

	private void Start()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)LE_GUIInterface.Instance == (Object)null)
		{
			Debug.LogError((object)"LE_LevelEditorMain: a LE_GUIInterface object must be added to the scene!");
		}
		LE_ConfigTerrain lE_ConfigTerrain = ((Component)this).GetComponentInChildren<LE_ConfigTerrain>();
		if ((Object)(object)lE_ConfigTerrain == (Object)null)
		{
			Debug.LogError((object)"LE_LevelEditorMain: a LE_ConfigTerrain component must be added to the game object with the LE_LevelEditorMain script!");
			lE_ConfigTerrain = ((Component)this).gameObject.AddComponent<LE_ConfigTerrain>();
		}
		if (!IS_TERRAIN_EDITOR)
		{
			if (lE_ConfigTerrain.Brushes.Length != 0)
			{
				Debug.LogWarning((object)"LE_LevelEditorMain: IS_TERRAIN_EDITOR is set to 'false', but you have provided a non empty array for LE_ConfigTerrain.Brushes! This is performance waist and will increase loading time! Remove brushes or reenable terrain editing.");
			}
			if ((Object)(object)lE_ConfigTerrain.BrushProjector != (Object)null)
			{
				Debug.LogWarning((object)"LE_LevelEditorMain: IS_TERRAIN_EDITOR is set to 'false', but you have provided a value for LE_ConfigTerrain.BrushProjector! This is performance waist and will increase loading time! Remove brush projector from scene or reenable terrain editing.");
			}
		}
		Camera cam = Cam;
		if ((Object)(object)cam == (Object)null)
		{
			Debug.LogError((object)"LE_LevelEditorMain: Start: could not find main camera!");
		}
		if (IS_OBJECT_EDITOR)
		{
			m_GUI3dObject = ((Component)this).gameObject.AddComponent<LE_GUI3dObject>();
			m_GUI3dObject.TERRAIN_LAYER = 14;
			LE_GUI3dObject gUI3dObject = m_GUI3dObject;
			gUI3dObject.OnFocused = (Action<Vector3, float>)Delegate.Combine(gUI3dObject.OnFocused, new Action<Vector3, float>(OnGUI3dObjectFocused));
			m_GUI3dObject.IsKeyComboFocus = (ACTIVE_KEY_COMBOS & LE_EKeyCombo.FOCUS) != 0;
			m_GUI3dObject.IsKeyComboDuplicate = (ACTIVE_KEY_COMBOS & LE_EKeyCombo.DUPLICATE) != 0;
			m_GUI3d.Add(m_GUI3dObject);
		}
		m_input = new LE_Input(this);
		m_camPivot = ((Component)cam).transform.position + ((Component)cam).transform.forward * 100f;
		LE_EventInterface.OnChangeLevelData = (EventHandler)Delegate.Combine(LE_EventInterface.OnChangeLevelData, new EventHandler(OnChangeLevelData));
		LE_GUIInterface.EventHandlers events = LE_GUIInterface.Instance.events;
		events.OnEditModeBtn = (EventHandler<LE_GUIInterface.EventHandlers.EditModeEventArgs>)Delegate.Combine(events.OnEditModeBtn, new EventHandler<LE_GUIInterface.EventHandlers.EditModeEventArgs>(OnEditModeBtn));
		MAP_EDITOR_SEED = StrandedWorld.GetRandowWorldSeed();
	}

	private void Initialize_InFirstUpdate()
	{
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		Camera cam = Cam;
		LE_ConfigTerrain componentInChildren = ((Component)this).GetComponentInChildren<LE_ConfigTerrain>();
		if (LE_GUIInterface.Instance.delegates.IsCursorOverUI == null)
		{
			Debug.LogError((object)"LE_LevelEditorMain: you have not setup LE_GUIInterface.delegates.IsCursorOverUI. Terrain might be edited behind UI while the user interacts with the UI, same is true for object placement!");
		}
		if (IS_TERRAIN_EDITOR)
		{
			m_GUI3dTerrain = ((Component)this).gameObject.AddComponent<LE_GUI3dTerrain>();
			m_GUI3d.Add(m_GUI3dTerrain);
			InitializeDefaultTerrain(componentInChildren);
			m_logicTerrain = new LE_LogicTerrain(componentInChildren, m_GUI3dTerrain);
			m_logic.Add(m_logicTerrain);
		}
		else if ((Object)(object)componentInChildren.CustomDefaultTerrain != (Object)null)
		{
			Debug.LogWarning((object)"LE_LevelEditorMain: IS_TERRAIN_EDITOR is set to 'false', but you have provided a value for LE_ConfigTerrain.CustomDefaultTerrain! The value will be ignored!");
		}
		m_logicLevel = new LE_LogicLevel(m_GUI3dTerrain, m_GUI3dObject, IS_OBLIQUE_FOCUS_CENTERING);
		m_logic.Add(m_logicLevel);
		if (IS_OBJECT_EDITOR)
		{
			if (ROOT_OBJECT_MAP == null)
			{
				InitializeObjects();
			}
			m_LogicObjects = new LE_LogicObjects(m_GUI3dObject, ROOT_OBJECT_MAP);
			m_logic.Add(m_LogicObjects);
		}
		if ((Object)(object)cam != (Object)null && IS_WITH_CAMERA_PERSPECTIVE_GIZMO)
		{
			if (LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset != null)
			{
				float num = 0.2f * (float)Screen.width / cam.aspect;
				float num2 = 0.2f * (float)Screen.height;
				m_perspectiveGizmoRect = new Rect((float)Screen.width - LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset() - num, 0f, num, num2);
				if (string.IsNullOrEmpty(LayerMask.LayerToName(CAMERA_PERSPECTIVE_GIZMO_LAYER)))
				{
					Debug.LogWarning((object)("LE_LevelEditorMain: Inspector property 'CAMERA_PERSPECTIVE_GIZMO_LAYER' is set to '" + CAMERA_PERSPECTIVE_GIZMO_LAYER + "', but this layer has no name in 'Tags & Layers' set. Please set the name of this layer to 'CameraPerspectiveGizmo' or change the value of the 'CAMERA_PERSPECTIVE_GIZMO_LAYER' property! To open the 'Tags & Layers' manager select any game object, in the inspector click on the layer drop down at top right then click on 'Add Layer...'."));
				}
				else if (LayerMask.LayerToName(CAMERA_PERSPECTIVE_GIZMO_LAYER) != "CameraPerspectiveGizmo")
				{
					Debug.LogWarning((object)("LE_LevelEditorMain: Inspector property 'CAMERA_PERSPECTIVE_GIZMO_LAYER' is set to '" + CAMERA_PERSPECTIVE_GIZMO_LAYER + "', but the name of this layer is '" + LayerMask.LayerToName(CAMERA_PERSPECTIVE_GIZMO_LAYER) + "'. Is this intended? If not please set the name of this layer to 'CameraPerspectiveGizmo' or change the value of the 'CAMERA_PERSPECTIVE_GIZMO_LAYER' property! To open the 'Tags & Layers' manager select any game object, in the inspector click on the layer drop down at top right then click on 'Add Layer...'."));
				}
				m_cameraPerspectiveGizmo = CPG_CameraPerspectiveGizmo.Create(cam, CAMERA_PERSPECTIVE_GIZMO_LAYER);
				if ((Object)(object)m_cameraPerspectiveGizmo != (Object)null)
				{
					((Component)m_cameraPerspectiveGizmo).transform.position = Vector3.down * -10000f;
					m_cameraPerspectiveGizmo.OrthoOffset = cam.farClipPlane * 0.2f;
					cam.cullingMask &= ~(1 << CAMERA_PERSPECTIVE_GIZMO_LAYER);
					m_cameraPerspectiveGizmo.RelativeScreenSize = 0.2f;
					float num3 = LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset() / (float)Screen.width;
					m_cameraPerspectiveGizmo.RelativeScreenPos = new Vector2(1f - 0.1f / cam.aspect - num3, 0.9f);
					CPG_CameraPerspectiveGizmo cameraPerspectiveGizmo = m_cameraPerspectiveGizmo;
					cameraPerspectiveGizmo.m_onBeforeSwitchToOrthographic = (EventHandler)Delegate.Combine(cameraPerspectiveGizmo.m_onBeforeSwitchToOrthographic, new EventHandler(OnCameraPerspectiveSwitchToOrthographic));
					CPG_CameraPerspectiveGizmo cameraPerspectiveGizmo2 = m_cameraPerspectiveGizmo;
					cameraPerspectiveGizmo2.m_onAfterSwitchToPerspective = (EventHandler)Delegate.Combine(cameraPerspectiveGizmo2.m_onAfterSwitchToPerspective, new EventHandler(OnCameraPerspectiveSwitchToPerspective));
				}
			}
			else
			{
				Debug.LogError((object)"LE_LevelEditorMain: LE_GUIInterface.delegates.GetCameraPerspectiveGizmoRightPixelOffset was not set from the UI scripts! The camera perspective gizmo cannot be placed to the right screen position! To set this delegate use 'LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset = ...' in one of the Start functions of your scripts!");
			}
		}
		if (IS_OBLIQUE_FOCUS_CENTERING && !SetObliqueFocusProjectionMatrix(p_isObliqueProjectionEnabled: true))
		{
			Debug.LogError((object)"LE_LevelEditorMain: IS_OBLIQUE_FOCUS_CENTERING is true, but you have not provided the LE_GUIInterface.delegates.GetObliqueCameraPerspectiveRightPixelOffset delegate! Provide it to bring focused objects in the center of the visible (not covered by UI) screen area!");
		}
	}

	public void OnObjectEditModeKey(LE_EObjectEditMode editMode)
	{
		m_GUI3dObject.ObjectEditMode = editMode;
	}

	public void OnSelectedObjectDeleteKey()
	{
		if ((Object)(object)m_GUI3dObject.SelectedObject != (Object)null)
		{
			if (LE_GUIInterface.Instance.delegates.ShowPopupConfirmDeleteObject != null)
			{
				m_LogicObjects.ConfirmDelete();
			}
			else
			{
				Debug.LogError((object)"LE_LogicObject: OnSelectedObjectDeleteBtn: you have triggered the delete button behaviour, but the LE_GUIInterface.delegates.ShowPopupConfirmDeleteObject delegate is not set! Set this delegate to make the delete button work!");
			}
		}
	}

	public void OnSelectedObjectDuplicateKey()
	{
		m_GUI3dObject.CloneObject();
	}

	public void InitializeObjects()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"LevelEditorMain:: Initializing Editor Objects");
		List<LE_ObjectMap> list = new List<LE_ObjectMap>();
		foreach (LE_Object.Categories value in Enum.GetValues(typeof(LE_Object.Categories)))
		{
			if (value != LE_Object.Categories.NONE)
			{
				LE_ObjectMap lE_ObjectMap = new LE_ObjectMap();
				lE_ObjectMap.CategoryName = Reflection.GetAttribute<DescriptionAttribute>(value).Text;
				lE_ObjectMap.ObjectPrefabs = new List<LE_Object>();
				list.Add(lE_ObjectMap);
			}
		}
		ROOT_OBJECT_MAP = new LE_ObjectMap();
		ROOT_OBJECT_MAP.ObjectPrefabs = new List<LE_Object>();
		Shader.SetGlobalColor("_HighlightRimColor", Color.black);
		Shader.SetGlobalFloat("_HighlightRimPower", 1f);
		LE_Object[] componentsInChildren = ((Component)ObjectInstanceContainer).GetComponentsInChildren<LE_Object>();
		foreach (LE_Object item in componentsInChildren)
		{
			MapObjectReferences.Add(item);
		}
		((Component)ObjectInstanceContainer).gameObject.SetActive(false);
		foreach (LE_Object mapObjectReference in MapObjectReferences)
		{
			if (mapObjectReference.Category == LE_Object.Categories.NONE)
			{
				ROOT_OBJECT_MAP.ObjectPrefabs.Add(mapObjectReference);
			}
			else
			{
				list[(int)(mapObjectReference.Category - 1)].ObjectPrefabs.Add(mapObjectReference);
			}
		}
		foreach (LE_ObjectMap item2 in list)
		{
			item2.ObjectPrefabs = item2.ObjectPrefabs.OrderBy((LE_Object go) => ((Object)((Component)go).gameObject).name).ToList();
			ROOT_OBJECT_MAP.SubObjectMaps.Add(item2);
		}
	}

	private void Validate_References()
	{
	}

	private void Initialize_InSecondUpdate()
	{
		if (IS_OBLIQUE_FOCUS_CENTERING)
		{
			Camera cam = Cam;
			if ((Object)(object)cam != (Object)null)
			{
				SetObliqueFocusProjectionMatrix(!cam.orthographic);
			}
		}
	}

	private void InitializeDefaultTerrain(LE_ConfigTerrain p_LEConfTerrain)
	{
		if ((Object)(object)p_LEConfTerrain.CustomDefaultTerrain != (Object)null && (Object)(object)p_LEConfTerrain.CustomDefaultTerrain.terrainData != (Object)null)
		{
			m_GUI3dTerrain.SetTerrain(p_LEConfTerrain.CustomDefaultTerrain);
			LE_EventInterface.LevelDataChanged(((Component)p_LEConfTerrain.CustomDefaultTerrain).gameObject);
			if (LE_GUIInterface.Instance.delegates.SetTerrainUIMode != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainUIMode(LE_GUIInterface.Delegates.ETerrainUIMode.EDIT);
			}
			else
			{
				Debug.LogWarning((object)"LE_LevelEditorMain: you have not set the LE_GUIInterface.delegates.SetTerrainUIMode delegate. You need to set it for example if you want to disable the create UI if the default Unity terrain is set!");
			}
		}
		else if (LE_GUIInterface.Instance.delegates.SetTerrainUIMode != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainUIMode(LE_GUIInterface.Delegates.ETerrainUIMode.CREATE);
		}
		else
		{
			Debug.LogWarning((object)"LE_LevelEditorMain: you have not set the LE_GUIInterface.delegates.SetTerrainUIMode delegate. You need to set it for example if you want to show the create UI if there is no default Unity terrain set!");
		}
	}

	private void Update()
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (m_isFirstUpdateInitialized && !m_isSecondUpdateInitialized)
		{
			m_isSecondUpdateInitialized = true;
			Initialize_InSecondUpdate();
		}
		if (!m_isFirstUpdateInitialized)
		{
			m_isFirstUpdateInitialized = true;
			Initialize_InFirstUpdate();
		}
		bool flag = IsMouseOverGUI();
		for (int i = 0; i < m_GUI3d.Count; i++)
		{
			m_GUI3d[i].IsInteractable = !flag && m_GUI3d[i].ActiveEditMode == m_editMode;
		}
		for (int j = 0; j < m_logic.Count; j++)
		{
			m_logic[j].Update();
		}
		m_input.Update();
		UpdateCameraPivotPoint();
		if ((Object)(object)m_cameraPerspectiveGizmo != (Object)null)
		{
			m_cameraPerspectiveGizmo.Pivot = m_camPivot;
		}
		if (m_executeWhenReady != null && IsReady)
		{
			m_executeWhenReady();
			m_executeWhenReady = null;
		}
	}

	public bool IsMouseOverGUI()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		bool flag = LE_GUIInterface.Instance.delegates.IsCursorOverUI != null && LE_GUIInterface.Instance.delegates.IsCursorOverUI();
		bool flag2 = false;
		if (IS_WITH_CAMERA_PERSPECTIVE_GIZMO && !flag)
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.y = Mathf.Clamp((float)Screen.height - mousePosition.y, 0f, (float)Screen.height);
			mousePosition.x = Mathf.Clamp(mousePosition.x, 0f, (float)Screen.width);
			flag2 = m_perspectiveGizmoRect.Contains(mousePosition);
		}
		return flag | flag2;
	}

	private void OnGUI3dObjectFocused(Vector3 p_focusOn, float p_distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		m_camPivot = p_focusOn;
		Camera cam = Cam;
		if ((Object)(object)cam != (Object)null && cam.orthographic && (Object)(object)m_cameraPerspectiveGizmo != (Object)null)
		{
			((Component)cam).transform.position = m_camPivot - ((Component)cam).transform.forward * Mathf.Abs(m_cameraPerspectiveGizmo.OrthoOffset + p_distance);
			cam.orthographicSize = p_distance;
		}
	}

	protected override void OnDestroy()
	{
		LE_EventInterface.OnChangeLevelData = (EventHandler)Delegate.Remove(LE_EventInterface.OnChangeLevelData, new EventHandler(OnChangeLevelData));
		if ((Object)(object)LE_GUIInterface.Instance != (Object)null)
		{
			LE_GUIInterface.EventHandlers events = LE_GUIInterface.Instance.events;
			events.OnEditModeBtn = (EventHandler<LE_GUIInterface.EventHandlers.EditModeEventArgs>)Delegate.Remove(events.OnEditModeBtn, new EventHandler<LE_GUIInterface.EventHandlers.EditModeEventArgs>(OnEditModeBtn));
		}
		for (int i = 0; i < m_GUI3d.Count; i++)
		{
			if (m_GUI3d[i] is LE_GUI3dObject)
			{
				((LE_GUI3dObject)m_GUI3d[i]).OnFocused = null;
			}
		}
		m_executeWhenReady = null;
		if (m_input != null)
		{
			m_input.Destroy();
		}
		for (int j = 0; j < m_logic.Count; j++)
		{
			m_logic[j].Destroy();
		}
		base.OnDestroy();
	}

	private void OnChangeLevelData(object p_sender, EventArgs p_args)
	{
		m_lastChangeFrame = Time.frameCount;
	}

	private void OnEditModeBtn(object p_sender, LE_GUIInterface.EventHandlers.EditModeEventArgs p_args)
	{
		m_editMode = p_args.EditMode;
		if (m_editMode != LE_EEditMode.TERRAIN && (Object)(object)m_GUI3dTerrain != (Object)null)
		{
			m_GUI3dTerrain.HideCursor();
		}
		if (m_editMode != LE_EEditMode.OBJECT && (Object)(object)m_GUI3dObject != (Object)null)
		{
			m_GUI3dObject.RemoveSelection();
		}
		if (m_editMode != LE_EEditMode.NONE)
		{
			_screenShotCamera.SetActive(false);
		}
		else
		{
			_screenShotCamera.SetActive(true);
		}
	}

	private void OnCameraPerspectiveSwitchToOrthographic(object p_sender, EventArgs p_args)
	{
		if (IS_OBLIQUE_FOCUS_CENTERING)
		{
			SetObliqueFocusProjectionMatrix(p_isObliqueProjectionEnabled: false);
		}
		if ((Object)(object)m_cameraPerspectiveGizmo != (Object)null)
		{
			float num = EstimateDistanceToLevel(EEstimateDistanceMode.NEAREST);
			Camera cam = Cam;
			if ((Object)(object)cam != (Object)null)
			{
				cam.orthographicSize = Mathf.Clamp(num, 5f, 60f);
			}
		}
	}

	private void OnCameraPerspectiveSwitchToPerspective(object p_sender, EventArgs p_args)
	{
		if (IS_OBLIQUE_FOCUS_CENTERING)
		{
			SetObliqueFocusProjectionMatrix(p_isObliqueProjectionEnabled: true);
		}
	}

	private void UpdateCameraPivotPoint()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		Camera cam = Cam;
		if ((Object)(object)cam != (Object)null)
		{
			Vector3 val = m_camPivot - ((Component)cam).transform.position;
			float num = Vector3.Dot(((Component)cam).transform.forward, val);
			if (num < 1f)
			{
				float num2 = num - 1.5f;
				m_camPivot = ((Component)cam).transform.position - ((Component)cam).transform.forward * num2;
				val = m_camPivot - ((Component)cam).transform.position;
				num = Vector3.Dot(((Component)cam).transform.forward, val);
			}
			Vector3 val2 = Vector3.Cross(((Component)cam).transform.forward, val);
			if (val2.magnitude > 1f)
			{
				m_camPivot = ((Component)cam).transform.position + ((Component)cam).transform.forward * num;
			}
		}
	}

	private float EstimateDistanceToLevel(EEstimateDistanceMode p_mode)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		float result = 300f;
		Camera cam = Cam;
		if ((Object)(object)cam != (Object)null)
		{
			float num = 0f;
			Vector3 val = Vector3.zero;
			float num2 = float.MaxValue;
			RaycastHit val2 = default;
			Vector3 val3;
			for (float num3 = 0f; num3 < 2f; num3++)
			{
				for (float num4 = 0f; num4 < 2f; num4++)
				{
					Rect rect = cam.rect;
					float num5 = rect.width * (float)Screen.width * (0.25f + 0.5f * num3);
					rect = cam.rect;
					if (!Physics.Raycast(cam.ScreenPointToRay(new Vector3(num5, rect.height * (float)Screen.height * (0.25f + 0.5f * num4), 0f)), ref val2))
					{
						continue;
					}
					num++;
					if (p_mode == EEstimateDistanceMode.AVERAGE)
					{
						val += val2.point;
						continue;
					}
					val3 = val2.point - ((Component)cam).transform.position;
					float magnitude = val3.magnitude;
					if (num2 > magnitude)
					{
						num2 = magnitude;
						val = val2.point;
					}
				}
			}
			if (num > 0f)
			{
				if (p_mode == EEstimateDistanceMode.AVERAGE)
				{
					val /= num;
				}
				val3 = val - ((Component)cam).transform.position;
				result = val3.magnitude;
			}
		}
		return result;
	}

	public void ClearLevel()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in ObjectDumpContainer)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
	}

	public LE_LevelEditorMain()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
	}
}
