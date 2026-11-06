using System.Collections.Generic;
using Beam.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

public static class CommandBufferExtensions
{
	private static Mesh _fullscreenQuad;

	public static Mesh FullscreenQuad
	{
		get
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected Obj, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_fullscreenQuad != (Object)null)
			{
				return _fullscreenQuad;
			}
			_fullscreenQuad = new Mesh
			{
				name = "Fullscreen Quad"
			};
			_fullscreenQuad.SetVertices(new List<Vector3>
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(1f, -1f, 1f),
				new Vector3(1f, 1f, 2f),
				new Vector3(-1f, 1f, 3f)
			});
			_fullscreenQuad.SetUVs(0, new List<Vector2>
			{
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f),
				new Vector2(0f, 0f)
			});
			_fullscreenQuad.SetIndices(new int[4] { 0, 1, 2, 3 }, (MeshTopology)2, 0, false);
			_fullscreenQuad.UploadMeshData(false);
			return _fullscreenQuad;
		}
	}

	public static void BlitFullscreenQuad(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, PropertySheet propertySheet, int pass, bool clear = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
		cmd.SetRenderTarget(destination);
		if (clear)
		{
			cmd.ClearRenderTarget(true, true, Color.clear);
		}
		cmd.DrawMesh(FullscreenQuad, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
	}

	public static void DrawRenderer(this CommandBuffer buffer, InstancedMeshRenderer renderer, Material material, int submeshIndex)
	{
		renderer.RenderToBufferInstanced(buffer, submeshIndex, material);
	}
}
