using System;
using UnityEngine;

namespace RootMotion;

public class CameraController : MonoBehaviour
{
	[Serializable]
	public enum UpdateMode
	{
		Update,
		FixedUpdate,
		LateUpdate,
		FixedLateUpdate
	}

	public Transform target;

	public Transform rotationSpace;

	public UpdateMode updateMode = UpdateMode.LateUpdate;

	public bool lockCursor = true;

	[Header("Position")]
	public bool smoothFollow;

	public Vector3 offset = new Vector3(0f, 1.5f, 0.5f);

	public float followSpeed = 10f;

	[Header("Rotation")]
	public float rotationSensitivity = 3.5f;

	public float yMinLimit = -20f;

	public float yMaxLimit = 80f;

	public bool rotateAlways = true;

	public bool rotateOnLeftButton;

	public bool rotateOnRightButton;

	public bool rotateOnMiddleButton;

	[Header("Distance")]
	public float distance = 10f;

	public float minDistance = 4f;

	public float maxDistance = 10f;

	public float zoomSpeed = 10f;

	public float zoomSensitivity = 1f;

	[Header("Blocking")]
	public LayerMask blockingLayers;

	public float blockingRadius = 1f;

	public float blockingSmoothTime = 0.1f;

	public float blockingOriginOffset;

	[Range(0f, 1f)]
	public float blockedOffset = 0.5f;

	private Vector3 targetDistance;

	private Vector3 position;

	private Quaternion rotation = Quaternion.identity;

	private Vector3 smoothPosition;

	private Camera cam;

	private bool fixedFrame;

	private float fixedDeltaTime;

	private Quaternion r = Quaternion.identity;

	private Vector3 lastUp;

	private float blockedDistance = 10f;

	private float blockedDistanceV;

	public float x { get; private set; }

	public float y { get; private set; }

	public float distanceTarget { get; private set; }

	private float zoomAdd
	{
		get
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis > 0f)
			{
				return 0f - zoomSensitivity;
			}
			if (axis < 0f)
			{
				return zoomSensitivity;
			}
			return 0f;
		}
	}

	public void SetAngles(Quaternion rotation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector3 eulerAngles = rotation.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
	}

	public void SetAngles(float yaw, float pitch)
	{
		x = yaw;
		y = pitch;
	}

	protected virtual void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 eulerAngles = ((Component)this).transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
		distanceTarget = distance;
		smoothPosition = ((Component)this).transform.position;
		cam = ((Component)this).GetComponent<Camera>();
		lastUp = (((Object)(object)rotationSpace != (Object)null) ? rotationSpace.up : Vector3.up);
	}

	protected virtual void Update()
	{
		if (updateMode == UpdateMode.Update)
		{
			UpdateTransform();
		}
	}

	protected virtual void FixedUpdate()
	{
		fixedFrame = true;
		fixedDeltaTime += Time.deltaTime;
		if (updateMode == UpdateMode.FixedUpdate)
		{
			UpdateTransform();
		}
	}

	protected virtual void LateUpdate()
	{
		UpdateInput();
		if (updateMode == UpdateMode.LateUpdate)
		{
			UpdateTransform();
		}
		if (updateMode == UpdateMode.FixedLateUpdate && fixedFrame)
		{
			UpdateTransform(fixedDeltaTime);
			fixedDeltaTime = 0f;
			fixedFrame = false;
		}
	}

	public void UpdateInput()
	{
		if (((Behaviour)cam).enabled)
		{
			Cursor.lockState = (CursorLockMode)(lockCursor ? 1 : 0);
			Cursor.visible = !lockCursor;
			if (rotateAlways || (rotateOnLeftButton && Input.GetMouseButton(0)) || (rotateOnRightButton && Input.GetMouseButton(1)) || (rotateOnMiddleButton && Input.GetMouseButton(2)))
			{
				x += Input.GetAxis("Mouse X") * rotationSensitivity;
				y = ClampAngle(y - Input.GetAxis("Mouse Y") * rotationSensitivity, yMinLimit, yMaxLimit);
			}
			distanceTarget = Mathf.Clamp(distanceTarget + zoomAdd, minDistance, maxDistance);
		}
	}

	public void UpdateTransform()
	{
		UpdateTransform(Time.deltaTime);
	}

	public void UpdateTransform(float deltaTime)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)cam).enabled)
		{
			return;
		}
		rotation = Quaternion.AngleAxis(x, Vector3.up) * Quaternion.AngleAxis(y, Vector3.right);
		if ((Object)(object)rotationSpace != (Object)null)
		{
			r = Quaternion.FromToRotation(lastUp, rotationSpace.up) * r;
			rotation = r * rotation;
			lastUp = rotationSpace.up;
		}
		if ((Object)(object)target != (Object)null)
		{
			distance += (distanceTarget - distance) * zoomSpeed * deltaTime;
			if (!smoothFollow)
			{
				smoothPosition = target.position;
			}
			else
			{
				smoothPosition = Vector3.Lerp(smoothPosition, target.position, deltaTime * followSpeed);
			}
			Vector3 val = smoothPosition + rotation * offset;
			Vector3 val2 = rotation * -Vector3.forward;
			if (LayerMask.op_Implicit(blockingLayers) != -1)
			{
				RaycastHit val3 = default;
				if (Physics.SphereCast(val - val2 * blockingOriginOffset, blockingRadius, val2, ref val3, blockingOriginOffset + distanceTarget - blockingRadius, LayerMask.op_Implicit(blockingLayers)))
				{
					blockedDistance = Mathf.SmoothDamp(blockedDistance, val3.distance + blockingRadius * (1f - blockedOffset) - blockingOriginOffset, ref blockedDistanceV, blockingSmoothTime);
				}
				else
				{
					blockedDistance = distanceTarget;
				}
				distance = Mathf.Min(distance, blockedDistance);
			}
			position = val + val2 * distance;
			((Component)this).transform.position = position;
		}
		((Component)this).transform.rotation = rotation;
	}

	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public CameraController()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
	}
}
