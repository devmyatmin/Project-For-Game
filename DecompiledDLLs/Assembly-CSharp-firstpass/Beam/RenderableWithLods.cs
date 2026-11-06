using System;
using UnityEngine;

namespace Beam;

[Serializable]
public class RenderableWithLods
{
	[SerializeField]
	private Renderable[] _lods;

	private int _lod;

	public Renderable Renderable => _lods[_lod];

	public void SetLod(int level)
	{
		_lod = Mathf.Min(level, _lods.Length - 1);
	}

	public int GetLod()
	{
		return _lod;
	}
}
