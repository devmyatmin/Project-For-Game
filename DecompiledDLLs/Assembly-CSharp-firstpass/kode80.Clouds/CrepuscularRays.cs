using UnityEngine;

namespace kode80.Clouds;

[ExecuteInEditMode]
public class CrepuscularRays : MonoBehaviour
{
	public kode80Clouds clouds;

	public float sampleCount = 20f;

	[Range(0f, 1f)]
	public float density = 0.813f;

	[Range(0f, 1f)]
	public float decay = 1f;

	[Range(0f, 1f)]
	public float weight = 1f;

	public float exposure = 3f;

	private Material _material;

	private void OnEnable()
	{
		CreateMaterialsIfNeeded();
	}

	private void OnDisable()
	{
		DestroyMaterials();
	}

	private void Reset()
	{
		CreateMaterialsIfNeeded();
	}

	private void Start()
	{
		CreateMaterialsIfNeeded();
		clouds = Object.FindObjectOfType<kode80Clouds>();
	}

	private void Update()
	{
	}

	public void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)clouds == (Object)null)
		{
			Graphics.Blit((Texture)(object)src, dst);
			return;
		}
		CreateMaterialsIfNeeded();
		Vector3 val = clouds.targetCamera.WorldToScreenPoint(((Component)clouds.sunLight).transform.forward * -100000f);
		val.x /= clouds.targetCamera.pixelWidth;
		val.y /= clouds.targetCamera.pixelHeight;
		_material.SetTexture("_Clouds", (Texture)(object)clouds.currentFrame);
		_material.SetVector("_SunScreenSpace", Vector4.op_Implicit(val));
		_material.SetFloat("_SampleCount", sampleCount);
		_material.SetFloat("_Density", density);
		_material.SetFloat("_Decay", decay);
		_material.SetFloat("_Weight", weight);
		_material.SetFloat("_Exposure", exposure);
		Graphics.Blit((Texture)(object)src, dst, _material);
	}

	private void CreateMaterialsIfNeeded()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected Obj, but got Unknown
		if ((Object)(object)_material == (Object)null)
		{
			_material = new Material(Shader.Find("Hidden/kode80/CloudsCrepuscularRays"));
			((Object)_material).hideFlags = (HideFlags)61;
		}
	}

	private void DestroyMaterials()
	{
		Object.DestroyImmediate((Object)(object)_material);
		_material = null;
	}
}
