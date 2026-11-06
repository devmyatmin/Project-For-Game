using UnityEngine;

namespace Beam.PostEffects;

[RequireComponent(typeof(Camera))]
public class EagleVision : MonoBehaviour
{
	[SerializeField]
	private Material _material;

	private bool _eagle;

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)104))
		{
			_eagle = !_eagle;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!_eagle)
		{
			Graphics.Blit((Texture)(object)source, destination);
		}
		else
		{
			Graphics.Blit((Texture)(object)source, destination, _material);
		}
	}
}
