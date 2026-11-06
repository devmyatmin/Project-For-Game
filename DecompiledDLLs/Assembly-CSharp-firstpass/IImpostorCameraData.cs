using UnityEngine;

public interface IImpostorCameraData
{
	Camera Camera { get; }

	void Update();

	bool IsVisible(Bounds bounds);

	bool IsVisibleInCache(Bounds bounds);
}
