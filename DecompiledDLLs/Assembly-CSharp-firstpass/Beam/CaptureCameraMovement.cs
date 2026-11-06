using Beam.Utilities;
using Rewired;
using UnityEngine;
using kode80.Clouds;

namespace Beam;

public class CaptureCameraMovement : MonoBehaviour, IGameCamera
{
	private const float MOVEMENT_SPEED = 20f;

	private const float PIVOT_SPEED = 20f;

	[SerializeField]
	private Camera m_camera;

	public int SpeedIncrement = 2;

	private float m_speed = 20f;

	private float m_pivotSpeed = 20f;

	private float m_angle;

	public Camera Camera => m_camera;

	public int Id { get; set; }

	private void OnEnable()
	{
		Cameras.AddCamera(this);
		Object.FindObjectOfType<kode80Clouds>().SetCamera(m_camera);
		Player val = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(RewiredUtilities.Players);
		if (val != null)
		{
			RewiredUtilities.DisableMaps(val);
		}
	}

	private void OnDisable()
	{
		Cameras.RemoveCamera(this);
		Object.FindObjectOfType<kode80Clouds>().SetCamera(Cameras.Main);
		Player val = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(RewiredUtilities.Players);
		if (val != null)
		{
			RewiredUtilities.RestoreMapsEnabled(val);
		}
	}

	private void UpdateSpeed()
	{
		m_speed = 20f;
		m_pivotSpeed = 20f;
		if (Input.GetKey((KeyCode)304))
		{
			m_speed *= 2f;
			m_pivotSpeed *= 2f;
		}
		bool key = Input.GetKey((KeyCode)305);
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			if (key)
			{
				m_pivotSpeed -= SpeedIncrement;
				m_pivotSpeed = Mathf.Max(m_pivotSpeed, 0f);
			}
			else
			{
				m_speed -= SpeedIncrement;
				m_speed = Mathf.Max(m_speed, 0f);
			}
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			if (key)
			{
				m_pivotSpeed += SpeedIncrement;
			}
			else
			{
				m_speed += SpeedIncrement;
			}
		}
	}

	private void UpdateCameraPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)Camera).transform.localPosition;
		if (Input.GetKey((KeyCode)105))
		{
			val += Vector3.forward * m_speed * Time.unscaledDeltaTime;
		}
		if (Input.GetKey((KeyCode)107))
		{
			val -= Vector3.forward * m_speed * Time.unscaledDeltaTime;
		}
		if (Input.GetKey((KeyCode)111))
		{
			val += Vector3.up * m_speed * 0.5f * Time.unscaledDeltaTime;
		}
		if (Input.GetKey((KeyCode)117))
		{
			val -= Vector3.up * m_speed * 0.5f * Time.unscaledDeltaTime;
		}
		((Component)Camera).transform.localPosition = val;
		if (!Input.GetKey((KeyCode)112))
		{
			Vector3 forward = ((Component)this).transform.forward;
			Vector3 val2 = ((Component)this).transform.position - ((Component)Camera).transform.position;
			if (!(Vector3.Dot(forward, val2.normalized) < 0f))
			{
				return;
			}
		}
		((Component)Camera).transform.localPosition = Vector3.zero;
		((Component)Camera).transform.forward = ((Component)this).transform.forward;
	}

	private void DoPivot()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if (Input.GetKey((KeyCode)106))
		{
			num += m_pivotSpeed * Time.unscaledDeltaTime;
		}
		if (Input.GetKey((KeyCode)108))
		{
			num -= m_pivotSpeed * Time.unscaledDeltaTime;
		}
		m_angle = num;
		((Component)this).transform.Rotate(Vector3.up, m_angle);
		Vector3 val = ((Component)this).transform.position - ((Component)Camera).transform.position;
		((Component)Camera).transform.rotation = Quaternion.Lerp(Quaternion.LookRotation(((Component)this).transform.forward), Quaternion.LookRotation(val.normalized), Mathf.Clamp01(val.magnitude));
	}

	private void DoMovement()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		Vector3 val = new Vector3(axis, 0f, axis2);
		val = ((Component)this).transform.TransformDirection(val);
		if (Input.GetKey((KeyCode)101))
		{
			val += Vector3.up;
		}
		else if (Input.GetKey((KeyCode)113))
		{
			val += -Vector3.up;
		}
		val *= m_speed;
		Transform transform = ((Component)this).transform;
		transform.position += val * Time.unscaledDeltaTime;
	}

	private void Update()
	{
		UpdateSpeed();
		UpdateCameraPosition();
		DoPivot();
		DoMovement();
	}
}
