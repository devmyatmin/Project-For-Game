using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.UI;

public abstract class NotificationPresenterBase<TNotification, TView> : MonoBehaviour, INotificationPresenter<TNotification> where TNotification : NotificationBase where TView : NotificationViewAdapterBase
{
	[SerializeField]
	private TView _view;

	private TNotification _notification;

	private CoroutineHandle _showSequence;

	private CoroutineHandle _hideSequence;

	[SerializeField]
	private UnityEvent _show;

	[SerializeField]
	private UnityEvent _hide;

	protected TView View => _view;

	protected TNotification Notification => _notification;

	private void Awake()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)(((object)this).GetType()?.ToString() + ":: No view component assigned!"));
			_view = NullNotificationViewAdapter.Instance as TView;
		}
	}

	private void Start()
	{
		_view.Hide();
	}

	public virtual void Initialize(TNotification notification)
	{
		_notification = notification;
		_view.TitleLabel.Text = _notification.Title;
		_view.MessageLabel.Text = _notification.Message;
	}

	public float Show_Yieldable()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_showSequence = Timing.RunCoroutine(Show_Sequence());
		return Timing.WaitUntilDone(_showSequence);
	}

	public float Hide_Yieldable()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_hideSequence = Timing.RunCoroutine(Hide_Sequence());
		return Timing.WaitUntilDone(_hideSequence);
	}

	public void ForceHide()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_showSequence);
		Timing.KillCoroutines(_hideSequence);
		_view.Hide();
	}

	protected IEnumerator<float> Show_Sequence()
	{
		_view.Show();
		UnityEvent show = _show;
		if (show != null)
		{
			show.Invoke();
		}
		float timer = 0f;
		while (_notification != null && !_notification.Interrupted && !_notification.Disposed)
		{
			if (_notification.Duration != -1f)
			{
				timer += Time.deltaTime;
				if (timer > _notification.Duration)
				{
					break;
				}
			}
			yield return 0f;
		}
	}

	protected IEnumerator<float> Hide_Sequence()
	{
		UnityEvent hide = _hide;
		if (hide != null)
		{
			hide.Invoke();
		}
		yield return _view.Hide_Coroutine();
		_notification = null;
	}
}
