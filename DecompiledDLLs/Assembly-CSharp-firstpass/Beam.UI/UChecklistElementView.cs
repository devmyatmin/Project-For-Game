using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class UChecklistElementView : ChecklistElementViewBase
{
	[SerializeField]
	private TMPLabelViewAdapter _descriptionLabel;

	[SerializeField]
	private Animator _checkBoxAnimator;

	private bool _animating;

	private CoroutineHandle _completeSequence;

	public override bool IsBusy => _animating;

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_completeSequence);
	}

	public override void SetDescription(string description)
	{
		_descriptionLabel.Text = description;
	}

	public override void SetComplete()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_completeSequence = Timing.RunCoroutine(Complete_Sequence());
	}

	private IEnumerator<float> Complete_Sequence()
	{
		yield return float.NegativeInfinity;
		_animating = true;
		int num = Animator.StringToHash("Check");
		_checkBoxAnimator.Play(num);
		AnimatorStateInfo currentAnimatorStateInfo = _checkBoxAnimator.GetCurrentAnimatorStateInfo(0);
		float num2 = Mathf.Abs(currentAnimatorStateInfo.length);
		yield return Timing.WaitForSeconds(num2);
		_animating = false;
	}
}
