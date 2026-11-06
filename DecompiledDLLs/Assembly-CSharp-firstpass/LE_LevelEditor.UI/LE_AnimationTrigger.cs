using System.Collections;
using UnityEngine;

namespace LE_LevelEditor.UI;

public class LE_AnimationTrigger : MonoBehaviour
{
	public enum ETriggerMode
	{
		ON_ENABLE,
		ON_DISABLE,
		ON_UMYGUI_ACTIVATETAB,
		ON_UMYGUI_DEACTIVATETAB,
		REDIRECT_ONMYGUI_EVENTS
	}

	[SerializeField]
	private Animation m_animation;

	[SerializeField]
	private string m_clipName;

	[SerializeField]
	private ETriggerMode m_condition;

	[SerializeField]
	private bool m_isActivateOnAnimStart;

	[SerializeField]
	private bool m_isDeactivateOnAnimEnd;

	[SerializeField]
	private MonoBehaviour m_alternativeCoroutineWorker;

	[SerializeField]
	private GameObject m_redirectDestination;

	private void OnEnable()
	{
		if (m_condition == ETriggerMode.ON_ENABLE)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		if (m_condition == ETriggerMode.ON_DISABLE)
		{
			Play();
		}
	}

	private void uMyGUI_OnActivateTab()
	{
		if (m_condition == ETriggerMode.ON_UMYGUI_ACTIVATETAB)
		{
			Play();
		}
		else if (m_condition == ETriggerMode.REDIRECT_ONMYGUI_EVENTS)
		{
			if ((Object)(object)m_redirectDestination == (Object)null)
			{
				Debug.LogError((object)"LE_AnimationTrigger: uMyGUI_OnActivateTab: REDIRECT_ONMYGUI_EVENTS mode requires m_redirectDestination to be set!");
			}
			else if (m_redirectDestination.activeInHierarchy)
			{
				m_redirectDestination.SendMessage("uMyGUI_OnActivateTab");
			}
		}
	}

	private void uMyGUI_OnDeactivateTab()
	{
		if (m_condition == ETriggerMode.ON_UMYGUI_DEACTIVATETAB)
		{
			Play();
		}
		else if (m_condition == ETriggerMode.REDIRECT_ONMYGUI_EVENTS)
		{
			if ((Object)(object)m_redirectDestination == (Object)null)
			{
				Debug.LogError((object)"LE_AnimationTrigger: uMyGUI_OnDeactivateTab: REDIRECT_ONMYGUI_EVENTS mode requires m_redirectDestination to be set!");
			}
			else if (m_redirectDestination.activeInHierarchy)
			{
				m_redirectDestination.SendMessage("uMyGUI_OnDeactivateTab");
			}
		}
	}

	private void Play()
	{
		if ((Object)(object)m_animation != (Object)null)
		{
			if (m_isActivateOnAnimStart)
			{
				((Component)m_animation).gameObject.SetActive(true);
			}
			if (m_isDeactivateOnAnimEnd && (TrackedReference)(object)m_animation[m_clipName] != (TrackedReference)null)
			{
				((MonoBehaviour)(((Object)(object)m_alternativeCoroutineWorker != (Object)null) ? ((LE_AnimationTrigger)(object)m_alternativeCoroutineWorker) : this)).StartCoroutine(DeactivateAfterDelay(((Component)m_animation).gameObject, m_animation[m_clipName].length));
			}
			m_animation.Play(m_clipName);
		}
		else
		{
			Debug.LogError((object)"LE_AnimationTrigger: OnDisable: lost reference to Animation!");
		}
	}

	private IEnumerator DeactivateAfterDelay(GameObject p_object, float p_delay)
	{
		yield return (object)new WaitForSeconds(p_delay);
		if ((Object)(object)p_object != (Object)null)
		{
			p_object.SetActive(false);
		}
	}
}
