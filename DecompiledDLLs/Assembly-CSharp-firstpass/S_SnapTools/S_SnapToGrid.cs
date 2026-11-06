using UnityEngine;

namespace S_SnapTools;

public class S_SnapToGrid : MonoBehaviour
{
	public enum ESnapCondition
	{
		ON_UPDATE,
		WHEN_STILL
	}

	[SerializeField]
	private float m_epsilon = 0.001f;

	[SerializeField]
	private float m_maxVelocity_StillMode = 0.001f;

	[SerializeField]
	private float m_stillTimeBeforSnap_StillMode = 0.2f;

	[SerializeField]
	private Vector3 m_gridOffset = Vector3.zero;

	[SerializeField]
	private Vector3 m_gridCellSize = Vector3.one * 10f;

	[SerializeField]
	private ESnapCondition m_snapCondition;

	[SerializeField]
	private bool m_isInstantSnap;

	[SerializeField]
	private bool m_isSnapAxisX = true;

	[SerializeField]
	private bool m_isSnapAxisY = true;

	[SerializeField]
	private bool m_isSnapAxisZ = true;

	private float m_stillTime;

	private Vector3 m_lastPosition = Vector3.zero;

	public float Epsilon
	{
		get
		{
			return m_epsilon;
		}
		set
		{
			m_epsilon = value;
		}
	}

	public float MaxVelocity_StillMode
	{
		get
		{
			return m_maxVelocity_StillMode;
		}
		set
		{
			m_maxVelocity_StillMode = value;
		}
	}

	public float StillTimeBeforSnap_StillMode
	{
		get
		{
			return m_stillTimeBeforSnap_StillMode;
		}
		set
		{
			m_stillTimeBeforSnap_StillMode = value;
		}
	}

	public Vector3 GridOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_gridOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_gridOffset = value;
		}
	}

	public Vector3 GridCellSize
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_gridCellSize;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_gridCellSize = value;
		}
	}

	public ESnapCondition SnapCondition
	{
		get
		{
			return m_snapCondition;
		}
		set
		{
			m_snapCondition = value;
		}
	}

	public bool IsInstantSnap
	{
		get
		{
			return m_isInstantSnap;
		}
		set
		{
			m_isInstantSnap = value;
		}
	}

	public bool IsSnapAxisX
	{
		get
		{
			return m_isSnapAxisX;
		}
		set
		{
			m_isSnapAxisX = value;
		}
	}

	public bool IsSnapAxisY
	{
		get
		{
			return m_isSnapAxisY;
		}
		set
		{
			m_isSnapAxisY = value;
		}
	}

	public bool IsSnapAxisZ
	{
		get
		{
			return m_isSnapAxisZ;
		}
		set
		{
			m_isSnapAxisZ = value;
		}
	}

	public void DoSnap()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - m_gridOffset;
		float num = val.x / m_gridCellSize.x;
		float num2 = val.y / m_gridCellSize.y;
		float num3 = val.z / m_gridCellSize.z;
		if ((m_isSnapAxisX && num - Mathf.Floor(num) > m_epsilon) || (m_isSnapAxisY && num2 - Mathf.Floor(num2) > m_epsilon) || (m_isSnapAxisZ && num3 - Mathf.Floor(num3) > m_epsilon))
		{
			if (m_isSnapAxisX)
			{
				num = Mathf.Round(num);
			}
			if (m_isSnapAxisY)
			{
				num2 = Mathf.Round(num2);
			}
			if (m_isSnapAxisZ)
			{
				num3 = Mathf.Round(num3);
			}
			Vector3 val2 = m_gridOffset + Vector3.Scale(new Vector3(num, num2, num3), m_gridCellSize);
			if (m_isInstantSnap)
			{
				((Component)this).transform.position = val2;
			}
			else
			{
				((Component)this).transform.position = Vector3.Lerp(((Component)this).transform.position, val2, 0.5f);
			}
			m_lastPosition = ((Component)this).transform.position;
		}
	}

	private void Start()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (m_gridCellSize.x == 0f || m_gridCellSize.y == 0f || m_gridCellSize.z == 0f)
		{
			m_gridCellSize += Vector3.one * 0.1f;
			Debug.LogError((object)"S_SnapToGrid: Start: All components of the GridCellSize vector must be unequal 0!");
		}
	}

	private void LateUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_snapCondition == ESnapCondition.ON_UPDATE;
		if (!flag)
		{
			Vector3 val = m_lastPosition - ((Component)this).transform.position;
			if (val.magnitude / Time.deltaTime < m_maxVelocity_StillMode)
			{
				m_stillTime += Time.deltaTime;
			}
			else
			{
				m_stillTime = 0f;
			}
			m_lastPosition = ((Component)this).transform.position;
			flag = m_stillTime >= m_stillTimeBeforSnap_StillMode;
		}
		if (flag)
		{
			DoSnap();
		}
	}

	public S_SnapToGrid()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
	}
}
