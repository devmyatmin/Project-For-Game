using UnityEngine;

namespace Beam.Language;

public class NullPlatformLocalizationHandler : NullObject<NullPlatformLocalizationHandler>, IPlatformLocalizationHandler
{
	public string SubstituteControllerGlyphs(string text, int playerId, params int[] actions)
	{
		Debug.LogError((object)"NullPlatformLocalizationHandler:: No localization.");
		return text;
	}
}
