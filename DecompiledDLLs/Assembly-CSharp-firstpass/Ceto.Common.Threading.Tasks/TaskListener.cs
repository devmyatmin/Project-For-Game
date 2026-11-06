namespace Ceto.Common.Threading.Tasks;

public class TaskListener
{
	private ThreadedTask m_task;

	private volatile int m_waiting;

	public ThreadedTask ListeningTask => m_task;

	public int Waiting
	{
		get
		{
			return m_waiting;
		}
		set
		{
			m_waiting = value;
		}
	}

	public TaskListener(ThreadedTask task)
	{
		m_task = task;
	}

	public void OnFinish()
	{
		m_waiting--;
		if (m_waiting == 0 && !m_task.Cancelled)
		{
			m_task.StopWaiting();
		}
	}
}
