using Beam;
using UnityEngine;

public class CullingSettings : MonoBehaviour
{
	[SerializeField]
	private float _interactiveObjectsCullingDistance = 40f;

	[SerializeField]
	private float _terrainObjectCullingDistance = 80f;

	private void Start()
	{
		float[] array = new float[32];
		array[Layers.INTERACTIVE_OBJECTS] = _interactiveObjectsCullingDistance;
		array[Layers.PARTICLES] = _interactiveObjectsCullingDistance;
		array[Layers.TERRAIN_OBJECTS] = _terrainObjectCullingDistance;
		array[Layers.TERRAIN_DETAILS] = 30f;
		foreach (IGameCamera allCamera in Cameras.AllCameras)
		{
			allCamera.Camera.layerCullDistances = array;
			allCamera.Camera.layerCullSpherical = true;
		}
	}
}
