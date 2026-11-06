using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beam.Rendering;

[Serializable]
public class LodGroup
{
	[SerializeField]
	private List<Lod> _lods = new List<Lod>();

	public IList<Lod> Lods => _lods;

	public LodGroup()
	{
	}

	public LodGroup(List<Lod> lods)
	{
		_lods = lods;
	}
}
