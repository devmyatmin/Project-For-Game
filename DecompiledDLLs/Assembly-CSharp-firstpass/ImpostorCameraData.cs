using Beam.Utilities;
using UnityEngine;

public class ImpostorCameraData : IImpostorCameraData
{
	private readonly Camera _camera;

	private readonly Camera _cachingCamera;

	public Camera Camera => _camera;

	public Camera CachingCamera => _cachingCamera;

	public Plane[] CameraPlanes { get; } = new Plane[6];

	public Plane[] CachingCameraPlanes { get; } = new Plane[6];

	public ImpostorCameraData(Camera camera, Camera cachingCamera)
	{
		_camera = camera;
		_cachingCamera = cachingCamera;
	}

	public void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		BeamGeometryUtility.CalculateFrustumPlanes(CameraPlanes, Camera.projectionMatrix * Camera.worldToCameraMatrix);
		BeamGeometryUtility.CalculateFrustumPlanes(CachingCameraPlanes, CachingCamera.projectionMatrix * CachingCamera.worldToCameraMatrix);
	}

	public bool IsVisible(Bounds bounds)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GeometryUtility.TestPlanesAABB(CameraPlanes, bounds);
	}

	public bool IsVisibleInCache(Bounds bounds)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GeometryUtility.TestPlanesAABB(CachingCameraPlanes, bounds);
	}
}
