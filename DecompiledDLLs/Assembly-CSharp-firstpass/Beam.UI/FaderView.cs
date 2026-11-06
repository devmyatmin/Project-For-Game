using UnityEngine;

namespace Beam.UI;

public class FaderView : MonoBehaviour
{
	[SerializeField]
	private GameObject _gameObject;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private UImageViewAdapter _image;

	[SerializeField]
	private UTextureViewAdapter _customTexture;

	public ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public IImageViewAdapter Image => _image;

	public ITextureViewAdapter CustomTexture => _customTexture;

	public void Show()
	{
		_gameObject.SetActive(true);
	}

	public void Hide()
	{
		_gameObject.SetActive(false);
	}
}
