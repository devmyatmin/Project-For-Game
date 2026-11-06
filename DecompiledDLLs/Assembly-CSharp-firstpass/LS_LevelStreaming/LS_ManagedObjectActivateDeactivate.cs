using UnityEngine;

namespace LS_LevelStreaming;

public class LS_ManagedObjectActivateDeactivate : LS_ManagedObjectBase
{
	protected GameObject m_go;

	public LS_ManagedObjectActivateDeactivate(GameObject p_go, LS_ITrigger p_trigger)
		: base(p_trigger)
	{
		m_go = p_go;
		if ((Object)(object)m_go != (Object)null)
		{
			m_isVisible = m_go.activeSelf;
		}
	}

	protected override void Hide()
	{
		if ((Object)(object)m_go != (Object)null)
		{
			m_go.SetActive(false);
		}
	}

	protected override void Show()
	{
		if ((Object)(object)m_go != (Object)null)
		{
			m_go.SetActive(true);
		}
	}
}
