using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Ceto.Common.Threading.Tasks;

namespace Ceto.Common.Threading.Scheduling;

public class Scheduler : IScheduler
{
	private LinkedList<IThreadedTask> m_scheduledTasks;

	private LinkedList<IThreadedTask> m_finishedTasks;

	private LinkedList<IThreadedTask> m_runningTasks;

	private LinkedList<IThreadedTask> m_waitingTasks;

	private ICoroutine m_coroutine;

	private Exception m_exception;

	private readonly object m_lock = new object();

	private LinkedList<IThreadedTask> m_haveRan;

	private volatile bool m_shutingDown;

	public int TasksRanThisUpdate { get; private set; }

	public int TasksFinishedThisUpdate { get; private set; }

	public int MaxTasksPerUpdate { get; private set; }

	public int MaxFinishPerUpdate { get; private set; }

	public float MaxWaitTime { get; set; }

	public float MinWaitTime { get; set; }

	public bool DisableMultithreading { get; set; }

	public bool ShutingDown
	{
		set
		{
			m_shutingDown = value;
		}
	}

	public Scheduler()
	{
		MaxWaitTime = 1000f;
		MinWaitTime = 100f;
		m_coroutine = null;
		MaxTasksPerUpdate = 100;
		MaxFinishPerUpdate = 100;
		m_scheduledTasks = new LinkedList<IThreadedTask>();
		m_finishedTasks = new LinkedList<IThreadedTask>();
		m_runningTasks = new LinkedList<IThreadedTask>();
		m_waitingTasks = new LinkedList<IThreadedTask>();
		m_haveRan = new LinkedList<IThreadedTask>();
	}

	public Scheduler(int maxTasksPerUpdate, int maxFinishPerUpdate, ICoroutine coroutine)
	{
		MaxWaitTime = 1000f;
		MinWaitTime = 100f;
		m_coroutine = coroutine;
		MaxTasksPerUpdate = Math.Max(1, maxTasksPerUpdate);
		MaxFinishPerUpdate = Math.Max(1, maxFinishPerUpdate);
		m_scheduledTasks = new LinkedList<IThreadedTask>();
		m_finishedTasks = new LinkedList<IThreadedTask>();
		m_runningTasks = new LinkedList<IThreadedTask>();
		m_waitingTasks = new LinkedList<IThreadedTask>();
		m_haveRan = new LinkedList<IThreadedTask>();
	}

	public void Update()
	{
		TasksRanThisUpdate = 0;
		TasksFinishedThisUpdate = 0;
		FinishTasks();
		while (TasksRanThisUpdate < MaxTasksPerUpdate)
		{
			if (ScheduledTasks() > 0)
			{
				IThreadedTask value = m_scheduledTasks.First.Value;
				m_scheduledTasks.RemoveFirst();
				RunTask(value);
			}
			CheckForException();
			if (ScheduledTasks() == 0)
			{
				break;
			}
		}
		FinishTasks();
	}

	private void RunTask(IThreadedTask task)
	{
		lock (m_lock)
		{
			TasksRanThisUpdate++;
			if (!task.IsThreaded || DisableMultithreading)
			{
				task.Start();
				IEnumerator enumerator = task.Run();
				if (enumerator != null)
				{
					if (m_coroutine == null)
					{
						throw new InvalidOperationException("Scheduler trying to run a coroutine task when coroutine interface is null");
					}
					m_coroutine.RunCoroutine(enumerator);
					if (!IsFinishing(task))
					{
						m_runningTasks.AddLast(task);
					}
				}
				else
				{
					task.End();
				}
			}
			else
			{
				task.Start();
				m_runningTasks.AddLast(task);
				ThreadPool.QueueUserWorkItem(RunThreaded, task);
			}
		}
	}

	private void RunThreaded(object o)
	{
		if (!(o is IThreadedTask threadedTask))
		{
			Throw(new InvalidCastException("Object is not a ITask or is null"));
			return;
		}
		try
		{
			threadedTask.Run();
		}
		catch (Exception e)
		{
			Throw(e);
		}
	}

	public void FinishTasks()
	{
		if (TasksFinishedThisUpdate >= MaxFinishPerUpdate)
		{
			return;
		}
		lock (m_lock)
		{
			m_haveRan.Clear();
			LinkedList<IThreadedTask>.Enumerator enumerator = m_runningTasks.GetEnumerator();
			while (enumerator.MoveNext())
			{
				IThreadedTask current = enumerator.Current;
				if (current.Ran)
				{
					m_haveRan.AddLast(current);
				}
			}
			LinkedList<IThreadedTask>.Enumerator enumerator2 = m_haveRan.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				Finished(enumerator2.Current);
			}
			if (m_finishedTasks.Count == 0)
			{
				return;
			}
			IThreadedTask threadedTask = m_finishedTasks.First.Value;
			m_finishedTasks.RemoveFirst();
			while (threadedTask != null)
			{
				threadedTask.End();
				TasksFinishedThisUpdate++;
				if (m_finishedTasks.Count == 0 || TasksFinishedThisUpdate >= MaxFinishPerUpdate)
				{
					threadedTask = null;
					continue;
				}
				threadedTask = m_finishedTasks.First.Value;
				m_finishedTasks.RemoveFirst();
			}
			m_haveRan.Clear();
		}
	}

	public bool HasTasks()
	{
		if (ScheduledTasks() <= 0 && RunningTasks() <= 0 && FinishingTasks() <= 0)
		{
			return WaitingTasks() > 0;
		}
		return true;
	}

	public void Cancel(IThreadedTask task)
	{
		lock (m_lock)
		{
			if (m_scheduledTasks.Contains(task))
			{
				task.Cancel();
				m_scheduledTasks.Remove(task);
			}
			else if (m_waitingTasks.Contains(task))
			{
				task.Cancel();
				m_waitingTasks.Remove(task);
			}
		}
	}

	public void CancelAllTasks()
	{
		lock (m_lock)
		{
			m_scheduledTasks.Clear();
			m_waitingTasks.Clear();
			foreach (IThreadedTask runningTask in m_runningTasks)
			{
				runningTask.Cancel();
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			bool flag = false;
			while (!flag && (float)stopwatch.ElapsedMilliseconds < MaxWaitTime)
			{
				flag = true;
				foreach (IThreadedTask runningTask2 in m_runningTasks)
				{
					if (!runningTask2.Ran)
					{
						flag = false;
						break;
					}
				}
			}
			while ((float)stopwatch.ElapsedMilliseconds < MinWaitTime)
			{
			}
			m_runningTasks.Clear();
			m_finishedTasks.Clear();
		}
	}

	public bool Contains(IThreadedTask task)
	{
		if (IsScheduled(task))
		{
			return true;
		}
		if (IsWaiting(task))
		{
			return true;
		}
		if (IsRunning(task))
		{
			return true;
		}
		if (IsFinishing(task))
		{
			return true;
		}
		return false;
	}

	public int ScheduledTasks()
	{
		int num = 0;
		lock (m_lock)
		{
			return m_scheduledTasks.Count;
		}
	}

	public int RunningTasks()
	{
		int num = 0;
		lock (m_lock)
		{
			return m_runningTasks.Count;
		}
	}

	public int WaitingTasks()
	{
		int num = 0;
		lock (m_lock)
		{
			return m_waitingTasks.Count;
		}
	}

	public int FinishingTasks()
	{
		int num = 0;
		lock (m_lock)
		{
			return m_finishedTasks.Count;
		}
	}

	public int Tasks()
	{
		return RunningTasks() + ScheduledTasks() + WaitingTasks() + FinishingTasks();
	}

	public bool IsScheduled(IThreadedTask task)
	{
		lock (m_lock)
		{
			return m_scheduledTasks.Contains(task);
		}
	}

	public bool IsRunning(IThreadedTask task)
	{
		lock (m_lock)
		{
			return m_runningTasks.Contains(task);
		}
	}

	public bool IsWaiting(IThreadedTask task)
	{
		lock (m_lock)
		{
			return m_waitingTasks.Contains(task);
		}
	}

	public bool IsFinishing(IThreadedTask task)
	{
		lock (m_lock)
		{
			return m_finishedTasks.Contains(task);
		}
	}

	public void Add(IThreadedTask task)
	{
		lock (m_lock)
		{
			if (!m_shutingDown)
			{
				task.Scheduler = this;
				m_scheduledTasks.AddLast(task);
			}
		}
	}

	public void Run(IThreadedTask task)
	{
		lock (m_lock)
		{
			if (!m_shutingDown)
			{
				task.Scheduler = this;
				if (TasksRanThisUpdate >= MaxTasksPerUpdate)
				{
					Add(task);
				}
				else
				{
					RunTask(task);
				}
			}
		}
	}

	public void AddWaiting(IThreadedTask task)
	{
		lock (m_lock)
		{
			if (!m_shutingDown)
			{
				task.Scheduler = this;
				m_waitingTasks.AddLast(task);
			}
		}
	}

	public void Finished(IThreadedTask task)
	{
		lock (m_lock)
		{
			m_runningTasks.Remove(task);
			if (!m_shutingDown && !task.NoFinish && !task.Cancelled)
			{
				m_finishedTasks.AddLast(task);
			}
		}
	}

	public void CheckForException()
	{
		lock (m_lock)
		{
			if (m_exception != null)
			{
				Exception exception = m_exception;
				m_exception = null;
				throw exception;
			}
		}
	}

	public void Throw(Exception e)
	{
		lock (m_lock)
		{
			m_exception = e;
		}
	}

	public void StopWaiting(IThreadedTask task, bool run)
	{
		lock (m_lock)
		{
			if (!m_shutingDown)
			{
				m_waitingTasks.Remove(task);
				if (run)
				{
					RunTask(task);
				}
				else
				{
					m_scheduledTasks.AddLast(task);
				}
			}
		}
	}

	public void Clear()
	{
		if (RunningTasks() > 0)
		{
			throw new InvalidOperationException("Can not clear the scheduler when there are running tasks.");
		}
		m_scheduledTasks.Clear();
		m_runningTasks.Clear();
		m_finishedTasks.Clear();
		m_waitingTasks.Clear();
		m_exception = null;
	}
}
