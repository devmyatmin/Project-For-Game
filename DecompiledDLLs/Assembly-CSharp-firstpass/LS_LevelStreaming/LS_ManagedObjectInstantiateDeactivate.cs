using System;
using UnityEngine;

namespace LS_LevelStreaming;

public class LS_ManagedObjectInstantiateDeactivate : LS_ManagedObjectInstantiateDestroy
{
	public LS_ManagedObjectInstantiateDeactivate(string p_resourcePath, Vector3 p_position, Quaternion p_rotation, Vector3 p_scale, LS_ITriggerByUpdatedPosition p_trigger, Action<int, GameObject> p_onInstantiated, Action<int, GameObject> p_onDeactivated)
		: base(p_resourcePath, p_position, p_rotation, p_scale, p_trigger, p_onInstantiated, p_onDeactivated)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
	}

	protected override void Hide()
	{
		if ((Object)(object)m_instance != (Object)null)
		{
			m_instance.SetActive(false);
			if (m_onDestroyed != null)
			{
				m_onDestroyed(ID, m_instance);
			}
		}
	}

	protected override void Show()
	{
		if ((Object)(object)m_instance != (Object)null)
		{
			m_instance.SetActive(true);
			if (m_onInstantiated != null)
			{
				m_onInstantiated(ID, m_instance);
			}
		}
		else
		{
			base.Show();
		}
	}
}
