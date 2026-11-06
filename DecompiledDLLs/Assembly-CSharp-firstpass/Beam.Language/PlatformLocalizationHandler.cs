using System.Text;
using Beam.Utilities;
using Funlabs;
using Rewired;
using UnityEngine;

namespace Beam.Language;

public class PlatformLocalizationHandler : IPlatformLocalizationHandler
{
	private const string PLATFORM_TAG = "[PLATFORM]";

	private const string PLATFORM_COLOR_TAG_PARAMETER = "orange";

	[SerializeField]
	private ControllerGlyphs _glyphsAsset;

	private StringBuilder _builder = new StringBuilder();

	public PlatformLocalizationHandler(ControllerGlyphs glyphsAsset)
	{
		_glyphsAsset = glyphsAsset;
	}

	public string SubstituteControllerGlyphs(string text, int playerId, params int[] actions)
	{
		if (actions != null && actions.Length != 0 && actions[0] != -1)
		{
			for (int i = 0; i < actions.Length; i++)
			{
				if (!text.Contains("[PLATFORM]"))
				{
					break;
				}
				int rewiredActionId = actions[i];
				string glyph_RichText = GetGlyph_RichText(playerId, rewiredActionId);
				if (!string.IsNullOrEmpty(glyph_RichText))
				{
					text = StringTools.ReplaceFirst(text, "[PLATFORM]", glyph_RichText);
				}
			}
		}
		return text;
	}

	private string GetGlyph_RichText(int playerId, int rewiredActionId)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (rewiredActionId < 0)
		{
			return string.Empty;
		}
		InputAction action = ReInput.mapping.GetAction(rewiredActionId);
		if (action == null)
		{
			Debug.LogError((object)$"No action exists for id [{rewiredActionId}]");
			return string.Empty;
		}
		if (MPExtensions.IsMultiplayer(Game.Mode) && MPExtensions.IsClient(Game.Mode))
		{
			playerId = 0;
		}
		Player player = ReInput.players.GetPlayer(playerId);
		_builder.Clear();
		ActionElementMap val = null;
		if (RewiredUtilities.HasJoystick(player))
		{
			val = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)2, rewiredActionId, false);
		}
		if (val == null)
		{
			val = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)0, rewiredActionId, false);
			if (val == null || val.elementIdentifierName.Equals("None"))
			{
				val = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)1, rewiredActionId, false);
			}
		}
		int num = 0;
		if (val != null)
		{
			string value = null;
			if (val.controllerMap.controller.deviceInstanceGuid != ((Controller)ReInput.controllers.Keyboard).deviceInstanceGuid && val.controllerMap.controller.deviceInstanceGuid != ((Controller)ReInput.controllers.Mouse).deviceInstanceGuid)
			{
				string glyph_RichText = _glyphsAsset.GetGlyph_RichText(val.controllerMap.hardwareGuid, val.elementIdentifierId);
				if (!string.IsNullOrEmpty(glyph_RichText))
				{
					value = glyph_RichText;
				}
			}
			if (string.IsNullOrWhiteSpace(value))
			{
				value = "<b>" + val.elementIdentifierName + "</b>";
			}
			_builder.Append(value);
			if (num++ > 0)
			{
				_builder.Append(" ");
			}
		}
		if (num == 0)
		{
			Debug.LogError((object)$"No <b><i>enabled</i></b> action->element maps for [{action.id}/{action.name}]");
			return string.Empty;
		}
		return _builder.ToString();
	}
}
