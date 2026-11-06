using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Camera/ReflectionCameraCullingDistances")]
[RequireComponent(typeof(Camera))]
public class ReflectionCameraCullingDistances : MonoBehaviour
{
	public bool sphericalCulling = true;

	public float[] distances = new float[32];

	private Camera m_camera;

	private void Start()
	{
		m_camera = ((Component)this).GetComponent<Camera>();
	}

	private void Update()
	{
		if (!((Object)(object)Ocean.Instance == (Object)null) && distances.Length == 32)
		{
			CameraData cameraData = Ocean.Instance.FindCameraData(m_camera);
			if (cameraData.reflection != null)
			{
				Camera cam = cameraData.reflection.cam;
				cam.layerCullDistances = distances;
				cam.layerCullSpherical = sphericalCulling;
			}
		}
	}
}
