using System;
using UnityEngine;

namespace kode80.Clouds;

[ExecuteInEditMode]
public class FullScreenQuad : MonoBehaviour
{
	public Material material;

	public bool renderWhenPlaying;

	public Camera targetCamera;

	private Mesh _mesh;

	private MeshFilter _filter;

	private MeshRenderer _renderer;

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		_mesh = new Mesh();
		((Object)_mesh).hideFlags = (HideFlags)61;
		Vector3 val = new Vector3(-1f, -1f, 1f);
		Vector3 val2 = new Vector3(1f, -1f, 1f);
		Vector3 val3 = new Vector3(1f, 1f, 1f);
		Vector3 val4 = new Vector3(-1f, 1f, 1f);
		Vector2 val5 = new Vector2(0f, 0f);
		Vector2 val6 = new Vector2(1f, 0f);
		Vector2 val7 = new Vector2(1f, 1f);
		Vector2 val8 = new Vector2(0f, 1f);
		_mesh.vertices = new Vector3[4] { val, val2, val3, val4 };
		_mesh.uv = new Vector2[4] { val5, val6, val7, val8 };
		_mesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
		_mesh.bounds = new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
		_renderer = ((Component)this).gameObject.AddComponent<MeshRenderer>();
		((Object)_renderer).hideFlags = (HideFlags)61;
		_filter = ((Component)this).gameObject.AddComponent<MeshFilter>();
		((Object)_filter).hideFlags = (HideFlags)61;
		_filter.sharedMesh = _mesh;
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreCull = (CameraCallback)Delegate.Combine((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(QuadPreCull));
	}

	private void OnDisable()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreCull = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(QuadPreCull));
	}

	private void LateUpdate()
	{
		((Renderer)_renderer).enabled = true;
		((Renderer)_renderer).sharedMaterial = material;
	}

	private void QuadPreCull(Camera camera)
	{
		Camera val = targetCamera ?? Camera.main;
		((Renderer)_renderer).enabled = (Object)(object)camera == (Object)(object)val;
	}
}
