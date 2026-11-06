using LE_LevelEditor.LEInput;
using UnityEngine;

namespace LE_LevelEditor.UI;

public abstract class LE_GUI3dBase : MonoBehaviour, LE_IInputHandler
{
	protected Ray m_cursorRay;

	protected RaycastHit m_cursorHitInfo;

	private bool m_isCursorOverSomething;

	private int m_inactiveFrame = -1;

	protected Vector3 m_cursorScreenCoords = -1f * Vector3.zero;

	public bool IsCursorOverSomething
	{
		get
		{
			CheckCursor();
			return m_isCursorOverSomething;
		}
	}

	public bool IsInteractable
	{
		get
		{
			return m_inactiveFrame + 1 < Time.frameCount;
		}
		set
		{
			if (value)
			{
				m_inactiveFrame = -1;
			}
			else
			{
				m_inactiveFrame = Time.frameCount;
			}
		}
	}

	public Vector3 CursorScreenCoords
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_cursorScreenCoords;
		}
	}

	public abstract LE_EEditMode ActiveEditMode { get; }

	public void SetIsCursorOverSomething(bool p_isHit)
	{
		m_isCursorOverSomething = p_isHit;
	}

	public virtual void SetCursorPosition(Vector3 p_cursorScreenCoords)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		m_cursorScreenCoords = p_cursorScreenCoords;
		m_cursorRay = Camera.main.ScreenPointToRay(p_cursorScreenCoords);
		m_isCursorOverSomething = Physics.Raycast(m_cursorRay, ref m_cursorHitInfo, float.PositiveInfinity, -5, (QueryTriggerInteraction)1);
	}

	public virtual void SetIsCursorAction(bool p_isCursorAction)
	{
	}

	public virtual void MoveCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords, bool scrolling)
	{
	}

	public virtual void RotateCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
	{
	}

	public virtual void RotateCameraAroundPivot(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
	{
	}

	private void CheckCursor()
	{
		if (m_isCursorOverSomething && ((Object)(object)m_cursorHitInfo.collider == (Object)null || (Object)(object)m_cursorHitInfo.transform == (Object)null))
		{
			m_isCursorOverSomething = false;
		}
	}

	protected LE_GUI3dBase()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
	}
}
