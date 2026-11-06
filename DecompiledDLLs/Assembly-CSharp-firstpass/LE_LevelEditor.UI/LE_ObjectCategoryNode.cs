using Beam.UI;
using MyUtility;
using UnityEngine;

namespace LE_LevelEditor.UI;

public class LE_ObjectCategoryNode : MonoBehaviour
{
	public class SendMessageInitData
	{
		public readonly string m_categoryName;

		public readonly int m_indentLevel;

		public SendMessageInitData(string p_categoryName, int p_indentLevel)
		{
			m_categoryName = p_categoryName;
			m_indentLevel = p_indentLevel;
		}
	}

	[SerializeField]
	private TMPLabelViewAdapter m_text;

	public void uMyGUI_TreeBrowser_InitNode(object p_data)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_text != (Object)null)
		{
			if (p_data is SendMessageInitData)
			{
				SendMessageInitData sendMessageInitData = (SendMessageInitData)p_data;
				m_text.Text = sendMessageInitData.m_categoryName;
				Vector2 offsetMax = ((RectTransform)((Component)m_text).transform).offsetMax;
				offsetMax.x -= (float)sendMessageInitData.m_indentLevel * 10f;
				((RectTransform)((Component)m_text).transform).offsetMax = offsetMax;
			}
			else
			{
				Debug.LogError((object)("LE_ObjectCategoryNode: uMyGUI_TreeBrowser_InitNode: expected p_data to be a string! p_data: " + p_data));
			}
		}
		else
		{
			Debug.LogError((object)"LE_ObjectCategoryNode: uMyGUI_TreeBrowser_InitNode: m_text was not set via inspector!");
		}
	}

	public static string GetCleanCategoryName(string p_categoryName)
	{
		if (p_categoryName.Contains("_"))
		{
			return UtilityStrings.InsertSpacesIntoCamelCase(p_categoryName.Substring(0, p_categoryName.IndexOf("_")));
		}
		return UtilityStrings.InsertSpacesIntoCamelCase(p_categoryName);
	}
}
