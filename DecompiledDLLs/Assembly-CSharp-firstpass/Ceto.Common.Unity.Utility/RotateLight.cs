using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public class RotateLight : MonoBehaviour
{
	public float speed = 50f;

	public Vector3 axis = new Vector3(1f, 0f, 0f);

	public KeyCode decrementKey = (KeyCode)269;

	public KeyCode incrementKey = (KeyCode)270;

	private void Start()
	{
	}

	private void Update()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.deltaTime * speed;
		Vector3 val = new Vector3(num, num, num);
		if (Input.GetKey(decrementKey))
		{
			val.x *= 0f - axis.x;
			val.y *= 0f - axis.y;
			val.z *= 0f - axis.z;
			((Component)this).transform.Rotate(val);
		}
		if (Input.GetKey(incrementKey))
		{
			val.x *= axis.x;
			val.y *= axis.y;
			val.z *= axis.z;
			((Component)this).transform.Rotate(val);
		}
	}

	public RotateLight()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
	}
}
