using System;
using Beam.Language;
using Beam.Serialization.Json;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using UnityEngine;

namespace Beam.Crafting;

[Serializable]
public class CraftingType : IEquatable<CraftingType>, ISaveable, ISerializationCallbackReceiver
{
	[NonSerialized]
	private AttributeType _attributeType;

	[NonSerialized]
	private InteractiveType _interactiveType;

	[SerializeField]
	[HideInInspector]
	private string _mTypeId;

	[SerializeField]
	[HideInInspector]
	private string _iTypeId;

	public static CraftingType Empty { get; } = new CraftingType(AttributeType.None, InteractiveType.None);


	public AttributeType AttributeType
	{
		get
		{
			return _attributeType;
		}
		set
		{
			_attributeType = value;
			_mTypeId = _attributeType.ToString();
		}
	}

	public InteractiveType InteractiveType
	{
		get
		{
			return _interactiveType;
		}
		set
		{
			_interactiveType = value;
			_iTypeId = _interactiveType.ToString();
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		_mTypeId = _attributeType.ToString();
		_iTypeId = _interactiveType.ToString();
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		try
		{
			_attributeType = (AttributeType)Enum.Parse(typeof(AttributeType), _mTypeId, ignoreCase: true);
		}
		catch (ArgumentException ex)
		{
			Debug.LogError("CraftingType:: Invalid or missing attribute type.\n" + ex.Message);
			_attributeType = AttributeType.INVALID;
		}
		try
		{
			_interactiveType = (InteractiveType)Enum.Parse(typeof(InteractiveType), _iTypeId, ignoreCase: true);
		}
		catch (ArgumentException ex2)
		{
			Debug.LogError("CraftingType:: Invalid or missing interactive type.\n" + ex2.Message);
			_interactiveType = InteractiveType.INVALID;
		}
	}

	private Color GetAttributeColor()
	{
		if (_attributeType != AttributeType.INVALID)
		{
			return GUI.color;
		}
		return Color.red;
	}

	private Color GetInteractiveColor()
	{
		if (_interactiveType != InteractiveType.INVALID)
		{
			return GUI.color;
		}
		return Color.red;
	}

	private Color GetAttributeBackingColor()
	{
		if (!(_mTypeId != _attributeType.ToString()))
		{
			return GUI.color;
		}
		return Color.red;
	}

	private Color GetInteractiveBackingColor()
	{
		if (!(_mTypeId != _attributeType.ToString()))
		{
			return GUI.color;
		}
		return Color.red;
	}

	public CraftingType(AttributeType attributeType, InteractiveType interactiveType)
	{
		AttributeType = attributeType;
		InteractiveType = interactiveType;
	}

	public override bool Equals(object obj)
	{
		CraftingType craftingType = obj as CraftingType;
		if (craftingType != null)
		{
			return Equals(craftingType);
		}
		return false;
	}

	public bool Equals(CraftingType craftingType)
	{
		if ((object)craftingType == null)
		{
			return false;
		}
		return Equals(craftingType.AttributeType, craftingType.InteractiveType);
	}

	public bool Equals(AttributeType attributeType, InteractiveType interactiveType)
	{
		if ((AttributeType == AttributeType.None && InteractiveType != 0) || (attributeType == AttributeType.None && interactiveType != 0))
		{
			return InteractiveType == interactiveType;
		}
		if ((AttributeType != 0 && InteractiveType == InteractiveType.None) || (attributeType != 0 && interactiveType == InteractiveType.None))
		{
			return AttributeType == attributeType;
		}
		if (AttributeType == attributeType)
		{
			return InteractiveType == interactiveType;
		}
		return false;
	}

	public static bool operator ==(CraftingType a, CraftingType b)
	{
		return a?.Equals(b) ?? ((object)b == null);
	}

	public static bool operator !=(CraftingType a, CraftingType b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		return (13 * 7 + AttributeType.GetHashCode()) * 7 + InteractiveType.GetHashCode();
	}

	public override string ToString()
	{
		string text = AttributeType.GetAttribute<DescriptionAttribute>().Text;
		string text2 = InteractiveType.GetAttribute<DescriptionAttribute>().Text;
		if (string.IsNullOrWhiteSpace(text2))
		{
			return text;
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			return text2;
		}
		return string_safemethods.Format(Localization.GetLanguageHandler().Localize("TYPE_SENTENCE"), text, text2);
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("IType", JSerializer.Serialize(InteractiveType));
		jObject.AddField("MType", JSerializer.Serialize(AttributeType));
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("IType");
		if (field != null)
		{
			try
			{
				InteractiveType = (InteractiveType)Enum.Parse(typeof(InteractiveType), field.GetValue<string>(), ignoreCase: true);
			}
			catch (ArgumentException)
			{
				InteractiveType = InteractiveType.INVALID;
			}
		}
		JObject field2 = data.GetField("MType");
		if (field2 != null)
		{
			try
			{
				AttributeType = (AttributeType)Enum.Parse(typeof(AttributeType), field2.GetValue<string>(), ignoreCase: true);
			}
			catch (ArgumentException)
			{
				AttributeType = AttributeType.INVALID;
			}
		}
	}
}
