using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Beam.Language;

public class ControllerGlyphs : SerializedScriptableObject
{
	private static readonly string DEBUG_HEADER = "<color=cyan>ControllerGlyphs::</color> ";

	private static readonly Guid UNKNOWN_GUID = Guid.Empty;

	private static readonly Guid XB1_GUID = new Guid("19002688-7406-4f4a-8340-8d25335406c8");

	[Header("Rewired Controller Elements->Glyph Names")]
	[Header(".csv")]
	public string _elementIdsCsv;

	[Header("Glyph Assets Folder")]
	public string _glyphAssetsFolder;

	[NonSerialized]
	[Space]
	[OdinSerialize]
	public List<ControllerGlyphSet> _controllerGlyphSets;

	public string GetGlyph_RichText(Guid controllerGuid, int controllerElementId)
	{
		ControllerGlyphSet glyphSet = GetGlyphSet(controllerGuid);
		if (glyphSet == null)
		{
			glyphSet = GetGlyphSet(XB1_GUID);
			if (glyphSet == null)
			{
				Debug.LogError((object)(DEBUG_HEADER + $"Controller entry for guid [{XB1_GUID}] not found."));
				return string.Empty;
			}
		}
		string text = "";
		for (int i = 0; i < glyphSet.ElementGlyphMaps.Count; i++)
		{
			ElementGlyphMap elementGlyphMap = glyphSet.ElementGlyphMaps[i];
			if (elementGlyphMap.ElementId == controllerElementId)
			{
				text = elementGlyphMap.SpriteText;
				break;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError((object)(DEBUG_HEADER + $"Controller element id [{controllerElementId}] key not found for controller guid [{controllerGuid}]"));
			return string.Empty;
		}
		return text;
	}

	private ControllerGlyphSet GetGlyphSet(Guid guid)
	{
		for (int i = 0; i < _controllerGlyphSets.Count; i++)
		{
			ControllerGlyphSet controllerGlyphSet = _controllerGlyphSets[i];
			if (controllerGlyphSet.Guid == guid)
			{
				return controllerGlyphSet;
			}
		}
		return null;
	}

	private string GetPlatformAssetPrefix(string controllerName)
	{
		controllerName = controllerName.ToUpper();
		if (controllerName.Contains("XBOX 360"))
		{
			return "XB360";
		}
		if (controllerName.Contains("XBOX ONE"))
		{
			return "XB1";
		}
		if (controllerName.Contains("DUALSHOCK 4"))
		{
			return "DS4";
		}
		if (controllerName.Contains("DUALSHOCK 3"))
		{
			return "DS3";
		}
		if (controllerName.Contains("STEAM"))
		{
			return "STEAM";
		}
		return string.Empty;
	}
}
