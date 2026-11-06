using UnityEngine;

public class TiltPanel : MonoBehaviour
{
	public Vector2 range = new Vector2(5f, 3f);

	private Transform mTrans;

	private Quaternion mStart;

	private Vector2 mRot = Vector2.zero;

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		mTrans = ((Component)this).transform;
		mStart = mTrans.localRotation;
	}

	private void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		Vector3 mousePosition = Input.mousePosition;
		float num = (float)Screen.width * 0.5f;
		float num2 = (float)Screen.height * 0.5f;
		float num3 = Mathf.Clamp((mousePosition.x - num) / num, -1f, 1f);
		float num4 = Mathf.Clamp((mousePosition.y - num2) / num2, -1f, 1f);
		mRot = Vector2.Lerp(mRot, new Vector2(num3, num4), Time.deltaTime * 5f);
		mTrans.localRotation = mStart * Quaternion.Euler((0f - mRot.y) * range.y, mRot.x * range.x, 0f);
	}

	public TiltPanel()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
	}
}
