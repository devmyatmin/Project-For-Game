using UnityEngine;

namespace kode80.Clouds.Examples;

public class FlyCamera : MonoBehaviour
{
	public float lookSpeed = 0.2f;

	public float moveSpeed = 15f;

	public float runMultiplier = 4f;

	private float _rotationX;

	private float _rotationY;

	private Vector3 _targetPosition;

	private Vector3 _movement;

	private bool _isRunning;

	private Vector2 _mousePrev;

	private Vector2 _mouseDelta;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		_targetPosition = ((Component)this).transform.position;
		_rotationX = ((Component)this).transform.localEulerAngles.y;
		_rotationY = ((Component)this).transform.localEulerAngles.x;
	}

	private void Update()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		UpdateInput();
		_rotationX += _mouseDelta.x * Time.deltaTime * lookSpeed;
		_rotationY += _mouseDelta.y * Time.deltaTime * lookSpeed;
		_rotationY = Mathf.Clamp(_rotationY, -90f, 90f);
		((Component)this).transform.localRotation = Quaternion.AngleAxis(_rotationX, Vector3.up);
		Transform transform = ((Component)this).transform;
		transform.localRotation *= Quaternion.AngleAxis(_rotationY, Vector3.left);
		float num = (_isRunning ? runMultiplier : 1f);
		_targetPosition += ((Component)this).transform.forward * moveSpeed * num * Time.deltaTime * _movement.z;
		_targetPosition += ((Component)this).transform.right * moveSpeed * num * Time.deltaTime * _movement.x;
		_targetPosition += ((Component)this).transform.up * moveSpeed * num * Time.deltaTime * _movement.y;
		((Component)this).transform.position = Vector3.Lerp(((Component)this).transform.position, _targetPosition, 0.5f);
	}

	private void UpdateInput()
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKey((KeyCode)119))
		{
			_movement.z = 1f;
		}
		else if (Input.GetKey((KeyCode)115))
		{
			_movement.z = -1f;
		}
		else
		{
			_movement.z = 0f;
		}
		if (Input.GetKey((KeyCode)100))
		{
			_movement.x = 1f;
		}
		else if (Input.GetKey((KeyCode)97))
		{
			_movement.x = -1f;
		}
		else
		{
			_movement.x = 0f;
		}
		if (Input.GetKey((KeyCode)101))
		{
			_movement.y = 1f;
		}
		else if (Input.GetKey((KeyCode)113))
		{
			_movement.y = -1f;
		}
		else
		{
			_movement.y = 0f;
		}
		_isRunning = Input.GetKey((KeyCode)304);
		Vector2 val = Vector2.op_Implicit(Input.mousePosition);
		if (Input.GetMouseButtonDown(0))
		{
			_mousePrev = val;
		}
		else if (Input.GetMouseButton(0))
		{
			_mouseDelta = val - _mousePrev;
		}
		else
		{
			_mouseDelta = Vector2.zero;
		}
	}
}
