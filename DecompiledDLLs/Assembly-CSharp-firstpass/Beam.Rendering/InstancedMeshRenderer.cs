using Beam.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beam.Rendering;

public class InstancedMeshRenderer : MonoBehaviour
{
	[SerializeField]
	private bool _isStatic;

	private Material[] _materials;

	private Matrix4x4[] _matrices;

	public IInstancedMeshData Data { get; protected set; }

	public bool IsStatic
	{
		get
		{
			return _isStatic;
		}
		set
		{
			_isStatic = value;
		}
	}

	public int DrawCount { get; set; }

	public void SetData(IInstancedMeshData data)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Data = data;
		Matrix4x4[] matrices = Data.GetMatrices();
		if (_matrices == null || _matrices.Length != matrices.Length)
		{
			_matrices = new Matrix4x4[matrices.Length];
		}
		if (IsStatic)
		{
			for (int i = 0; i < matrices.Length; i++)
			{
				_matrices[i] = ((Component)this).transform.localToWorldMatrix * matrices[i];
			}
		}
	}

	public Bounds GetBounds(int instanceCount = -1)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = Data.GetMesh().bounds;
		Matrix4x4[] matrices = Data.GetMatrices();
		Bounds result = default;
		int num = ((instanceCount == -1) ? matrices.Length : Mathf.Min(DrawCount, matrices.Length));
		for (int i = 0; i < num; i++)
		{
			Matrix4x4 val = matrices[i];
			result.Encapsulate(val.MultiplyPoint3x4(bounds.min));
			result.Encapsulate(val.MultiplyPoint3x4(bounds.max));
		}
		return result;
	}

	private void Update()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (Data == null || DrawCount == 0)
		{
			return;
		}
		if (!IsStatic)
		{
			Matrix4x4[] matrices = Data.GetMatrices();
			int num = Mathf.Min(DrawCount, matrices.Length);
			for (int i = 0; i < num; i++)
			{
				_matrices[i] = ((Component)this).transform.localToWorldMatrix * matrices[i];
			}
		}
		Render();
	}

	public void SetMaterialsColor(string colorName, Color c)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsValid(Data))
		{
			Material[] materials = Data.GetMaterials();
			for (int i = 0; i < materials.Length; i++)
			{
				materials[i].SetColor(colorName, c);
			}
		}
	}

	public void Render()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (Data == null || DrawCount == 0)
		{
			return;
		}
		Mesh mesh = Data.GetMesh();
		Material[] materials = Data.GetMaterials();
		int num = Mathf.Min(DrawCount, _matrices.Length);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < mesh.subMeshCount; j++)
			{
				Graphics.DrawMesh(mesh, _matrices[i], materials[j], Layers.INTERACTIVE_OBJECTS, (Camera)null, j, (MaterialPropertyBlock)null, (ShadowCastingMode)1, true);
			}
		}
	}

	public void RenderToBufferInstanced(CommandBuffer buffer, int submeshIndex, Material material)
	{
		if (Data != null)
		{
			Mesh mesh = Data.GetMesh();
			int num = Mathf.Min(DrawCount, _matrices.Length);
			buffer.DrawMeshInstanced(mesh, submeshIndex, material, 0, _matrices, num);
		}
	}

	protected virtual void OnDestroy()
	{
	}
}
