using UnityEngine;

[ExecuteInEditMode]
public class PlayerPreviewCameraScaler : MonoBehaviour
{
	private Camera _camera;

	private void Update()
	{
		if (_camera == null)
		{
			_camera = GetComponent<Camera>();
		}
		Vector2 vector = new Vector2(Screen.width, Screen.height);
		float num = Mathf.Clamp01(vector.y / vector.x);
		_camera.rect = new Rect(Mathf.Clamp01(1f - num), 0f, num, 1f);
	}
}
