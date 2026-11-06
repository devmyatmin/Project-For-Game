using System;
using Beam.Crafting;
using Beam.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class PileRenderTest : MonoBehaviour
{
	public InstancingData _pileData;

	private IInstancedMeshData _data;

	public CraftingType CraftingType;

	public Material Material;

	private ComputeBuffer _buff;

	private MaterialPropertyBlock _block;

	public int Count;

	private Matrix4x4[] _matrices;

	private void Update()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected Obj, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (_block == null)
		{
			_block = new MaterialPropertyBlock();
		}
		if (_data == null)
		{
			_data = _pileData.GetInstancingData(CraftingType);
			_buff = new ComputeBuffer(_data.GetMatrices().Length, 64);
			_buff.SetData((Array)_data.GetMatrices());
			Material[] materials = _data.GetMaterials();
			for (int i = 0; i < materials.Length; i++)
			{
				_ = materials[i];
			}
			Matrix4x4[] matrices = _data.GetMatrices();
			_matrices = new Matrix4x4[100];
			for (int j = 0; j < matrices.Length; j++)
			{
				_matrices[j] = ((Component)this).transform.localToWorldMatrix * matrices[j];
			}
		}
		Mesh mesh = _data.GetMesh();
		Material[] materials2 = _data.GetMaterials();
		Matrix4x4[] matrices2 = _data.GetMatrices();
		for (int k = 0; k < matrices2.Length; k++)
		{
			_matrices[k] = ((Component)this).transform.localToWorldMatrix * matrices2[k];
			for (int l = 0; l < mesh.subMeshCount; l++)
			{
				Graphics.DrawMesh(mesh, _matrices[k], materials2[l], Layers.INTERACTIVE_OBJECTS, (Camera)null, l, _block, (ShadowCastingMode)1, true);
			}
		}
	}
}
