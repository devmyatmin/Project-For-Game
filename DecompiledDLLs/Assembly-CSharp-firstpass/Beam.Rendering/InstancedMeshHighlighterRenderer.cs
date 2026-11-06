using System.Collections.Generic;
using Beam.PostEffects;
using HighlightingSystem;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beam.Rendering;

public class InstancedMeshHighlighterRenderer : HighlighterRenderer
{
	[SerializeField]
	private InstancedMeshRenderer _renderer;

	public override void Initialize(Material sharedOpaqueMaterial, Shader transparentShader, bool seeThrough)
	{
		data = new List<Data>();
		Material[] array = _renderer.Data?.GetMaterials();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Material sourceMat = array[i];
				Data item = default;
				item.material = GetHighlighterMaterial(sourceMat, sharedOpaqueMaterial, transparentShader, seeThrough, out item.transparent);
				item.submeshIndex = i;
				data.Add(item);
			}
		}
	}

	public override bool FillBuffer(CommandBuffer buffer, bool forceRender)
	{
		if ((Object)(object)_renderer == (Object)null)
		{
			return false;
		}
		if (HighlightingBase.IsHighlightingCamera(Camera.current))
		{
			lastCamera = Camera.current;
		}
		if (((Object)(object)lastCamera == (Object)(object)Camera.current) | forceRender)
		{
			for (int i = 0; i < base.data.Count; i++)
			{
				Data data = base.data[i];
				buffer.DrawRenderer(_renderer, data.material, data.submeshIndex);
			}
		}
		lastCamera = null;
		return true;
	}
}
