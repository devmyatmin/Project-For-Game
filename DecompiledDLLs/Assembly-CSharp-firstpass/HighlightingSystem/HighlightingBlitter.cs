using System.Collections.Generic;
using UnityEngine;

namespace HighlightingSystem;

[RequireComponent(typeof(Camera))]
public class HighlightingBlitter : MonoBehaviour
{
	protected List<HighlightingBase> renderers = new List<HighlightingBase>();

	public virtual void Register(HighlightingBase renderer)
	{
		if (!renderers.Contains(renderer))
		{
			renderers.Add(renderer);
		}
		((Behaviour)this).enabled = renderers.Count > 0;
	}

	public virtual void Unregister(HighlightingBase renderer)
	{
		int num = renderers.IndexOf(renderer);
		if (num != -1)
		{
			renderers.RemoveAt(num);
		}
		((Behaviour)this).enabled = renderers.Count > 0;
	}
}
