using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RefractionCullingSettings : MonoBehaviour
{
	[SerializeField]
	private LayerMask _refractionCullingMask;

	[SerializeField]
	private float _cullingDistanceNear = 30f;

	[SerializeField]
	private float _cullingDistanceFar = 30f;

	private void Start()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		float[] array = new float[32];
		array[Layers.INTERACTIVE_OBJECTS] = _cullingDistanceNear;
		array[Layers.TERRAIN_OBJECTS] = _cullingDistanceNear;
		array[Layers.INTERACTIVE_TREES] = _cullingDistanceFar;
		array[Layers.TERRAIN] = _cullingDistanceFar;
		Camera component = ((Component)this).GetComponent<Camera>();
		component.layerCullDistances = array;
		component.cullingMask = LayerMask.op_Implicit(_refractionCullingMask);
	}
}
