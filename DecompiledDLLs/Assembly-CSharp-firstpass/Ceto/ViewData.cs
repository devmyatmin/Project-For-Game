using UnityEngine;

namespace Ceto;

public abstract class ViewData
{
	private bool m_updated;

	public bool IsViewUpdated(Camera cam)
	{
		return m_updated;
	}

	public void ClearUpdatedViews()
	{
		m_updated = false;
	}

	public void SetViewAsUpdated(Camera cam)
	{
		m_updated = true;
	}
}
