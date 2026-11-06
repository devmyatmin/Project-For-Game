using Beam.UI;
using LE_LevelEditor.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LE_LevelEditor.UI;

public class LE_ObjectPrefabNode : MonoBehaviour
{
	public class SendMessageInitData
	{
		public readonly LE_Object m_object;

		public readonly int m_indentLevel;

		public SendMessageInitData(LE_Object p_object, int p_indentLevel)
		{
			m_object = p_object;
			m_indentLevel = p_indentLevel;
		}
	}

	[SerializeField]
	private TMPLabelViewAdapter m_text;

	[SerializeField]
	private RawImage m_image;

	[SerializeField]
	private Image m_selectionImage;

	public RawImage Image => m_image;

	public void uMyGUI_TreeBrowser_InitNode(object p_data)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected Obj, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_text != (Object)null && (Object)(object)m_image != (Object)null)
		{
			if (p_data is SendMessageInitData)
			{
				SendMessageInitData sendMessageInitData = (SendMessageInitData)p_data;
				Texture2D val = (Texture2D)Resources.Load("WorldEditor/ObjectIcons/" + ((Object)sendMessageInitData.m_object).name);
				if ((Object)(object)val != (Object)null)
				{
					m_image.texture = (Texture)(object)val;
				}
				else
				{
					Debug.LogError((object)("LE_ObjectPrefabNode: uMyGUI_TreeBrowser_InitNode: object '" + ((Object)sendMessageInitData.m_object).name));
				}
				m_text.Text = sendMessageInitData.m_object.EditorObjectName;
				Vector2 offsetMax = ((RectTransform)((Component)m_text).transform).offsetMax;
				offsetMax.x -= (float)sendMessageInitData.m_indentLevel * 10f;
				((RectTransform)((Component)m_text).transform).offsetMax = offsetMax;
			}
			else if (p_data is LE_ObjectCategoryNode.SendMessageInitData)
			{
				LE_ObjectCategoryNode.SendMessageInitData sendMessageInitData2 = (LE_ObjectCategoryNode.SendMessageInitData)p_data;
				((Component)m_image).gameObject.SetActive(false);
				m_text.Text = LE_ObjectCategoryNode.GetCleanCategoryName(sendMessageInitData2.m_categoryName);
			}
			else
			{
				Debug.LogError((object)("LE_ObjectPrefabNode: uMyGUI_TreeBrowser_InitNode: expected p_data to be a LE_ObjectPrefabNode.SendMessageInitData! p_data: " + p_data));
			}
		}
		else
		{
			Debug.LogError((object)"LE_ObjectPrefabNode: uMyGUI_TreeBrowser_InitNode: m_text or m_image were not set via inspector!");
		}
	}

	public void OnSelected()
	{
		LE_ObjectDragDrop componentInParent = ((Component)this).GetComponentInParent<LE_ObjectDragDrop>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			componentInParent.OnObjectSelected(this);
		}
		else
		{
			Debug.LogError((object)"LE_ObjectPrefabNode: OnSelected: could not find LE_ObjectDragDrop in parents!");
		}
		ShowSelection();
	}

	public void ShowSelection()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_selectionImage != (Object)null)
		{
			((Graphic)m_selectionImage).color = Color.green;
		}
		if ((Object)(object)m_text != (Object)null)
		{
			m_text.Color = Color.green;
		}
	}

	public void HideSelection()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_selectionImage != (Object)null)
		{
			((Graphic)m_selectionImage).color = Color.white;
		}
		if ((Object)(object)m_text != (Object)null)
		{
			m_text.Color = Color.white;
		}
	}
}
