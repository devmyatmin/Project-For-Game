using UnityEngine;

namespace kode80.Effects;

[ExecuteInEditMode]
public class FilmicTonemapping : MonoBehaviour
{
	[Range(0f, 16f)]
	public float exposure = 1.5f;

	private Material _material;

	private void Start()
	{
	}

	private void Update()
	{
	}

	[ImageEffectTransformsToLDR]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected Obj, but got Unknown
		if ((Object)(object)_material == (Object)null)
		{
			_material = new Material(Shader.Find("Hidden/kode80/Effects/FilmicTonemapping"));
			((Object)_material).hideFlags = (HideFlags)61;
		}
		_material.SetFloat("_Exposure", exposure);
		Graphics.Blit((Texture)(object)source, destination, _material);
	}
}
