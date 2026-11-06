using UnityEngine;

namespace LS_LevelStreaming;

public class LS_DistanceTrigger : LS_ITrigger
{
	private int m_updateFrequency;

	private Transform m_playerOrCameraTransform;

	private Transform m_triggeredObjectTransform;

	private float m_spawnDistance;

	private float m_despawnDistance;

	private bool m_isVisible;

	private int m_updateCounter;

	public LS_DistanceTrigger(int p_updateFrequency, Transform p_playerOrCameraTransform, Transform p_triggeredObjectTransform, float p_spawnDistance, float p_despawnDistance)
	{
		m_updateFrequency = p_updateFrequency;
		if (m_updateFrequency > 0)
		{
			m_updateCounter = Random.Range(0, m_updateFrequency);
		}
		m_playerOrCameraTransform = p_playerOrCameraTransform;
		m_triggeredObjectTransform = p_triggeredObjectTransform;
		m_spawnDistance = p_spawnDistance;
		m_despawnDistance = p_despawnDistance;
		if ((Object)(object)m_triggeredObjectTransform != (Object)null)
		{
			m_isVisible = ((Component)m_triggeredObjectTransform).gameObject.activeSelf;
		}
	}

	bool LS_ITrigger.IsVisible()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (m_updateCounter <= 0)
		{
			m_updateCounter = m_updateFrequency;
			if ((Object)(object)m_playerOrCameraTransform != (Object)null && (Object)(object)m_triggeredObjectTransform != (Object)null)
			{
				m_isVisible = ((Component)m_triggeredObjectTransform).gameObject.activeSelf;
				Vector3 val = m_playerOrCameraTransform.position - m_triggeredObjectTransform.position;
				float magnitude = val.magnitude;
				if (m_spawnDistance >= magnitude)
				{
					m_isVisible = true;
				}
				else if (m_despawnDistance <= magnitude)
				{
					m_isVisible = false;
				}
			}
		}
		else
		{
			m_updateCounter--;
		}
		return m_isVisible;
	}
}
