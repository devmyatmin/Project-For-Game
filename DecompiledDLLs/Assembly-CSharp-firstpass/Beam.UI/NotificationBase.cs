using System;

namespace Beam.UI;

[Serializable]
public abstract class NotificationBase
{
	public abstract NotificationPriority Priority { get; set; }

	public abstract float Duration { get; set; }

	public abstract int PlayerId { get; set; }

	public abstract string Title { get; }

	public abstract string Message { get; }

	public abstract bool SurviveSuspension { get; set; }

	public abstract bool Interrupted { get; set; }

	public abstract bool Disposed { get; set; }

	public abstract event Action<NotificationBase> Finished;

	public abstract void Raise();

	public abstract void Finish();

	public void Dispose()
	{
		Disposed = true;
	}
}
