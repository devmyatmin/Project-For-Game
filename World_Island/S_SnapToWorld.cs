using UnityEngine;

namespace S_SnapTools;

public class S_SnapToWorld : MonoBehaviour
{
	[SerializeField]
	private LayerMask m_snapToLayers = 1;

	[SerializeField]
	private Vector3 m_snapDirection = Vector3.down;

	[SerializeField]
	private int m_snapFrameRate = 5;

	[SerializeField]
	private bool m_isRotationSnap = true;

	[SerializeField]
	private float m_maxSnapDistanceAgainstDirection = 1000f;

	private int m_snapCounter = -1;

	public LayerMask SnapToLayers
	{
		get
		{
			return m_snapToLayers;
		}
		set
		{
			m_snapToLayers = value;
		}
	}

	public Vector3 SnapDirection
	{
		get
		{
			return m_snapDirection;
		}
		set
		{
			m_snapDirection = value;
		}
	}

	public int SnapFrameRate
	{
		get
		{
			return m_snapFrameRate;
		}
		set
		{
			m_snapFrameRate = value;
		}
	}

	public bool IsRotationSnap
	{
		get
		{
			return m_isRotationSnap;
		}
		set
		{
			m_isRotationSnap = value;
		}
	}

	public float MaxSnapDistanceAgainstDirection
	{
		get
		{
			return m_maxSnapDistanceAgainstDirection;
		}
		set
		{
			m_maxSnapDistanceAgainstDirection = value;
		}
	}

	public void DoSnap()
	{
		Vector3 origin = base.transform.position - m_snapDirection * m_maxSnapDistanceAgainstDirection;
		RaycastHit hitInfo;
		if (((1 << base.gameObject.layer) & m_snapToLayers.value) != 0)
		{
			RaycastHit[] array = Physics.RaycastAll(origin, m_snapDirection, float.PositiveInfinity, m_snapToLayers.value, QueryTriggerInteraction.Ignore);
			RaycastHit? raycastHit = null;
			float num = float.PositiveInfinity;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].transform.IsChildOf(base.transform) && num > array[i].distance)
				{
					num = array[i].distance;
					raycastHit = array[i];
				}
			}
			if (raycastHit.HasValue)
			{
				SnapToHit(raycastHit.Value);
			}
		}
		else if (Physics.Raycast(origin, m_snapDirection, out hitInfo, float.PositiveInfinity, m_snapToLayers.value, QueryTriggerInteraction.Ignore))
		{
			SnapToHit(hitInfo);
		}
	}

	private void SnapToHit(RaycastHit p_hit)
	{
		base.transform.position = p_hit.point;
		if (!m_isRotationSnap)
		{
			return;
		}
		Vector3 forward = base.transform.forward;
		base.transform.up = p_hit.normal;
		forward -= base.transform.up * Vector3.Dot(base.transform.up, forward);
		forward.Normalize();
		if (Vector3.Dot(Vector3.Cross(base.transform.forward, forward), p_hit.normal) > 0f)
		{
			float num = 0f - Vector3.Angle(base.transform.forward, forward);
			if (!float.IsNaN(num))
			{
				base.transform.Rotate(Vector3.down, num);
			}
		}
		else
		{
			float num2 = 0f - Vector3.Angle(base.transform.forward, forward);
			if (!float.IsNaN(num2))
			{
				base.transform.Rotate(Vector3.up, num2);
			}
		}
	}

	private void Update()
	{
		if (m_snapFrameRate != -1)
		{
			if (m_snapCounter <= 0)
			{
				m_snapCounter = m_snapFrameRate;
				DoSnap();
			}
			else
			{
				m_snapCounter--;
			}
		}
	}

	private static void MoveToLayer(Transform p_root, int p_layer)
	{
		p_root.gameObject.layer = p_layer;
		foreach (Transform item in p_root)
		{
			MoveToLayer(item, p_layer);
		}
	}
}
