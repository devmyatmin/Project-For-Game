using System;
using System.Collections;
using System.Collections.Generic;
using Ceto.Common.Threading.Scheduling;

namespace Ceto.Common.Threading.Tasks;

public abstract class ThreadedTask : IThreadedTask, ICancelToken
{
	private readonly bool m_isThreaded;

	private volatile bool m_done;

	private volatile bool m_ran;

	private volatile bool m_noFinish;

	private volatile bool m_runOnStopWaiting;

	private volatile bool m_started;

	private volatile bool m_cancelled;

	protected IScheduler m_scheduler;

	private LinkedList<TaskListener> m_listeners;

	private TaskListener m_listener;

	private readonly object m_lock = new object();

	public float RunTime { get; set; }

	public bool IsThreaded => m_isThreaded;

	public bool Done => m_done;

	public bool Ran => m_ran;

	public bool NoFinish
	{
		get
		{
			return m_noFinish;
		}
		set
		{
			m_noFinish = value;
		}
	}

	public bool Waiting => m_listener.Waiting > 0;

	public bool RunOnStopWaiting
	{
		get
		{
			return m_runOnStopWaiting;
		}
		set
		{
			m_runOnStopWaiting = value;
		}
	}

	public bool Started => m_started;

	public bool Cancelled => m_cancelled;

	public IScheduler Scheduler
	{
		set
		{
			m_scheduler = value;
		}
	}

	protected LinkedList<TaskListener> Listeners => m_listeners;

	protected TaskListener Listener => m_listener;

	protected ThreadedTask(bool isThreaded)
	{
		m_scheduler = null;
		m_isThreaded = isThreaded;
		m_listeners = new LinkedList<TaskListener>();
		m_listener = new TaskListener(this);
	}

	public virtual void Start()
	{
		m_started = true;
	}

	public virtual void Reset()
	{
		lock (m_lock)
		{
			m_listeners.Clear();
			m_listener.Waiting = 0;
			m_ran = false;
			m_done = false;
			m_cancelled = false;
			m_started = false;
			RunTime = 0f;
		}
	}

	public abstract IEnumerator Run();

	protected virtual void FinishedRunning()
	{
		m_ran = true;
		if (m_noFinish)
		{
			m_done = true;
		}
		lock (m_lock)
		{
			if (m_noFinish && !m_cancelled)
			{
				LinkedList<TaskListener>.Enumerator enumerator = m_listeners.GetEnumerator();
				while (enumerator.MoveNext())
				{
					enumerator.Current.OnFinish();
				}
				m_listeners.Clear();
			}
		}
	}

	public virtual void End()
	{
		m_done = true;
		lock (m_lock)
		{
			if (!m_cancelled)
			{
				LinkedList<TaskListener>.Enumerator enumerator = m_listeners.GetEnumerator();
				while (enumerator.MoveNext())
				{
					enumerator.Current.OnFinish();
				}
			}
			m_listeners.Clear();
		}
	}

	public virtual void Cancel()
	{
		lock (m_lock)
		{
			m_cancelled = true;
			m_listeners.Clear();
		}
	}

	public virtual void WaitOn(ThreadedTask task)
	{
		lock (m_lock)
		{
			if (task.Cancelled)
			{
				throw new InvalidOperationException("Can not wait on a task that is cancelled");
			}
			if (task.Done)
			{
				throw new InvalidOperationException("Can not wait on a task that is already done");
			}
			if (task.IsThreaded && task.NoFinish && !m_isThreaded)
			{
				throw new InvalidOperationException("A non-threaded task cant wait on a threaded task with no finish");
			}
			m_listener.Waiting++;
			task.Listeners.AddLast(m_listener);
		}
	}

	public virtual void StopWaiting()
	{
		lock (m_lock)
		{
			if (m_scheduler != null && !m_cancelled)
			{
				m_scheduler.StopWaiting(this, m_runOnStopWaiting);
			}
		}
	}

	public override string ToString()
	{
		return string.Format("[Task: isThreaded={0}, started={1}, ran={2}, done={3}, cancelled={4}]", new object[5] { m_isThreaded, m_started, m_ran, m_done, m_cancelled });
	}
}
