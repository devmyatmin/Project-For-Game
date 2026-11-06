using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class USkillNotificationViewAdapter : NotificationViewAdapterBase
{
	[SerializeField]
	private TMPLabelViewAdapter _titleLabel;

	[SerializeField]
	private TMPLabelViewAdapter _messageLabel;

	[SerializeField]
	private Animator _animator;

	private CoroutineHandle _hideSequence;

	public override ILabelViewAdapter TitleLabel => _titleLabel;

	public override ILabelViewAdapter MessageLabel => _messageLabel;

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_hideSequence);
	}

	public override float Hide_Coroutine()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_hideSequence = Timing.RunCoroutine(Hide_Sequence());
		return Timing.WaitUntilDone(_hideSequence);
	}

	private IEnumerator<float> Hide_Sequence()
	{
		int num = Animator.StringToHash("Close");
		_animator.Play(num);
		yield return Timing.WaitForSeconds(0.75f);
		Hide();
	}
}
