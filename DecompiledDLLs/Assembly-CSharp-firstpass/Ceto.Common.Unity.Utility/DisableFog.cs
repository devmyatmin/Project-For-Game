using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public class DisableFog : MonoBehaviour
{
	private bool revertFogState;

	private void Start()
	{
	}

	private void OnPreRender()
	{
		revertFogState = RenderSettings.fog;
		RenderSettings.fog = false;
	}

	private void OnPostRender()
	{
		RenderSettings.fog = revertFogState;
	}
}
