using Ceto.Common.Unity.Utility;
using UnityEngine;

namespace Ceto;

public class DepthData : ViewData
{
	public Camera cam;

	public RenderTexture target0;

	public RenderTexture target1;

	public IRefractionCommand refractionCommand;

	public void DestroyCamera()
	{
		if (!((Object)(object)cam == (Object)null))
		{
			cam.targetTexture = null;
			Object.Destroy((Object)(object)((Component)cam).gameObject);
			Object.Destroy((Object)(object)cam);
			cam = null;
		}
	}

	public void DestroyTargets()
	{
		RTUtility.ReleaseAndDestroy(target0);
		RTUtility.ReleaseAndDestroy(target1);
		target0 = null;
		target1 = null;
	}
}
