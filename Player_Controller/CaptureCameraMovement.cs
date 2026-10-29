using System.Runtime.CompilerServices;
using Beam.Utilities;
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
		RewiredUtilities.Players.FirstOrDefault_NonAlloc()?.DisableMaps();
	}

	private void OnDisable()
	{
		Cameras.RemoveCamera(this);
		Object.FindObjectOfType<kode80Clouds>().SetCamera(Cameras.Main);
		RewiredUtilities.Players.FirstOrDefault_NonAlloc()?.RestoreMapsEnabled();
	}

	private void UpdateSpeed()
	{
		m_speed = 20f;
		m_pivotSpeed = 20f;
		if (Input.GetKey(KeyCode.LeftShift))
		{
			m_speed *= 2f;
			m_pivotSpeed *= 2f;
		}
		bool key = Input.GetKey(KeyCode.RightControl);
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
		Vector3 localPosition = Camera.transform.localPosition;
		if (Input.GetKey(KeyCode.I))
		{
			localPosition += Vector3.forward * m_speed * Time.unscaledDeltaTime;
		}
		if (Input.GetKey(KeyCode.K))
		{
			localPosition -= Vector3.forward * m_speed * Time.unscaledDeltaTime;
		}
		if (Input.GetKey(KeyCode.O))
		{
			localPosition += Vector3.up * m_speed * 0.5f * Time.unscaledDeltaTime;
		}
		if (Input.GetKey(KeyCode.U))
		{
			localPosition -= Vector3.up * m_speed * 0.5f * Time.unscaledDeltaTime;
		}
		Camera.transform.localPosition = localPosition;
		if (Input.GetKey(KeyCode.P) || Vector3.Dot(base.transform.forward, (base.transform.position - Camera.transform.position).normalized) < 0f)
		{
			Camera.transform.localPosition = Vector3.zero;
			Camera.transform.forward = base.transform.forward;
		}
	}

	private void DoPivot()
	{
		float num = 0f;
		if (Input.GetKey(KeyCode.J))
		{
			num += m_pivotSpeed * Time.unscaledDeltaTime;
		}
		if (Input.GetKey(KeyCode.L))
		{
			num -= m_pivotSpeed * Time.unscaledDeltaTime;
		}
		m_angle = num;
		base.transform.Rotate(Vector3.up, m_angle);
		Vector3 vector = base.transform.position - Camera.transform.position;
		Camera.transform.rotation = Quaternion.Lerp(Quaternion.LookRotation(base.transform.forward), Quaternion.LookRotation(vector.normalized), Mathf.Clamp01(vector.magnitude));
	}

	private void DoMovement()
	{
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		Vector3 direction = new Vector3(axis, 0f, axis2);
		direction = base.transform.TransformDirection(direction);
		if (Input.GetKey(KeyCode.E))
		{
			direction += Vector3.up;
		}
		else if (Input.GetKey(KeyCode.Q))
		{
			direction += -Vector3.up;
		}
		direction *= m_speed;
		base.transform.position += direction * Time.unscaledDeltaTime;
	}

	private void Update()
	{
		UpdateSpeed();
		UpdateCameraPosition();
		DoPivot();
		DoMovement();
	}

	[SpecialName]
	GameObject IGameCamera.get_gameObject()
	{
		return base.gameObject;
	}
}
