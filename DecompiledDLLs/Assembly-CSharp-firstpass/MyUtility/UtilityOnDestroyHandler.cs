using System;
using UnityEngine;

namespace MyUtility;

public class UtilityOnDestroyHandler : MonoBehaviour
{
	private bool m_isHandlingDisabled;

	public Action m_onDestroy;

	public void DestroyWithoutHandling()
	{
		m_isHandlingDisabled = true;
		m_onDestroy = null;
		Object.Destroy((Object)(object)this);
	}

	private void OnDestroy()
	{
		if (m_onDestroy != null)
		{
			m_onDestroy();
		}
		else if (!m_isHandlingDisabled)
		{
			Debug.LogError((object)"UtilityOnDestroyHandler: OnDestroy: destroy handler was not set!");
		}
	}
}
