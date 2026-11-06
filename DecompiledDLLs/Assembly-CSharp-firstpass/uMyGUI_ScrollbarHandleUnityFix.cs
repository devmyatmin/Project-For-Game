using UnityEngine;

public class uMyGUI_ScrollbarHandleUnityFix : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_anchorMin = new Vector2(0.8f, 0f);

	[SerializeField]
	private Vector2 m_anchorMax = new Vector2(1f, 1f);

	[SerializeField]
	private Vector2 m_pivot = new Vector2(0.5f, 0.5f);

	[SerializeField]
	private Vector2 m_offsetMin = new Vector2(-10f, -10f);

	[SerializeField]
	private Vector2 m_offsetMax = new Vector2(10f, 10f);

	public void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		RectTransform component = ((Component)this).GetComponent<RectTransform>();
		((Transform)component).localPosition = Vector3.zero;
		component.anchoredPosition3D = Vector3.zero;
		component.anchorMin = m_anchorMin;
		component.anchorMax = m_anchorMax;
		component.pivot = m_pivot;
		component.offsetMin = m_offsetMin;
		component.offsetMax = m_offsetMax;
	}

	public uMyGUI_ScrollbarHandleUnityFix()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
	}
}
