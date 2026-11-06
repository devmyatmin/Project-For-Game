using System;

namespace LS_LevelStreaming;

public abstract class LS_ManagedObjectBase
{
	private static int s_nextFreeID = 1;

	public EventHandler m_onShow;

	public EventHandler m_onHide;

	protected readonly int m_id;

	protected readonly LS_ITrigger m_trigger;

	protected bool m_isVisible;

	public int ID => m_id;

	public LS_ITrigger Trigger => m_trigger;

	public bool IsVisible => m_isVisible;

	public LS_ManagedObjectBase(LS_ITrigger p_trigger)
	{
		m_id = s_nextFreeID++;
		m_trigger = p_trigger;
	}

	public virtual void Update()
	{
		bool flag = m_trigger.IsVisible();
		if (m_isVisible && !flag)
		{
			ForceHide();
		}
		else if (!m_isVisible & flag)
		{
			ForceShow();
		}
	}

	public void ForceShow()
	{
		if (!m_isVisible)
		{
			m_isVisible = true;
			Show();
			if (m_onShow != null)
			{
				m_onShow(this, EventArgs.Empty);
			}
		}
	}

	public void ForceHide()
	{
		if (m_isVisible)
		{
			m_isVisible = false;
			Hide();
			if (m_onHide != null)
			{
				m_onHide(this, EventArgs.Empty);
			}
		}
	}

	protected abstract void Show();

	protected abstract void Hide();
}
