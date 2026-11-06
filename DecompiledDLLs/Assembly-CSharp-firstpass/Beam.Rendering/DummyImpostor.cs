using System.Collections.Generic;
using UnityEngine;

namespace Beam.Rendering;

public class DummyImpostor : ImpostorBase
{
	private static DummyImpostor _Instance;

	public static DummyImpostor Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_Instance == (Object)null)
			{
				_Instance = new GameObject("~DummyImpostor").AddComponent<DummyImpostor>();
				((Component)_Instance).transform.SetAsFirstSibling();
				((Component)_Instance).transform.position = Vector3.zero;
				((Component)_Instance).transform.rotation = Quaternion.identity;
			}
			return _Instance;
		}
	}

	public override void SetRenderables(IEnumerable<Renderer> renderers)
	{
	}
}
