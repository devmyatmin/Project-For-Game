using UnityEngine;

[ExecuteInEditMode]
public class FogVolume : MonoBehaviour
{
	[SerializeField]
	private Shader _shader;

	[HideInInspector]
	public Material FogMaterial;

	public Vector3 fogVolumeScale = new Vector3(20f, 20f, 20f);

	public float Visibility = 5f;

	[Header("Lighting")]
	[SerializeField]
	private Light Sun;

	private MeshRenderer _renderer;

	private MeshFilter filter;

	private Mesh mesh;

	private Vector3 currentFogVolume = Vector3.one;

	private MaterialPropertyBlock _propertyBlock;

	public float GetVisibility()
	{
		return Visibility;
	}

	private void CreateMaterial()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		FogMaterial = new Material(_shader);
		((Object)FogMaterial).name = "Fog Material";
		((Object)FogMaterial).hideFlags = (HideFlags)61;
	}

	public void SetFade(float value)
	{
		((Renderer)_renderer).GetPropertyBlock(_propertyBlock);
		_propertyBlock.SetFloat("_Fade", Mathf.Clamp01(value));
		((Renderer)_renderer).SetPropertyBlock(_propertyBlock);
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		CreateMaterial();
		_propertyBlock = new MaterialPropertyBlock();
		_renderer = ((Component)this).GetComponent<MeshRenderer>();
		((Renderer)_renderer).sharedMaterial = FogMaterial;
		SetFade(1f);
		filter = ((Component)this).gameObject.GetComponent<MeshFilter>();
		if ((Object)(object)filter == (Object)null)
		{
			CreateBoxMesh(((Component)this).transform.localScale);
			((Component)this).transform.localScale = Vector3.one;
		}
		UpdateBoxMesh();
	}

	private void Update()
	{
	}

	private void OnWillRenderObject()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Sun))
		{
			FogMaterial.SetColor("_LightColor", Sun.color * Sun.intensity);
		}
		FogMaterial.SetVector("VolumeSize", new Vector4(fogVolumeScale.x, fogVolumeScale.y, fogVolumeScale.z, 0f));
		Vector3 val = currentFogVolume;
		FogMaterial.SetVector("_BoxMin", Vector4.op_Implicit(val * -0.5f));
		FogMaterial.SetVector("_BoxMax", Vector4.op_Implicit(val * 0.5f));
		FogMaterial.SetFloat("_Visibility", Visibility);
	}

	private void UpdateBoxMesh()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (currentFogVolume != fogVolumeScale || (Object)(object)filter == (Object)null)
		{
			CreateBoxMesh(fogVolumeScale);
		}
		((Component)this).transform.localScale = Vector3.one;
	}

	private void CreateBoxMesh(Vector3 scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		currentFogVolume = scale;
		if ((Object)(object)filter == (Object)null)
		{
			filter = ((Component)this).gameObject.AddComponent<MeshFilter>();
		}
		if ((Object)(object)mesh == (Object)null)
		{
			mesh = new Mesh();
			((Object)mesh).name = ((Object)((Component)this).gameObject).name;
			filter.sharedMesh = mesh;
		}
		mesh.Clear();
		float y = scale.y;
		float z = scale.z;
		float x = scale.x;
		Vector3 val = new Vector3((0f - x) * 0.5f, (0f - y) * 0.5f, z * 0.5f);
		Vector3 val2 = new Vector3(x * 0.5f, (0f - y) * 0.5f, z * 0.5f);
		Vector3 val3 = new Vector3(x * 0.5f, (0f - y) * 0.5f, (0f - z) * 0.5f);
		Vector3 val4 = new Vector3((0f - x) * 0.5f, (0f - y) * 0.5f, (0f - z) * 0.5f);
		Vector3 val5 = new Vector3((0f - x) * 0.5f, y * 0.5f, z * 0.5f);
		Vector3 val6 = new Vector3(x * 0.5f, y * 0.5f, z * 0.5f);
		Vector3 val7 = new Vector3(x * 0.5f, y * 0.5f, (0f - z) * 0.5f);
		Vector3 val8 = new Vector3((0f - x) * 0.5f, y * 0.5f, (0f - z) * 0.5f);
		Vector3[] vertices = new Vector3[24]
		{
			val, val2, val3, val4, val8, val5, val, val4, val5, val6,
			val2, val, val7, val8, val4, val3, val6, val7, val3, val2,
			val8, val7, val6, val5
		};
		int[] triangles = new int[36]
		{
			3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
			6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
			12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
			23, 21, 20, 23, 22, 21
		};
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
	}

	public FogVolume()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
	}
}
