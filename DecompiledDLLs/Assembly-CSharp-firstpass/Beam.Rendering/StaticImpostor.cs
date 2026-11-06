using System.Collections.Generic;
using UnityEngine;

namespace Beam.Rendering;

public class StaticImpostor : ImpostorBase
{
	private void Reset()
	{
		Lod.Renderers = ((Component)this).GetComponentsInChildren<Renderer>();
	}

	public override void SetRenderables(IEnumerable<Renderer> renderers)
	{
	}
}
