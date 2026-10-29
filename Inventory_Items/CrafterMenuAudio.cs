using UnityEngine;

namespace Beam.UI.Crafting;

public class CrafterMenuAudio : MonoBehaviour
{
	[SerializeField]
	private CrafterMenuPresenter _crafterMenuPresenter;

	[SerializeField]
	private AudioSource _audio;

	[SerializeField]
	private AudioClip _openClip;

	[SerializeField]
	private AudioClip _closeClip;

	[SerializeField]
	private AudioClip _nextClip;

	[SerializeField]
	private AudioClip _previousClip;

	private void Start()
	{
		if (_crafterMenuPresenter != null)
		{
			_crafterMenuPresenter.Opened += CrafterMenuPresenter_Opened;
			_crafterMenuPresenter.Closed += CrafterMenuPresenter_Closed;
			_crafterMenuPresenter.CategoryIncremented += CrafterMenuPresenter_CategoryIncremented;
			_crafterMenuPresenter.CategoryDecremented += CrafterMenuPresenter_CategoryDecremented;
		}
	}

	private void OnDestroy()
	{
		if (_crafterMenuPresenter != null)
		{
			_crafterMenuPresenter.Opened -= CrafterMenuPresenter_Opened;
			_crafterMenuPresenter.Closed -= CrafterMenuPresenter_Closed;
			_crafterMenuPresenter.CategoryIncremented -= CrafterMenuPresenter_CategoryIncremented;
			_crafterMenuPresenter.CategoryDecremented -= CrafterMenuPresenter_CategoryDecremented;
		}
	}

	public void CrafterMenuPresenter_Opened()
	{
		_audio.clip = _openClip;
		_audio.Play();
	}

	public void CrafterMenuPresenter_Closed()
	{
		_audio.clip = _closeClip;
		_audio.Play();
	}

	public void CrafterMenuPresenter_CategoryIncremented()
	{
		_audio.clip = _nextClip;
		_audio.Play();
	}

	public void CrafterMenuPresenter_CategoryDecremented()
	{
		_audio.clip = _previousClip;
		_audio.Play();
	}
}
