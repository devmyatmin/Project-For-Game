using System;
using UnityEngine;

namespace LS_LevelStreaming;

public class LS_ManagedObjectInstantiateDestroy : LS_ManagedObjectBase
{
	protected readonly string m_resourcePath;

	protected Vector3 m_position;

	protected Quaternion m_rotation;

	protected Vector3 m_scale;

	protected readonly LS_ITriggerByUpdatedPosition m_updatedTrigger;

	protected readonly Action<int, GameObject> m_onInstantiated;

	protected readonly Action<int, GameObject> m_onDestroyed;

	protected GameObject m_instance;

	public GameObject Instance => m_instance;

	public LS_ManagedObjectInstantiateDestroy(string p_resourcePath, Vector3 p_position, Quaternion p_rotation, Vector3 p_scale, LS_ITriggerByUpdatedPosition p_trigger, Action<int, GameObject> p_onInstantiated, Action<int, GameObject> p_onDestroyed)
		: base(p_trigger)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		m_updatedTrigger = p_trigger;
		m_resourcePath = p_resourcePath;
		m_position = p_position;
		m_rotation = p_rotation;
		m_scale = p_scale;
		m_onInstantiated = p_onInstantiated;
		m_onDestroyed = p_onDestroyed;
		m_isVisible = false;
	}

	public override void Update()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		m_updatedTrigger.Update(((Object)(object)m_instance == (Object)null) ? m_position : m_instance.transform.position);
		base.Update();
		if (m_isVisible && (Object)(object)m_instance == (Object)null)
		{
			LS_LevelStreamingSceneManager.Instance.RemoveManagedObjectAtFrameEnd(ID);
		}
	}

	protected override void Hide()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_instance != (Object)null)
		{
			m_position = m_instance.transform.position;
			m_rotation = m_instance.transform.rotation;
			m_scale = m_instance.transform.localScale;
			Object.Destroy((Object)(object)m_instance);
			if (m_onDestroyed != null)
			{
				m_onDestroyed(ID, m_instance);
			}
			m_instance = null;
		}
		else
		{
			Debug.LogError((object)"LS_ManagedObjectInstantiateDestroy: Hide: object instance is already destroyed!");
		}
	}

	protected override void Show()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected Obj, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_instance != (Object)null)
		{
			Object.Destroy((Object)(object)m_instance);
			Debug.LogError((object)"LS_ManagedObjectInstantiateDestroy: Show: object instance was already instantiated!");
		}
		Object val = Resources.Load(m_resourcePath);
		if (val != (Object)null)
		{
			m_instance = (GameObject)Object.Instantiate(val, m_position, m_rotation);
			m_instance.transform.localScale = m_scale;
			if (m_onInstantiated != null)
			{
				m_onInstantiated(ID, m_instance);
			}
		}
		else
		{
			Debug.LogError((object)("LS_ManagedObjectInstantiateDestroy: Show: object prefab not found in resources at '" + m_resourcePath + "'!"));
		}
	}
}
