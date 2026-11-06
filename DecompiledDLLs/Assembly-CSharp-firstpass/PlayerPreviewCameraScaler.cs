using UnityEngine;

[ExecuteInEditMode]
public class PlayerPreviewCameraScaler : MonoBehaviour
{
	private Camera _camera;

	private void Update()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_camera == (Object)null)
		{
			_camera = ((Component)this).GetComponent<Camera>();
		}
		Vector2 val = new Vector2((float)Screen.width, (float)Screen.height);
		float num = Mathf.Clamp01(val.y / val.x);
		_camera.rect = new Rect(Mathf.Clamp01(1f - num), 0f, num, 1f);
	}
}
