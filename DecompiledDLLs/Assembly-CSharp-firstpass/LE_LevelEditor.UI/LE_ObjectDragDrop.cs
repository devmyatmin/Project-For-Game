using UnityEngine;
using UnityEngine.UI;

namespace LE_LevelEditor.UI;

public class LE_ObjectDragDrop : MonoBehaviour
{
	[SerializeField]
	private float MAX_IMAGE_SIZE = 150f;

	[SerializeField]
	private RectTransform SELECTED_OBJECT_TRANSFORM;

	[SerializeField]
	private RawImage SELECTED_OBJECT_IMAGE;

	private float m_selectedObjectTransformHeight;

	private LE_ObjectPrefabNode m_selectedNode;

	public void OnObjectSelected(LE_ObjectPrefabNode p_objectNode)
	{
		if (!((Object)(object)m_selectedNode != (Object)(object)p_objectNode))
		{
			return;
		}
		if ((Object)(object)m_selectedNode != (Object)null)
		{
			m_selectedNode.HideSelection();
		}
		m_selectedNode = p_objectNode;
		if ((Object)(object)SELECTED_OBJECT_TRANSFORM != (Object)null)
		{
			((Component)SELECTED_OBJECT_TRANSFORM).gameObject.SetActive(true);
			float num = Mathf.Min(MAX_IMAGE_SIZE, m_selectedObjectTransformHeight);
			SELECTED_OBJECT_TRANSFORM.SetSizeWithCurrentAnchors((Axis)0, num);
			SELECTED_OBJECT_TRANSFORM.SetSizeWithCurrentAnchors((Axis)1, num);
		}
		else
		{
			Debug.LogError((object)"LE_ObjectDragDrop: OnObjectSelected: SELECTED_OBJECT_TRANSFORM was not set in inspector!");
		}
		if ((Object)(object)SELECTED_OBJECT_IMAGE != (Object)null)
		{
			if ((Object)(object)m_selectedNode.Image != (Object)null)
			{
				SELECTED_OBJECT_IMAGE.texture = m_selectedNode.Image.texture;
			}
		}
		else
		{
			Debug.LogError((object)"LE_ObjectDragDrop: OnObjectSelected: SELECTED_OBJECT_IMAGE was not set in inspector!");
		}
	}

	private void Start()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)SELECTED_OBJECT_TRANSFORM != (Object)null)
		{
			Rect rect = SELECTED_OBJECT_TRANSFORM.rect;
			m_selectedObjectTransformHeight = rect.height;
		}
		else
		{
			Debug.LogError((object)"LE_ObjectDragDrop: OnObjectSelected: SELECTED_OBJECT_TRANSFORM was not set in inspector!");
		}
	}
}
