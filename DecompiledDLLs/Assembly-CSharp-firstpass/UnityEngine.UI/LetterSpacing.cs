using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;

namespace UnityEngine.UI;

[AddComponentMenu("UI/Effects/Letter Spacing", 15)]
public class LetterSpacing : BaseMeshEffect
{
	private const string SupportedTagRegexPattersn = "<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>";

	[SerializeField]
	private bool useRichText;

	[SerializeField]
	private float m_spacing;

	public float spacing
	{
		get
		{
			return m_spacing;
		}
		set
		{
			if (m_spacing != value)
			{
				m_spacing = value;
				if ((Object)(object)((BaseMeshEffect)this).graphic != (Object)null)
				{
					((BaseMeshEffect)this).graphic.SetVerticesDirty();
				}
			}
		}
	}

	protected LetterSpacing()
	{
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (((UIBehaviour)this).IsActive())
		{
			List<UIVertex> list = new List<UIVertex>();
			vh.GetUIVertexStream(list);
			ModifyVertices(list);
			vh.Clear();
			vh.AddUIVertexTriangleStream(list);
		}
	}

	public void ModifyVertices(List<UIVertex> verts)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected I4, but got Unknown
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		if (!((UIBehaviour)this).IsActive())
		{
			return;
		}
		Text component = ((Component)this).GetComponent<Text>();
		string text = component.text;
		IList<UILineInfo> lines = component.cachedTextGenerator.lines;
		for (int num = lines.Count - 1; num > 0; num--)
		{
			text = text.Insert(lines[num].startCharIdx, "\n");
			text = text.Remove(lines[num].startCharIdx - 1, 1);
		}
		string[] array = text.Split('\n');
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogWarning((object)"LetterSpacing: Missing Text component");
			return;
		}
		float num2 = spacing * (float)component.fontSize / 100f;
		float num3 = 0f;
		int num4 = 0;
		bool flag = useRichText && component.supportRichText;
		IEnumerator enumerator = null;
		Match match = null;
		TextAnchor alignment = component.alignment;
		switch ((int)alignment)
		{
		case 0:
		case 3:
		case 6:
			num3 = 0f;
			break;
		case 1:
		case 4:
		case 7:
			num3 = 0.5f;
			break;
		case 2:
		case 5:
		case 8:
			num3 = 1f;
			break;
		}
		foreach (string text2 in array)
		{
			int lineLengthWithoutTags = text2.Length;
			if (flag)
			{
				enumerator = GetRegexMatchedTagCollection(text2, out lineLengthWithoutTags);
				match = null;
				if (enumerator.MoveNext())
				{
					match = (Match)enumerator.Current;
				}
			}
			float num5 = (float)(lineLengthWithoutTags - 1) * num2 * num3;
			int num6 = 0;
			int num7 = 0;
			while (num6 < text2.Length)
			{
				if (flag && match != null && match.Index == num6)
				{
					num6 += match.Length - 1;
					num7--;
					num4 += match.Length;
					match = null;
					if (enumerator.MoveNext())
					{
						match = (Match)enumerator.Current;
					}
				}
				else
				{
					int index = num4 * 6;
					int index2 = num4 * 6 + 1;
					int index3 = num4 * 6 + 2;
					int index4 = num4 * 6 + 3;
					int index5 = num4 * 6 + 4;
					int num8 = num4 * 6 + 5;
					if (num8 > verts.Count - 1)
					{
						return;
					}
					UIVertex value = verts[index];
					UIVertex value2 = verts[index2];
					UIVertex value3 = verts[index3];
					UIVertex value4 = verts[index4];
					UIVertex value5 = verts[index5];
					UIVertex value6 = verts[num8];
					Vector3 val = Vector3.right * (num2 * (float)num7 - num5);
					ref Vector3 position = ref value.position;
					position += val;
					ref Vector3 position2 = ref value2.position;
					position2 += val;
					ref Vector3 position3 = ref value3.position;
					position3 += val;
					ref Vector3 position4 = ref value4.position;
					position4 += val;
					ref Vector3 position5 = ref value5.position;
					position5 += val;
					ref Vector3 position6 = ref value6.position;
					position6 += val;
					verts[index] = value;
					verts[index2] = value2;
					verts[index3] = value3;
					verts[index4] = value4;
					verts[index5] = value5;
					verts[num8] = value6;
					num4++;
				}
				num6++;
				num7++;
			}
			num4++;
		}
	}

	private IEnumerator GetRegexMatchedTagCollection(string line, out int lineLengthWithoutTags)
	{
		MatchCollection matchCollection = Regex.Matches(line, "<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>");
		lineLengthWithoutTags = 0;
		int num = 0;
		if (matchCollection.Count > 0)
		{
			foreach (Match item in matchCollection)
			{
				num += item.Length;
			}
		}
		lineLengthWithoutTags = line.Length - num;
		return matchCollection.GetEnumerator();
	}
}
