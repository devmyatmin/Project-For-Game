using System;
using UnityEngine;

namespace Ceto;

public class ShipCamera : MonoBehaviour
{
	private struct Position
	{
		public Vector3 forwardAmount;

		public float turnAmount;

		public Vector2 camRotation;

		public float camDistance;
	}

	public bool disableInterpolation;

	public GameObject m_ship;

	public Vector3 m_forward = new Vector3(0f, 0f, 1f);

	public float m_shipMoveSpeed = 20f;

	[Range(0.01f, 1f)]
	public float shipSmoothness = 0.5f;

	public float m_camRotationSpeed = 10f;

	public float m_camStartRotationX = 180f;

	public float m_camStartRotationY = 60f;

	public float m_camStartDistance = 100f;

	[Range(0.01f, 1f)]
	public float camSmoothness = 0.5f;

	private Position m_position;

	private Position m_target;

	private const float MAX_ACCELERATION = 1f;

	private const float ACCELERATION_RATE = 1f;

	private const float DECELERATION_RATE = 0.25f;

	private float m_acceleration;

	private Vector3 m_previousPos;

	private Vector3 m_velocity;

	private GameObject m_dummy;

	private GameObject Ship
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected Obj, but got Unknown
			if ((Object)(object)m_ship == (Object)null)
			{
				m_dummy = new GameObject();
				m_ship = m_dummy;
			}
			return m_ship;
		}
	}

	private void Start()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		m_position.camRotation.x = m_camStartRotationX;
		m_position.camRotation.y = m_camStartRotationY;
		m_position.camDistance = m_camStartDistance;
		m_position.forwardAmount = Vector3.zero;
		m_target = m_position;
		m_previousPos = Ship.transform.position;
	}

	private void LateUpdate()
	{
		ProcessInput();
		InterpolateToTarget();
		MoveShip();
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_dummy != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)m_dummy);
		}
	}

	private void MoveShip()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = Ship.transform;
		transform.position += m_position.forwardAmount;
		Vector3 eulerAngles = Ship.transform.eulerAngles;
		eulerAngles.y += m_position.turnAmount;
		Ship.transform.eulerAngles = eulerAngles;
		float num = Mathf.Cos(m_position.camRotation.y * ((float)Math.PI / 180f));
		float num2 = Mathf.Sin(m_position.camRotation.y * ((float)Math.PI / 180f));
		float num3 = Mathf.Cos(m_position.camRotation.x * ((float)Math.PI / 180f));
		float num4 = Mathf.Sin(m_position.camRotation.x * ((float)Math.PI / 180f));
		Vector3 position = Ship.transform.position;
		Vector3 position2 = position + new Vector3(num4 * num2, num, num3 * num2) * m_position.camDistance;
		((Component)this).transform.position = position2;
		((Component)this).transform.LookAt(position);
		m_velocity = Ship.transform.position - m_previousPos;
		m_previousPos = Ship.transform.position;
	}

	private void InterpolateToTarget()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (disableInterpolation || Time.timeScale == 0f)
		{
			m_position = m_target;
			return;
		}
		float num = 1f / Mathf.Clamp(camSmoothness, 0.01f, 1f);
		float num2 = Mathf.Clamp01(Time.deltaTime * num);
		num = 1f / Mathf.Clamp(shipSmoothness, 0.01f, 1f);
		float num3 = Mathf.Clamp01(Time.deltaTime * num);
		m_position.camDistance = Mathf.Lerp(m_position.camDistance, m_target.camDistance, num2);
		m_position.camRotation = Vector2.Lerp(m_position.camRotation, m_target.camRotation, num2);
		m_position.forwardAmount = Vector3.Lerp(m_position.forwardAmount, m_target.forwardAmount, num3);
		m_position.turnAmount = Mathf.Lerp(m_position.turnAmount, m_target.turnAmount, num3);
	}

	private void ProcessInput()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		float shipMoveSpeed = m_shipMoveSpeed;
		float magnitude = m_velocity.magnitude;
		m_target.forwardAmount = Vector3.zero;
		m_target.turnAmount = 0f;
		if (Input.GetKey((KeyCode)97))
		{
			float num = shipMoveSpeed * magnitude * 2f;
			m_target.turnAmount -= num * Time.deltaTime;
			m_target.camRotation.x -= num * Time.deltaTime;
		}
		if (Input.GetKey((KeyCode)100))
		{
			float num2 = shipMoveSpeed * magnitude * 2f;
			m_target.turnAmount += num2 * Time.deltaTime;
			m_target.camRotation.x += num2 * Time.deltaTime;
		}
		Vector3 val = Vector4.op_Implicit(Ship.transform.localToWorldMatrix * Vector4.op_Implicit(m_forward));
		val.Normalize();
		if (Input.GetKey((KeyCode)119))
		{
			m_acceleration += Time.deltaTime * 1f;
		}
		else
		{
			m_acceleration -= Time.deltaTime * 0.25f;
		}
		m_acceleration = Mathf.Clamp(m_acceleration, 0f, 1f);
		ref Vector3 forwardAmount = ref m_target.forwardAmount;
		forwardAmount += val * shipMoveSpeed * m_acceleration * Time.deltaTime;
		float num3 = Time.deltaTime * 1000f;
		float num4 = Mathf.Pow(1.02f, Mathf.Min(num3, 1f));
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			m_target.camDistance *= num4;
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			m_target.camDistance /= num4;
		}
		m_target.camDistance = Mathf.Max(1f, m_target.camDistance);
		m_target.camRotation.y = Mathf.Clamp(m_target.camRotation.y, 20f, 160f);
		if (Input.GetMouseButton(0))
		{
			m_target.camRotation.y += Input.GetAxis("Mouse Y") * m_camRotationSpeed;
			m_target.camRotation.x += Input.GetAxis("Mouse X") * m_camRotationSpeed;
		}
	}

	public ShipCamera()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}
}
