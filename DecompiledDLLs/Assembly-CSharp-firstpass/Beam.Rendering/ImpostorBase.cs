using System.Collections.Generic;
using UnityEngine;

namespace Beam.Rendering;

public abstract class ImpostorBase : MonoBehaviour
{
	[SerializeField]
	private int _parentPrefabId;

	[SerializeField]
	private int _parentInstanceId;

	[SerializeField]
	private Lod _lod = new Lod();

	public int ParentPrefabId
	{
		get
		{
			return _parentPrefabId;
		}
		set
		{
			_parentPrefabId = value;
		}
	}

	public int ParentInstanceId
	{
		get
		{
			return _parentInstanceId;
		}
		set
		{
			_parentInstanceId = value;
		}
	}

	public Lod Lod => _lod;

	public abstract void SetRenderables(IEnumerable<Renderer> renderers);

	public virtual void Refresh()
	{
	}

	public virtual void Release()
	{
	}
}
