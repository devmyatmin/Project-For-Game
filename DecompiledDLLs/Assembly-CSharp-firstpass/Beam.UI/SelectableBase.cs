using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class SelectableBase : MonoBehaviour
{
	[SerializeField]
	private AnimationTriggers _animationTriggers = new AnimationTriggers();

	private Animator _animator;

	private SelectionState _state;

	protected Animator Animator
	{
		get
		{
			if ((Object)(object)_animator == (Object)null)
			{
				_animator = ((Component)this).GetComponent<Animator>();
			}
			return _animator;
		}
	}

	public void Select()
	{
		_state = SelectionState.Highlighted;
		DoStateTransition(_state);
	}

	public void Deselect()
	{
		_state = SelectionState.Normal;
		DoStateTransition(_state);
	}

	protected virtual void DoStateTransition(SelectionState state)
	{
		string triggername = state switch
		{
			SelectionState.Normal => _animationTriggers.normalTrigger, 
			SelectionState.Highlighted => _animationTriggers.highlightedTrigger, 
			SelectionState.Pressed => _animationTriggers.pressedTrigger, 
			SelectionState.Disabled => _animationTriggers.disabledTrigger, 
			_ => string.Empty, 
		};
		if (((Component)this).gameObject.activeInHierarchy)
		{
			TriggerAnimation(triggername);
		}
	}

	private void TriggerAnimation(string triggername)
	{
		if (!string.IsNullOrEmpty(triggername))
		{
			Animator.ResetTrigger(_animationTriggers.normalTrigger);
			Animator.ResetTrigger(_animationTriggers.pressedTrigger);
			Animator.ResetTrigger(_animationTriggers.highlightedTrigger);
			Animator.ResetTrigger(_animationTriggers.disabledTrigger);
			Animator.SetTrigger(triggername);
		}
	}

	public SelectableBase()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
	}
}
