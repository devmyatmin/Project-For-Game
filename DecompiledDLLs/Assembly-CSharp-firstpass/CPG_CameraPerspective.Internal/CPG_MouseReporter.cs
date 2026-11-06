using System;
using MyUtility;
using UnityEngine;

namespace CPG_CameraPerspective.Internal;

public class CPG_MouseReporter : MonoBehaviour
{
	[SerializeField]
	private CPG_CameraPerspectiveGizmo m_parent;

	[SerializeField]
	private CPG_CameraPerspectiveGizmo.EButtonTypes m_type;

	private void Start()
	{
		UtilityClickTouchDetector utilityClickTouchDetector = ((Component)this).gameObject.AddComponent<UtilityClickTouchDetector>();
		utilityClickTouchDetector.CameraInstance = m_parent.OwnCamera;
		utilityClickTouchDetector.m_onClick = (Action)Delegate.Combine(utilityClickTouchDetector.m_onClick, new Action(OnClicked));
	}

	private void OnClicked()
	{
		if ((Object)(object)m_parent != (Object)null)
		{
			m_parent.ReportClick(m_type);
		}
	}
}
