using System;
using UnityEngine;

namespace Beam.UI;

public class UVideoScreenViewAdapter : MonoBehaviour
{
	[SerializeField]
	private GameObject _gameObject;

	[SerializeField]
	private RectTransform _temp;

	[SerializeField]
	private AudioClip _tempClip;

	public void Show()
	{
		_gameObject.SetActive(true);
		LeanTween.value(((Component)this).gameObject, (Action<float>)SetScale, 0f, 1f, 0.15f);
		AudioManager.GetAudioPlayer().Play2D(_tempClip, AudioMixerChannels.Master);
	}

	public void Hide()
	{
		_gameObject.SetActive(false);
	}

	private void SetScale(float value)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		((Transform)_temp).localScale = new Vector3(value, value, 1f);
	}
}
