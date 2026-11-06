using System;
using UnityEngine;

namespace S_SnapTools;

public class S_SnapToWorld : MonoBehaviour
{
	[SerializeField]
	private LayerMask m_snapToLayers = LayerMask.op_Implicit(1);

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
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_snapToLayers;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_snapToLayers = value;
		}
	}

	public Vector3 SnapDirection
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_snapDirection;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - m_snapDirection * m_maxSnapDistanceAgainstDirection;
		RaycastHit p_hit = default;
		if (((1 << ((Component)this).gameObject.layer) & m_snapToLayers.value) != 0)
		{
			RaycastHit[] array = Physics.RaycastAll(val, m_snapDirection, float.PositiveInfinity, m_snapToLayers.value, (QueryTriggerInteraction)1);
			Nullable val2 = null;
			float num = float.PositiveInfinity;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].transform.IsChildOf(((Component)this).transform) && num > array[i].distance)
				{
					num = array[i].distance;
					val2 = array[i];
				}
			}
			if (val2.HasValue)
			{
				SnapToHit(val2.Value);
			}
		}
		else if (Physics.Raycast(val, m_snapDirection, ref p_hit, float.PositiveInfinity, m_snapToLayers.value, (QueryTriggerInteraction)1))
		{
			SnapToHit(p_hit);
		}
	}

	private void SnapToHit(RaycastHit p_hit)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = p_hit.point;
		if (!m_isRotationSnap)
		{
			return;
		}
		Vector3 val = ((Component)this).transform.forward;
		((Component)this).transform.up = p_hit.normal;
		val -= ((Component)this).transform.up * Vector3.Dot(((Component)this).transform.up, val);
		val.Normalize();
		if (Vector3.Dot(Vector3.Cross(((Component)this).transform.forward, val), p_hit.normal) > 0f)
		{
			float num = 0f - Vector3.Angle(((Component)this).transform.forward, val);
			if (!float.IsNaN(num))
			{
				((Component)this).transform.Rotate(Vector3.down, num);
			}
		}
		else
		{
			float num2 = 0f - Vector3.Angle(((Component)this).transform.forward, val);
			if (!float.IsNaN(num2))
			{
				((Component)this).transform.Rotate(Vector3.up, num2);
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
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected Obj, but got Unknown
		((Component)p_root).gameObject.layer = p_layer;
		foreach (Transform item in p_root)
		{
			MoveToLayer(item, p_layer);
		}
	}

	public S_SnapToWorld()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
	}
}
