using UnityEngine;

namespace Ceto;

[RequireComponent(typeof(Camera))]
public class AddRenderTarget : MonoBehaviour
{
	public int scale = 2;

	private void Start()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		((Component)this).GetComponent<Camera>().targetTexture = new RenderTexture(Screen.width / scale, Screen.height / scale, 24);
	}

	private void OnGUI()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Camera component = ((Component)this).GetComponent<Camera>();
		if (!((Object)(object)component.targetTexture == (Object)null))
		{
			int width = ((Texture)component.targetTexture).width;
			int height = ((Texture)component.targetTexture).height;
			GUI.DrawTexture(new Rect(10f, 10f, (float)width, (float)height), (Texture)(object)component.targetTexture);
		}
	}
}
