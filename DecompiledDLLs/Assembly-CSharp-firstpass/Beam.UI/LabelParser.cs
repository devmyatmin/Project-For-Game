using System;
using Beam.Utilities.UGUI;
using UnityEngine;

namespace Beam.UI;

public class LabelParser : MonoBehaviour
{
	public struct XmlAttribute
	{
		public int NameHashCode;

		public int ValueHashCode;

		public int StartIndex;

		public int Length;
	}

	[SerializeField]
	private TMPLabelViewAdapter _label;

	private char[] _chars;

	private char[] _newChars;

	private char[] _tagChars;

	private int _newCharCount;

	private void Awake()
	{
		if ((Object)(object)_label != (Object)null)
		{
			_label.TextChanged += Label_TextChanged;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)_label != (Object)null)
		{
			_label.TextChanged -= Label_TextChanged;
		}
	}

	private void Label_TextChanged(ILabelViewAdapter sender, EventArgs args)
	{
		string text = sender.Text;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		InitializeCharBuffers(text.Length);
		FillCharBuffer(text, ref _chars);
		_newCharCount = 0;
		for (int i = 0; i < _chars.Length && _chars[i] != '\0'; i++)
		{
			if (_chars[i] == '<' && ParseXmlElements(_chars, i + 1, out var endIndex))
			{
				i = endIndex;
				continue;
			}
			ResizeCharBuffer(_newCharCount, ref _newChars);
			_newChars[_newCharCount] = _chars[i];
			_newCharCount++;
		}
		sender.Text = new string(_newChars);
	}

	private void InitializeCharBuffers(int length)
	{
		if (_chars == null || length > _chars.Length)
		{
			CreateCharBuffer(length, out _chars);
			CreateCharBuffer(length, out _newChars);
		}
		else
		{
			ClearCharBuffer(ref _chars);
			ClearCharBuffer(ref _newChars);
		}
		if (_tagChars == null)
		{
			CreateCharBuffer(32, out _tagChars);
		}
		else
		{
			ClearCharBuffer(ref _tagChars);
		}
	}

	private bool ParseXmlElements(char[] chars, int startIndex, out int endIndex)
	{
		bool flag = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		XmlAttribute xmlAttribute = default;
		endIndex = startIndex;
		for (int i = startIndex; i < chars.Length && chars[i] != '\0'; i++)
		{
			if (chars[i] == '>')
			{
				flag = true;
				_tagChars[num3] = '\0';
				endIndex = i;
				break;
			}
			_tagChars[num3] = chars[i];
			num3++;
			if (num == 1)
			{
				if (num2 == 0)
				{
					if (chars[i] == '"')
					{
						num2 = 1;
						xmlAttribute.StartIndex = num3;
					}
				}
				else if (chars[i] != '"')
				{
					xmlAttribute.ValueHashCode = ((xmlAttribute.ValueHashCode << 5) + xmlAttribute.ValueHashCode) ^ chars[i];
					xmlAttribute.Length++;
				}
			}
			else
			{
				xmlAttribute.NameHashCode = (xmlAttribute.NameHashCode << 3) - xmlAttribute.NameHashCode + chars[i];
			}
			if (chars[i] == '=')
			{
				num = 1;
			}
		}
		if (!flag)
		{
			return false;
		}
		int nameHashCode = xmlAttribute.NameHashCode;
		if (nameHashCode == 29790 || nameHashCode == 42558)
		{
			string text = string.Empty;
			switch (xmlAttribute.ValueHashCode)
			{
			case -247528402:
				text = RewiredUtilities.GetActionElementName(PlayerRegistry.LocalPlayer.Input, 13);
				break;
			case 105489225:
				text = RewiredUtilities.GetActionElementName(PlayerRegistry.LocalPlayer.Input, 11);
				break;
			case -1174368926:
				text = RewiredUtilities.GetActionElementName(PlayerRegistry.LocalPlayer.Input, 34);
				break;
			case -784466810:
				text = RewiredUtilities.GetActionElementName(PlayerRegistry.LocalPlayer.Input, 12);
				break;
			case 444442249:
				text = RewiredUtilities.GetActionElementName(PlayerRegistry.LocalPlayer.Input, 7);
				break;
			case -1691680381:
				text = RewiredUtilities.GetActionElementName(PlayerRegistry.LocalPlayer.Input, 10);
				break;
			case -363747654:
				text = RewiredUtilities.GetActionElementName(PlayerRegistry.LocalPlayer.Input, 8);
				break;
			}
			text = UGUITools.FormatColor(text, "FF9C00");
			InsertStringIntoCharBuffer(endIndex, text, ref _newChars);
			return true;
		}
		return false;
	}

	private void CreateCharBuffer(int length, out char[] buffer)
	{
		int num = ((length > 1024) ? (length + 256) : Mathf.NextPowerOfTwo(length + 1));
		buffer = new char[num];
	}

	private void ResizeCharBuffer(int length, ref char[] buffer)
	{
		if (length >= buffer.Length)
		{
			CreateCharBuffer(length, out var buffer2);
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer2[i] = buffer[i];
			}
			buffer = buffer2;
		}
	}

	private void FillCharBuffer(string text, ref char[] buffer)
	{
		if (string.IsNullOrEmpty(text))
		{
			buffer[0] = '\0';
			return;
		}
		int num = 0;
		for (int i = 0; i < text.Length; i++)
		{
			buffer[num] = text[i];
			num++;
		}
	}

	private void InsertStringIntoCharBuffer(int startIndex, string text, ref char[] buffer)
	{
		int length = text.Length;
		ResizeCharBuffer(startIndex + length, ref buffer);
		int num = 0;
		for (int i = startIndex; i < startIndex + length; i++)
		{
			buffer[_newCharCount] = text[num];
			_newCharCount++;
			num++;
		}
	}

	private void ClearCharBuffer(ref char[] buffer)
	{
		for (int i = 0; i < buffer.Length; i++)
		{
			buffer[i] = '\0';
		}
	}
}
