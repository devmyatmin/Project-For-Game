using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class ULoadingProgressViewAdapter : LoadingProgressViewBase
{
	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private UImageViewAdapter _tickerImage;

	private CoroutineHandle _updateProgressSequence;

	private void Awake()
	{
		base.Visible = _container.activeSelf;
	}

	private void OnDestroy()
	{
		Timing.KillCoroutines(_updateProgressSequence);
	}

	public override void Show()
	{
		_container.SetActive(value: true);
		base.Show();
		_updateProgressSequence = Timing.RunCoroutine(Show_Sequence());
	}

	public override void Hide()
	{
		_container.SetActive(value: false);
		base.Hide();
		Timing.KillCoroutines(_updateProgressSequence);
	}

	private IEnumerator<float> Show_Sequence()
	{
		while (base.Visible)
		{
			UpdateProgress(0f);
			yield return 0f;
		}
	}

	public override void UpdateProgress(float progress)
	{
		Vector3 localEulerAngles = _tickerImage.transform.localEulerAngles;
		localEulerAngles.z -= 100f * Time.unscaledDeltaTime;
		if (localEulerAngles.z > 360f)
		{
			localEulerAngles.z = 0f;
		}
		_tickerImage.transform.localEulerAngles = localEulerAngles;
	}
}
