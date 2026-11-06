using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Beam.Serialization.Json;

public class JObject
{
	public enum EToken
	{
		NULL,
		NUMBER,
		STRING,
		BOOL,
		ARRAY,
		OBJECT
	}

	private static readonly char[] WHITESPACE = new char[4] { ' ', '\r', '\n', '\t' };

	private const string INFINITY = "\"INFINITY\"";

	private const string NEGATIVE_INFINITY = "\"NEGATIVE_INFINITY\"";

	private const string NAN = "\"NaN\"";

	private EToken _type;

	private List<string> _keys;

	private List<JObject> _children;

	private object _data;

	public EToken Type
	{
		get
		{
			return _type;
		}
		private set
		{
			_type = value;
		}
	}

	public IList<string> Keys => _keys;

	public IList<JObject> Children => _children;

	public object Data
	{
		get
		{
			return _data;
		}
		set
		{
			_data = value;
		}
	}

	public JObject this[string key]
	{
		get
		{
			return GetField(key);
		}
		set
		{
			SetField(key, value);
		}
	}

	public JObject()
	{
	}

	public JObject(string data)
	{
		Deserialize(data, -2);
	}

	public JObject(Action<JObject> data)
	{
		data(this);
	}

	public JObject(EToken type, object value)
	{
		_type = type;
		_data = value;
	}

	public JObject(object instance)
	{
		Type type = instance.GetType();
		if (type == typeof(bool))
		{
			_type = EToken.BOOL;
			_data = (bool)instance;
		}
		else if (type == typeof(char) || type == typeof(string) || type.IsEnum)
		{
			_type = EToken.STRING;
			_data = instance.ToString();
		}
		else if (Beam.ExtensionMethods.IsIntegralNumeric(type) || Beam.ExtensionMethods.IsFloatingPointNumeric(type))
		{
			_type = EToken.NUMBER;
			_data = instance;
		}
		else
		{
			Debug.LogError((object)("JObject:: Error creating new JObject of unsupported type [" + type.Name + "]"));
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		Serialize(stringBuilder, 0);
		return stringBuilder.ToString();
	}

	public bool IsNull()
	{
		if (_type != EToken.NULL)
		{
			return _children == null;
		}
		return true;
	}

	private void Deserialize(string data, int maxDepth)
	{
		if (string.IsNullOrEmpty(data))
		{
			_type = EToken.NULL;
			return;
		}
		data = data.Trim(WHITESPACE);
		if (data.Length > 0)
		{
			if (string.Compare(data, "true", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				_type = EToken.BOOL;
				_data = true;
				return;
			}
			if (string.Compare(data, "false", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				_type = EToken.BOOL;
				_data = false;
				return;
			}
			if (string.Compare(data, "null", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				_type = EToken.NULL;
				return;
			}
			switch (data)
			{
			case "\"INFINITY\"":
				_type = EToken.NUMBER;
				_data = float.PositiveInfinity;
				return;
			case "\"NEGATIVE_INFINITY\"":
				_type = EToken.NUMBER;
				_data = float.NegativeInfinity;
				return;
			case "\"NaN\"":
				_type = EToken.NUMBER;
				_data = float.NaN;
				return;
			}
			if (data[0] == '"')
			{
				_type = EToken.STRING;
				_data = data.Substring(1, data.Length - 2);
				return;
			}
			int num = 1;
			int num2 = 0;
			switch (data[num2])
			{
			case '{':
				_type = EToken.OBJECT;
				_keys = new List<string>();
				_children = new List<JObject>();
				break;
			case '[':
				_type = EToken.ARRAY;
				_children = new List<JObject>();
				break;
			case '-':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				_type = EToken.NUMBER;
				_data = data;
				break;
			default:
				_type = EToken.NULL;
				throw new JSerializerException("Improper JSON formatting: " + data);
			}
			string item = string.Empty;
			bool flag = false;
			bool flag2 = false;
			int num3 = 0;
			while (++num2 < data.Length)
			{
				if (Array.IndexOf(WHITESPACE, data[num2]) > -1)
				{
					continue;
				}
				if (data[num2] == '\\')
				{
					num2 += 2;
				}
				if (data[num2] == '"')
				{
					if (flag)
					{
						if (!flag2 && num3 == 0 && Type == EToken.OBJECT)
						{
							item = data.Substring(num + 1, num2 - num - 1);
						}
						flag = false;
					}
					else
					{
						if (num3 == 0 && Type == EToken.OBJECT)
						{
							num = num2;
						}
						flag = true;
					}
				}
				if (flag)
				{
					continue;
				}
				if (Type == EToken.OBJECT && num3 == 0 && data[num2] == ':')
				{
					num = num2 + 1;
					flag2 = true;
				}
				if (data[num2] == '[' || data[num2] == '{')
				{
					num3++;
				}
				else if (data[num2] == ']' || data[num2] == '}')
				{
					num3--;
				}
				if ((data[num2] != ',' || num3 != 0) && num3 >= 0)
				{
					continue;
				}
				flag2 = false;
				string inner = data.Substring(num, num2 - num).Trim(WHITESPACE);
				if (inner.Length > 0)
				{
					if (Type == EToken.OBJECT)
					{
						_keys.Add(item);
					}
					if (maxDepth != -1)
					{
						_children.Add(new JObject((JObject jo) =>
						{
							jo.Deserialize(inner, (maxDepth < -1) ? (-2) : (maxDepth - 1));
						}));
					}
				}
				num = num2 + 1;
			}
		}
		else
		{
			Type = EToken.NULL;
		}
	}

	public void Serialize(StringBuilder builder, int indent)
	{
		switch (_type)
		{
		case EToken.STRING:
			builder.AppendFormat(ExtensionMethods.EnglishCulture, "\"{0}\"", _data.ToString());
			break;
		case EToken.NUMBER:
		{
			float f = Convert.ToSingle(_data, ExtensionMethods.EnglishCulture);
			if (float.IsInfinity(f))
			{
				builder.Append("\"INFINITY\"");
			}
			else if (float.IsNegativeInfinity(f))
			{
				builder.Append("\"NEGATIVE_INFINITY\"");
			}
			else if (float.IsNaN(f))
			{
				builder.Append("\"NaN\"");
			}
			else
			{
				builder.AppendFormat(ExtensionMethods.EnglishCulture, "{0}", _data);
			}
			break;
		}
		case EToken.OBJECT:
			builder.Append("{\n");
			indent++;
			if (_children.Count > 0)
			{
				for (int i = 0; i < _children.Count; i++)
				{
					string arg = _keys[i];
					JObject jObject = _children[i];
					if (jObject != null)
					{
						DoIndents(builder, indent);
						builder.AppendFormat(ExtensionMethods.EnglishCulture, "\"{0}\": ", arg);
						jObject.Serialize(builder, indent);
						if (i != _children.Count - 1)
						{
							builder.Append(",");
						}
						builder.Append("\n");
					}
				}
				builder.Length--;
			}
			builder.Append("\n");
			indent--;
			DoIndents(builder, indent);
			builder.Append("}");
			break;
		case EToken.ARRAY:
			builder.Append("[\n");
			indent++;
			if (_children.Count > 0)
			{
				for (int j = 0; j < _children.Count; j++)
				{
					if ((bool)_children[j])
					{
						DoIndents(builder, indent);
						_children[j].Serialize(builder, indent);
						if (j != _children.Count - 1)
						{
							builder.Append(",");
						}
						builder.Append("\n");
					}
				}
				builder.Length--;
			}
			builder.Append("\n");
			indent--;
			DoIndents(builder, indent);
			builder.Append("]");
			break;
		case EToken.BOOL:
			builder.AppendFormat(ExtensionMethods.EnglishCulture, "{0}", ((bool)_data) ? "true" : "false");
			break;
		case EToken.NULL:
			builder.Append("null");
			break;
		}
	}

	private void DoIndents(StringBuilder builder, int indents)
	{
		for (int i = 0; i < indents; i++)
		{
			builder.Append("\t");
		}
	}

	public void Add(bool instance)
	{
		Add(new JObject
		{
			Type = EToken.BOOL,
			Data = instance
		});
	}

	public void Add(float instance)
	{
		Add(new JObject
		{
			Type = EToken.NUMBER,
			Data = instance
		});
	}

	public void Add(string instance)
	{
		Add(new JObject
		{
			Type = EToken.STRING,
			Data = instance
		});
	}

	public void Add(JObject data)
	{
		if (data != null)
		{
			if (_type != EToken.ARRAY)
			{
				_type = EToken.ARRAY;
				_children = new List<JObject>();
			}
			_children.Add(data);
		}
	}

	public void AddField(string name, bool instance)
	{
		AddField(name, new JObject
		{
			Type = EToken.BOOL,
			Data = instance
		});
	}

	public void AddField(string name, int instance)
	{
		AddField(name, new JObject
		{
			Type = EToken.NUMBER,
			Data = instance
		});
	}

	public void AddField(string name, float instance)
	{
		AddField(name, new JObject
		{
			Type = EToken.NUMBER,
			Data = instance
		});
	}

	public void AddField(string name, string instance)
	{
		AddField(name, new JObject
		{
			Type = EToken.STRING,
			Data = instance
		});
	}

	public void AddField(string name, JObject data)
	{
		if (!data)
		{
			return;
		}
		if (_type != EToken.OBJECT)
		{
			if (_keys == null)
			{
				_keys = new List<string>();
			}
			if (Type == EToken.ARRAY)
			{
				for (int i = 0; i < _children.Count; i++)
				{
					_keys.Add(i.ToString(ExtensionMethods.EnglishCulture));
				}
			}
			else if (_children == null)
			{
				_children = new List<JObject>();
			}
			_type = EToken.OBJECT;
		}
		_keys.Add(name);
		_children.Add(data);
	}

	public JObject GetField(string name)
	{
		if (_type == EToken.OBJECT)
		{
			for (int i = 0; i < _keys.Count; i++)
			{
				if (_keys[i].Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return _children[i];
				}
			}
		}
		return null;
	}

	public void SetField(string index, JObject data)
	{
		JObject field = GetField(index);
		if ((bool)field)
		{
			_children.Remove(field);
			_keys.Remove(index);
		}
		AddField(index, data);
	}

	public void Clear()
	{
		_type = EToken.NULL;
		if (_children != null)
		{
			_children.Clear();
		}
		if (_keys != null)
		{
			_keys.Clear();
		}
		_data = null;
	}

	public static implicit operator bool(JObject data)
	{
		return data != null;
	}
}
