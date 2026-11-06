using UnityEngine;

public class ExtendedFlycam : MonoBehaviour
{
	public float cameraSensitivity = 90f;

	public float climbSpeed = 4f;

	public float normalMoveSpeed = 10f;

	public float slowMoveFactor = 0.25f;

	public float fastMoveFactor = 3f;

	private float rotationX;

	private float rotationY;

	private void Start()
	{
		CursorUtils.LockState = (CursorLockMode)1;
		CursorUtils.Visible = false;
	}

	private void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		if ((int)CursorUtils.LockState != 0)
		{
			rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
			rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
		}
		rotationY = Mathf.Clamp(rotationY, -90f, 90f);
		Quaternion val = Quaternion.AngleAxis(rotationX, Vector3.up);
		val *= Quaternion.AngleAxis(rotationY, Vector3.left);
		((Component)this).transform.localRotation = Quaternion.Lerp(((Component)this).transform.localRotation, val, Time.deltaTime * 5f);
		if (Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303))
		{
			Transform transform = ((Component)this).transform;
			transform.position += ((Component)this).transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
			Transform transform2 = ((Component)this).transform;
			transform2.position += ((Component)this).transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
			if (Input.GetKey((KeyCode)113))
			{
				Transform transform3 = ((Component)this).transform;
				transform3.position += Vector3.up * climbSpeed * fastMoveFactor * Time.deltaTime;
			}
			if (Input.GetKey((KeyCode)101))
			{
				Transform transform4 = ((Component)this).transform;
				transform4.position -= Vector3.up * climbSpeed * fastMoveFactor * Time.deltaTime;
			}
		}
		else if (Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305))
		{
			Transform transform5 = ((Component)this).transform;
			transform5.position += ((Component)this).transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
			Transform transform6 = ((Component)this).transform;
			transform6.position += ((Component)this).transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
			if (Input.GetKey((KeyCode)113))
			{
				Transform transform7 = ((Component)this).transform;
				transform7.position += Vector3.up * climbSpeed * slowMoveFactor * Time.deltaTime;
			}
			if (Input.GetKey((KeyCode)101))
			{
				Transform transform8 = ((Component)this).transform;
				transform8.position -= Vector3.up * climbSpeed * slowMoveFactor * Time.deltaTime;
			}
		}
		else
		{
			Transform transform9 = ((Component)this).transform;
			transform9.position += ((Component)this).transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
			Transform transform10 = ((Component)this).transform;
			transform10.position += ((Component)this).transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
			if (Input.GetKey((KeyCode)113))
			{
				Transform transform11 = ((Component)this).transform;
				transform11.position += Vector3.up * climbSpeed * Time.deltaTime;
			}
			if (Input.GetKey((KeyCode)101))
			{
				Transform transform12 = ((Component)this).transform;
				transform12.position -= Vector3.up * climbSpeed * Time.deltaTime;
			}
		}
		if (Input.GetKeyDown((KeyCode)279) || Input.GetKeyDown((KeyCode)27))
		{
			if ((int)CursorUtils.LockState == 0)
			{
				CursorUtils.LockState = (CursorLockMode)1;
				CursorUtils.Visible = false;
			}
			else
			{
				CursorUtils.LockState = (CursorLockMode)0;
				CursorUtils.Visible = true;
			}
		}
	}
}
