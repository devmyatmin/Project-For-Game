using System;
using UnityEngine;

public class UILookAtCamera : MonoBehaviour
{
	private void OnEnable()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(Camera_OnPreRender));
	}

	private void OnDisable()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(Camera_OnPreRender));
	}

	private void Camera_OnPreRender(Camera camera)
	{
		if (!(camera == null) && (camera.CompareTag("MainCamera") || camera.CompareTag("PlayerCamera")))
		{
			Vector3 vector = camera.transform.position - base.transform.position;
			vector.x = 0f;
			vector.z = 0f;
			base.transform.LookAt(camera.transform.position - vector);
		}
	}
}
