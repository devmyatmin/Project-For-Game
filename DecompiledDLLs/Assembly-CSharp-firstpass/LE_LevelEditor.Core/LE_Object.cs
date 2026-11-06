using System;
using LE_LevelEditor.Events;
using UnityEngine;

namespace LE_LevelEditor.Core;

public class LE_Object : MonoBehaviour
{
	public enum Categories
	{
		NONE,
		[Description("MAP_EDITOR_OBJECTS_CATEGORY_ITEMS", "")]
		ITEMS,
		[Description("MAP_EDITOR_OBJECTS_CATEGORY_TREES", "")]
		TREES,
		[Description("MAP_EDITOR_OBJECTS_CATEGORY_ROCKS", "")]
		ROCKS,
		[Description("MAP_EDITOR_OBJECTS_CATEGORY_ANIMALS", "")]
		ANIMALS,
		[Description("MAP_EDITOR_OBJECTS_CATEGORY_STRUCTURES", "")]
		STRUCTURES,
		[Description("MAP_EDITOR_OBJECTS_CATEGORY_MISC", "")]
		MISC
	}

	public enum ESnapType
	{
		SNAP_DISABLED,
		SNAP_TO_TERRAIN,
		SNAP_TO_OBJECT,
		SNAP_TO_3D_GRID,
		SNAP_TO_2D_GRID_AND_TERRAIN
	}

	public uint Id;

	public Categories Category;

	public string EditorObjectName;

	[SerializeField]
	private int _densityWeight = 1;

	private const float OUTLINE_INV_WIDTH = 300f;

	private static int s_nextFreeUID = 1;

	[SerializeField]
	private string m_iconPath = "";

	[SerializeField]
	private bool m_isPlacementRotationByNormal = true;

	[SerializeField]
	private bool m_isMovable = true;

	[SerializeField]
	private bool m_isMovableOnX = true;

	[SerializeField]
	private bool m_isMovableOnY = true;

	[SerializeField]
	private bool m_isMovableOnZ = true;

	[SerializeField]
	private bool m_isRotatable = true;

	[SerializeField]
	private bool m_isRotatableAroundX = true;

	[SerializeField]
	private bool m_isRotatableAroundY = true;

	[SerializeField]
	private bool m_isRotatableAroundZ = true;

	[SerializeField]
	private int m_maxInstancesInLevel;

	[SerializeField]
	private ESnapType m_snapType;

	[SerializeField]
	private int m_rootSnapPointIndex = -1;

	[SerializeField]
	private LE_ObjectSnapPoint[] m_objectSnapPoints = new LE_ObjectSnapPoint[0];

	[SerializeField]
	private bool m_isDrawSnapToObjectUI = true;

	[SerializeField]
	private Vector3 m_snapGridOffset = Vector3.zero;

	[SerializeField]
	private Vector3 m_snapGridCellSize = Vector3.one * 10f;

	private int m_UID = s_nextFreeUID++;

	private bool m_isSelectedChanged;

	private bool m_isSelected;

	private LE_EObjectEditMode m_editMode;

	private LE_ObjectEditHandle m_editHandle;

	public int DensityWeight
	{
		get
		{
			return _densityWeight;
		}
		set
		{
			_densityWeight = value;
		}
	}

	[HideInInspector]
	public string IconPath
	{
		get
		{
			return m_iconPath;
		}
		set
		{
			m_iconPath = value;
		}
	}

	public bool IsPlacementRotationByNormal
	{
		get
		{
			return m_isPlacementRotationByNormal;
		}
		set
		{
			m_isPlacementRotationByNormal = value;
		}
	}

	public bool IsMovable
	{
		get
		{
			return m_isMovable;
		}
		set
		{
			m_isMovable = value;
		}
	}

	public bool IsMovableOnX
	{
		get
		{
			return m_isMovableOnX;
		}
		set
		{
			m_isMovableOnX = value;
		}
	}

	public bool IsMovableOnY
	{
		get
		{
			return m_isMovableOnY;
		}
		set
		{
			m_isMovableOnY = value;
		}
	}

	public bool IsMovableOnZ
	{
		get
		{
			return m_isMovableOnZ;
		}
		set
		{
			m_isMovableOnZ = value;
		}
	}

	public bool IsRotatable
	{
		get
		{
			return m_isRotatable;
		}
		set
		{
			m_isRotatable = value;
		}
	}

	public bool IsRotatableAroundX
	{
		get
		{
			return m_isRotatableAroundX;
		}
		set
		{
			m_isRotatableAroundX = value;
		}
	}

	public bool IsRotatableAroundY
	{
		get
		{
			return m_isRotatableAroundY;
		}
		set
		{
			m_isRotatableAroundY = value;
		}
	}

	public bool IsRotatableAroundZ
	{
		get
		{
			return m_isRotatableAroundZ;
		}
		set
		{
			m_isRotatableAroundZ = value;
		}
	}

	public int MaxInstancesInLevel
	{
		get
		{
			return m_maxInstancesInLevel;
		}
		set
		{
			m_maxInstancesInLevel = value;
		}
	}

	public ESnapType SnapType
	{
		get
		{
			return m_snapType;
		}
		set
		{
			m_snapType = value;
		}
	}

	public int RootSnapPointIndex
	{
		get
		{
			return m_rootSnapPointIndex;
		}
		set
		{
			m_rootSnapPointIndex = value;
		}
	}

	public LE_ObjectSnapPoint[] ObjectSnapPoints
	{
		get
		{
			return m_objectSnapPoints;
		}
		set
		{
			m_objectSnapPoints = value;
		}
	}

	public bool IsDrawSnapToObjectUI
	{
		get
		{
			return m_isDrawSnapToObjectUI;
		}
		set
		{
			m_isDrawSnapToObjectUI = value;
		}
	}

	public Vector3 SnapGridOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_snapGridOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_snapGridOffset = value;
		}
	}

	public Vector3 SnapGridCellSize
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_snapGridCellSize;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_snapGridCellSize = value;
		}
	}

	public int UID
	{
		get
		{
			return m_UID;
		}
		set
		{
			m_UID = value;
		}
	}

	public bool IsSelected
	{
		get
		{
			return m_isSelected;
		}
		set
		{
			if (m_isSelected != value)
			{
				m_isSelected = value;
				m_isSelectedChanged = true;
			}
		}
	}

	public LE_EObjectEditMode EditMode
	{
		get
		{
			return m_editMode;
		}
		set
		{
			m_editMode = value;
		}
	}

	public static void ReportUsedUID(int p_usedUID)
	{
		s_nextFreeUID = Mathf.Max(s_nextFreeUID, p_usedUID + 1);
	}

	public void SetupReferences(LE_ObjectReference le)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		Category = le.Category;
		EditorObjectName = le.EditorObjectName;
		_densityWeight = le.DensityWeight;
		m_iconPath = le.IconPath;
		m_isPlacementRotationByNormal = le.IsPlacementRotationByNormal;
		m_isMovable = le.IsMovable;
		m_isMovableOnX = le.IsMovableOnX;
		m_isMovableOnY = le.IsMovableOnY;
		m_isMovableOnZ = le.IsMovableOnZ;
		m_isRotatable = le.IsRotatable;
		m_isRotatableAroundX = le.IsRotatableAroundX;
		m_isRotatableAroundY = le.IsRotatableAroundY;
		m_isRotatableAroundZ = le.IsRotatableAroundZ;
		m_maxInstancesInLevel = le.MaxInstancesInLevel;
		m_snapType = le.SnapType;
		m_rootSnapPointIndex = le.RootSnapPointIndex;
		m_objectSnapPoints = le.ObjectSnapPoints;
		m_isDrawSnapToObjectUI = le.IsDrawSnapToObjectUI;
		m_snapGridOffset = le.SnapGridOffset;
		m_snapGridCellSize = le.SnapGridCellSize;
	}

	private void Update()
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected Obj, but got Unknown
		if (m_isSelectedChanged)
		{
			m_isSelectedChanged = false;
		}
		if (m_editMode == LE_EObjectEditMode.NO_EDIT)
		{
			if ((Object)(object)m_editHandle != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)m_editHandle).gameObject);
			}
		}
		else
		{
			if (!((Object)(object)m_editHandle == (Object)null) && m_editHandle.EditMode == m_editMode)
			{
				return;
			}
			if ((Object)(object)m_editHandle != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)m_editHandle).gameObject);
			}
			if ((m_editMode != LE_EObjectEditMode.MOVE || !m_isMovable || (!m_isMovableOnX && !m_isMovableOnY && !m_isMovableOnZ)) && (m_editMode != LE_EObjectEditMode.ROTATE || !m_isRotatable || (!m_isRotatableAroundX && !m_isRotatableAroundY && !m_isRotatableAroundZ)))
			{
				return;
			}
			string text = m_editMode.ToString();
			GameObject val = (GameObject)Object.Instantiate(Resources.Load("ObjectEditHandle" + text), ((Component)this).transform.position, ((Component)this).transform.rotation);
			val.transform.parent = ((Component)this).transform;
			m_editHandle = val.GetComponent<LE_ObjectEditHandle>();
			LE_ObjectEditHandle editHandle = m_editHandle;
			editHandle.m_onTransformed = (EventHandler)Delegate.Combine(editHandle.m_onTransformed, (EventHandler)((object p_obj, EventArgs p_args) =>
			{
				LE_EventInterface.LevelDataChanged(m_editHandle);
			}));
			switch (m_editMode)
			{
			case LE_EObjectEditMode.MOVE:
				if (!m_isMovableOnX)
				{
					m_editHandle.DisableAxisX();
				}
				if (!m_isMovableOnY)
				{
					m_editHandle.DisableAxisY();
				}
				if (!m_isMovableOnZ)
				{
					m_editHandle.DisableAxisZ();
				}
				break;
			case LE_EObjectEditMode.ROTATE:
				if (!m_isRotatableAroundX)
				{
					m_editHandle.DisableAxisX();
				}
				if (!m_isRotatableAroundY)
				{
					m_editHandle.DisableAxisY();
				}
				if (!m_isRotatableAroundZ)
				{
					m_editHandle.DisableAxisZ();
				}
				break;
			}
		}
	}

	public LE_Object()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
	}
}
