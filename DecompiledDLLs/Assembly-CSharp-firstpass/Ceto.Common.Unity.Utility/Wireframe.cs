using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public class Wireframe : MonoBehaviour
{
	public bool on;

	public KeyCode toggleKey = (KeyCode)283;

	private void Start()
	{
	}

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown(toggleKey))
		{
			on = !on;
		}
	}

	private void OnPreRender()
	{
		if (on)
		{
			GL.wireframe = true;
		}
	}

	private void OnPostRender()
	{
		if (on)
		{
			GL.wireframe = false;
		}
	}

	public Wireframe()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
