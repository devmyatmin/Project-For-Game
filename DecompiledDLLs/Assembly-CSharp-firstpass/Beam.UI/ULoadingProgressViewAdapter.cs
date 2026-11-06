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
		Visible = _container.activeSelf;
	}

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_updateProgressSequence);
	}

	public override void Show()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		_container.SetActive(true);
		base.Show();
		_updateProgressSequence = Timing.RunCoroutine(Show_Sequence());
	}

	public override void Hide()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_container.SetActive(false);
		base.Hide();
		Timing.KillCoroutines(_updateProgressSequence);
	}

	private IEnumerator<float> Show_Sequence()
	{
		while (Visible)
		{
			UpdateProgress(0f);
			yield return 0f;
		}
	}

	public override void UpdateProgress(float progress)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localEulerAngles = ((Component)_tickerImage).transform.localEulerAngles;
		localEulerAngles.z -= 100f * Time.unscaledDeltaTime;
		if (localEulerAngles.z > 360f)
		{
			localEulerAngles.z = 0f;
		}
		((Component)_tickerImage).transform.localEulerAngles = localEulerAngles;
	}
}
