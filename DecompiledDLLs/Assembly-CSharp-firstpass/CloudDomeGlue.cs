using UnityEngine;
using kode80.Clouds;

public class CloudDomeGlue : MonoBehaviour
{
	public kode80Clouds clouds;

	private MeshRenderer _renderer;

	private void Start()
	{
		_renderer = ((Component)this).GetComponent<MeshRenderer>();
	}

	private void LateUpdate()
	{
		((Renderer)_renderer).sharedMaterial.SetTexture("_CloudTex", (Texture)(object)clouds.currentFrame);
	}
}
