using UnityEngine;

public class AlwaysCameraData : IImpostorCameraData
{
	private readonly Camera _camera;

	public Camera Camera => _camera;

	public AlwaysCameraData(Camera camera)
	{
		_camera = camera;
	}

	public void Update()
	{
	}

	public bool IsVisible(Bounds bounds)
	{
		return true;
	}

	public bool IsVisibleInCache(Bounds bounds)
	{
		return true;
	}
}
