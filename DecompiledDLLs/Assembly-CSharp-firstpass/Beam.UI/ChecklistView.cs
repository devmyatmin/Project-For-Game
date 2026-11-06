using System.Collections.Generic;
using Beam.Utilities;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class ChecklistView : NotificationViewAdapterBase
{
	[SerializeField]
	private TMPLabelViewAdapter _titleLabel;

	[SerializeField]
	private TMPLabelViewAdapter _messageLabel;

	[SerializeField]
	private ChecklistElementController _checklistElementPrefab;

	[SerializeField]
	private Transform _checklistElementContainer;

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

	public ChecklistElementController CreateChecklistElement()
	{
		ChecklistElementController checklistElementController = Object.Instantiate<ChecklistElementController>(_checklistElementPrefab);
		((Component)checklistElementController).transform.SetParent(_checklistElementContainer, false);
		return checklistElementController;
	}

	private void DestroyChecklistElements()
	{
		for (int i = 0; i < _checklistElementContainer.childCount; i++)
		{
			BeamTeam.Destroy<GameObject>(((Component)_checklistElementContainer.GetChild(i)).gameObject);
		}
	}

	public override void Show()
	{
		((Component)_messageLabel).gameObject.SetActive(!string.IsNullOrEmpty(_messageLabel.Text));
		base.Show();
	}

	public override void Hide()
	{
		base.Hide();
		DestroyChecklistElements();
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
		ChecklistElementController[] checklistElementControllers = ((Component)_checklistElementContainer).GetComponentsInChildren<ChecklistElementController>();
		while (ChecklistElementViewIsBusyPredicate(checklistElementControllers))
		{
			yield return float.NegativeInfinity;
		}
		int num = Animator.StringToHash("Close");
		_animator.Play(num);
		AnimatorStateInfo currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
		float num2 = Mathf.Abs(currentAnimatorStateInfo.length);
		yield return Timing.WaitForSeconds(num2);
		Hide();
	}

	private bool ChecklistElementViewIsBusyPredicate(ChecklistElementController[] checklistElements)
	{
		for (int i = 0; i < checklistElements?.Length; i++)
		{
			if (checklistElements[i].View.IsBusy)
			{
				return true;
			}
		}
		return false;
	}
}
