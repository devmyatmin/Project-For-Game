using System;
using UnityEngine;

namespace Beam.Rendering;

[Serializable]
public class Lod
{
	private static MaterialPropertyBlock _PropertyBlock;

	public int CullingDistance;

	public Renderer[] Renderers;

	private bool _active;

	protected MaterialPropertyBlock PropertyBlock
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected Obj, but got Unknown
			MaterialPropertyBlock val = _PropertyBlock;
			if (val == null)
			{
				MaterialPropertyBlock val2 = new MaterialPropertyBlock();
				_PropertyBlock = val2;
				val = val2;
			}
			return val;
		}
	}

	public bool Active => _active;

	public bool IsImpostor { get; set; }

	public void SetActive(bool active)
	{
		_active = active;
		for (int i = 0; i < Renderers?.Length; i++)
		{
			Renderers[i].enabled = _active;
		}
	}

	public void Crossfade(float value)
	{
		for (int i = 0; i < Renderers?.Length; i++)
		{
			Renderer obj = Renderers[i];
			obj.GetPropertyBlock(PropertyBlock);
			PropertyBlock.SetFloat("Beam_LODFade", value);
			obj.SetPropertyBlock(PropertyBlock);
		}
	}
}
